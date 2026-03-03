using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OnTopReplica.MessagePumpProcessors;
using OnTopReplica.Native;

namespace OnTopReplica.MultiWindow {

    /// <summary>
    /// Manages multiple monitored windows. Coordinates:
    /// - Primary window selection (displayed in ThumbnailPanel preview)
    /// - Region synchronization (all windows use the primary window's region)
    /// - Independent color detection (own enabled categories, not tied to ColorDetectionProcessor)
    /// - Icon/graphic template detection with disappearance alarm
    /// </summary>
    class MultiWindowManager : IDisposable {

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height,
            IntPtr hdcSrc, int xSrc, int ySrc, uint rop);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        private const uint SRCCOPY = 0x00CC0020;
        private const uint PW_RENDERFULLCONTENT = 2;
        private const uint PW_CLIENTONLY = 1;

        private readonly List<MonitoredWindow> _windows = new List<MonitoredWindow>();
        private volatile bool _detectionRunning = false;
        private System.Threading.Thread _detectionThread;
        private int _sampleInterval = 1000; // ms between scan rounds

        // Independent color detection settings
        private readonly HashSet<ColorCategory> _enabledCategories = new HashSet<ColorCategory>();
        private bool _colorDetectionEnabled = false;

        // Icon/graphic template detection
        private Bitmap _iconTemplate = null;
        private byte[] _iconTemplatePixels = null;
        private int _iconTemplateW = 0;
        private int _iconTemplateH = 0;
        private int _iconTemplateStride = 0;
        private bool _iconDetectionEnabled = false;
        private float _iconMatchThreshold = 0.82f; // 82% similarity required (lowered from 0.85 to reduce false negatives on scaled windows)
        // Normalization size: the ThumbnailPanel display size at template capture time.
        // Captured window regions are resized to this before matching to correct scale mismatch.
        private int _iconCaptureNormalizeW = 0;
        private int _iconCaptureNormalizeH = 0;
        private volatile bool _iconAlarmActive = false;

        // Sound for icon disappearance alarm  
        private string _alarmSoundFile = string.Empty;
        private float _alarmVolume = 1.0f;
        private System.Windows.Media.MediaPlayer _mediaPlayer;
        private System.Windows.Threading.Dispatcher _uiDispatcher;
        private System.Threading.Timer _alarmStopTimer;
        private const int AlarmDuration = 5000;

        /// <summary>
        /// Reference to the MainForm (set when enabled).
        /// </summary>
        public MainForm Form { get; set; }

        /// <summary>
        /// Gets all monitored windows.
        /// </summary>
        public IReadOnlyList<MonitoredWindow> Windows {
            get { return _windows; }
        }

        /// <summary>
        /// Gets the primary window (the one shown in ThumbnailPanel).
        /// </summary>
        public MonitoredWindow PrimaryWindow {
            get { return _windows.FirstOrDefault(w => w.IsPrimary); }
        }

        /// <summary>
        /// Gets whether multi-window monitoring is active (detection thread is running).
        /// </summary>
        public bool IsActive {
            get { return _detectionRunning; }
        }

        #region Independent Color Detection Settings

        /// <summary>
        /// Gets the independently managed set of enabled color categories.
        /// </summary>
        public HashSet<ColorCategory> EnabledCategories {
            get { return _enabledCategories; }
        }

        /// <summary>
        /// Gets or sets whether color detection is enabled for multi-window monitoring.
        /// </summary>
        public bool ColorDetectionEnabled {
            get { return _colorDetectionEnabled; }
            set { _colorDetectionEnabled = value; }
        }

        /// <summary>
        /// Sets a color category enabled or disabled.
        /// </summary>
        public void SetCategoryEnabled(ColorCategory category, bool enabled) {
            if (enabled)
                _enabledCategories.Add(category);
            else
                _enabledCategories.Remove(category);
        }

        #endregion

        #region Icon Template Detection Settings

        /// <summary>
        /// Gets or sets whether icon/graphic template detection is enabled.
        /// </summary>
        public bool IconDetectionEnabled {
            get { return _iconDetectionEnabled; }
            set { _iconDetectionEnabled = value; }
        }

        /// <summary>
        /// Gets the current icon template bitmap (for display in UI).
        /// </summary>
        public Bitmap IconTemplate {
            get { return _iconTemplate; }
        }

        /// <summary>
        /// Gets or sets the match threshold (0.0-1.0). Higher = stricter matching.
        /// </summary>
        public float IconMatchThreshold {
            get { return _iconMatchThreshold; }
            set { _iconMatchThreshold = Math.Max(0.5f, Math.Min(1.0f, value)); }
        }

        /// <summary>
        /// Gets or sets the alarm sound file path.
        /// </summary>
        public string AlarmSoundFile {
            get { return _alarmSoundFile; }
            set { _alarmSoundFile = value ?? string.Empty; }
        }

        /// <summary>
        /// Gets or sets the alarm volume (0.0 to 1.0).
        /// </summary>
        public float AlarmVolume {
            get { return _alarmVolume; }
            set { _alarmVolume = Math.Max(0, Math.Min(1, value)); }
        }

        /// <summary>
        /// Sets the icon template from a bitmap. Pre-computes pixel data for fast matching.
        /// <param name="normalizeW">ThumbnailPanel display width at capture time (0 = no normalization).</param>
        /// <param name="normalizeH">ThumbnailPanel display height at capture time (0 = no normalization).</param>
        /// </summary>
        public void SetIconTemplate(Bitmap template, int normalizeW = 0, int normalizeH = 0) {
            _iconCaptureNormalizeW = normalizeW;
            _iconCaptureNormalizeH = normalizeH;
            if (_iconTemplate != null) {
                _iconTemplate.Dispose();
                _iconTemplate = null;
            }
            _iconTemplatePixels = null;

            if (template == null || template.Width <= 0 || template.Height <= 0)
                return;

            _iconTemplate = new Bitmap(template);
            _iconTemplateW = _iconTemplate.Width;
            _iconTemplateH = _iconTemplate.Height;

            // Pre-compute pixel array for fast matching
            BitmapData data = null;
            try {
                data = _iconTemplate.LockBits(
                    new Rectangle(0, 0, _iconTemplateW, _iconTemplateH),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                _iconTemplateStride = data.Stride;
                int byteCount = _iconTemplateStride * _iconTemplateH;
                _iconTemplatePixels = new byte[byteCount];
                Marshal.Copy(data.Scan0, _iconTemplatePixels, 0, byteCount);
            }
            finally {
                if (data != null) _iconTemplate.UnlockBits(data);
            }

            Log.Write("MultiWindowManager: Icon template set, {0}x{1}, normalizeTarget={2}x{3}",
                _iconTemplateW, _iconTemplateH, normalizeW, normalizeH);
        }

        #endregion

        /// <summary>
        /// Captures the UI dispatcher for sound playback (call on UI thread).
        /// </summary>
        public void CaptureUIDispatcher() {
            _uiDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        /// Fired when any window's color detection triggers an alarm.
        /// </summary>
        public event Action<MonitoredWindow> AlarmTriggered;

        /// <summary>
        /// Fired when the icon disappears from ALL monitored windows.
        /// </summary>
        public event Action IconDisappearedAlarm;

        /// <summary>
        /// Fired when the monitored window list changes.
        /// </summary>
        public event Action WindowListChanged;

        /// <summary>
        /// Adds a window to the monitored list.
        /// </summary>
        public void AddWindow(WindowHandle handle) {
            if (handle == null) return;
            if (_windows.Any(w => w.WindowHandle.Handle == handle.Handle))
                return;

            var mw = new MonitoredWindow(handle);
            if (_windows.Count == 0)
                mw.IsPrimary = true;

            _windows.Add(mw);
            Log.Write("MultiWindowManager: Added window '{0}' (handle={1}), total={2}",
                mw.Title, handle.Handle, _windows.Count);

            WindowListChanged?.Invoke();
        }

        /// <summary>
        /// Removes a window from the monitored list.
        /// </summary>
        public void RemoveWindow(WindowHandle handle) {
            if (handle == null) return;
            var mw = _windows.FirstOrDefault(w => w.WindowHandle.Handle == handle.Handle);
            if (mw == null) return;

            bool wasPrimary = mw.IsPrimary;
            _windows.Remove(mw);

            if (wasPrimary && _windows.Count > 0) {
                _windows[0].IsPrimary = true;
                SwitchPrimaryToThumbnail();
            }

            Log.Write("MultiWindowManager: Removed window '{0}', total={1}", mw.Title, _windows.Count);
            WindowListChanged?.Invoke();
        }

        /// <summary>
        /// Sets a specific window as the primary (preview) window.
        /// </summary>
        public void SetPrimary(WindowHandle handle) {
            foreach (var w in _windows) {
                w.IsPrimary = (w.WindowHandle.Handle == handle.Handle);
            }

            SwitchPrimaryToThumbnail();
            WindowListChanged?.Invoke();
        }

        private void SwitchPrimaryToThumbnail() {
            var primary = PrimaryWindow;
            if (primary == null || Form == null) return;

            try {
                Form.SetThumbnail(primary.WindowHandle, null);
                var region = Form.SelectedThumbnailRegion;
                if (region != null) {
                    Form.SelectedThumbnailRegion = region;
                }
                Log.Write("MultiWindowManager: Switched primary to '{0}'", primary.Title);
            }
            catch (Exception ex) {
                Log.Write("MultiWindowManager: Failed to switch primary: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Clears all monitored windows and stops detection.
        /// </summary>
        public void Clear() {
            StopDetection();
            _windows.Clear();
            WindowListChanged?.Invoke();
        }

        #region Detection Thread

        /// <summary>
        /// Starts background detection for all monitored windows.
        /// </summary>
        public void StartDetection() {
            if (_detectionThread != null && _detectionThread.IsAlive)
                return;

            _detectionRunning = true;
            _detectionThread = new System.Threading.Thread(DetectionLoop) {
                IsBackground = true,
                Name = "MultiWindowDetection"
            };
            _detectionThread.Start();
            Log.Write("MultiWindowManager: Detection started, windows={0}, color={1}, icon={2}",
                _windows.Count, _colorDetectionEnabled, _iconDetectionEnabled);
        }

        /// <summary>
        /// Stops background detection.
        /// </summary>
        public void StopDetection() {
            _detectionRunning = false;
            StopAlarm();
            Log.Write("MultiWindowManager: Detection stop requested");
        }

        /// <summary>
        /// Background detection loop.
        /// </summary>
        private void DetectionLoop() {
            while (_detectionRunning) {
                System.Threading.Thread.Sleep(_sampleInterval);
                if (!_detectionRunning) break;

                Rectangle regionRect = GetCurrentRegion();

                bool anyColorEnabled = _colorDetectionEnabled && _enabledCategories.Count > 0;
                bool anyIconEnabled = _iconDetectionEnabled && _iconTemplatePixels != null;

                if (!anyColorEnabled && !anyIconEnabled) continue;

                // Track icon presence across ALL windows for disappearance alarm.
                // Short-circuit optimization: once any window reports icon found,
                // remaining windows skip icon detection (no alarm can trigger).
                bool allWindowsLostIcon = anyIconEnabled;
                bool needIconCheck = anyIconEnabled; // set false as soon as icon is found
                int iconCheckedCount = 0;

                foreach (var mw in _windows.ToArray()) {
                    if (!_detectionRunning) break;

                    if (!IsWindow(mw.WindowHandle.Handle)) {
                        Log.Write("MultiWindowManager: Window '{0}' no longer valid, removing", mw.Title);
                        _windows.Remove(mw);
                        WindowListChanged?.Invoke();
                        continue;
                    }

                    // If only icon detection is enabled and we already found it,
                    // skip capturing this window entirely — no need to allocate bitmap.
                    bool needCapture = anyColorEnabled || needIconCheck;
                    if (!needCapture) {
                        mw.LastIconDetected = true; // assumed present (we already found it in an earlier window)
                        continue;
                    }

                    try {
                        // Capture the region bitmap once for both checks
                        Bitmap regionBmp = CaptureWindowRegion(mw.WindowHandle.Handle, regionRect);
                        if (regionBmp == null) continue;

                        try {
                            // --- Color detection ---
                            if (anyColorEnabled && mw.IsColorDetectionEnabled) {
                                bool colorMatch = ScanBitmapForColor(regionBmp, _enabledCategories);
                                mw.LastDetectionResult = colorMatch;
                                mw.LastDetectionTime = DateTime.Now;

                                if (colorMatch) {
                                    Log.Write("MultiWindowManager: Color match in '{0}'!", mw.Title);
                                    AlarmTriggered?.Invoke(mw);
                                }
                            }

                            // --- Icon/graphic detection (short-circuit once found) ---
                            if (needIconCheck) {
                                float iconScore;
                                bool iconFound = MatchIconTemplate(regionBmp, out iconScore);
                                mw.LastIconDetected = iconFound;
                                iconCheckedCount++;
                                Log.Write("MultiWindowManager: Icon check '{0}': score={1:F3} found={2}",
                                    mw.Title, iconScore, iconFound);

                                if (iconFound) {
                                    allWindowsLostIcon = false;
                                    needIconCheck = false; // skip icon check for subsequent windows
                                }
                            }
                        }
                        finally {
                            regionBmp.Dispose();
                        }
                    }
                    catch (Exception ex) {
                        Log.Write("MultiWindowManager: Detection error for '{0}': {1}", mw.Title, ex.Message);
                    }
                }

                // Icon disappearance alarm: triggers when icon is gone from ALL windows
                if (anyIconEnabled && iconCheckedCount > 0 && allWindowsLostIcon) {
                    if (!_iconAlarmActive) {
                        _iconAlarmActive = true;
                        Log.Write("MultiWindowManager: Icon disappeared from ALL {0} windows! Alarm!", iconCheckedCount);
                        PlayIconDisappearAlarm();
                        IconDisappearedAlarm?.Invoke();
                    }
                }
                else {
                    _iconAlarmActive = false;
                }
            }
            Log.Write("MultiWindowManager: Detection thread exited");
        }

        /// <summary>
        /// Gets the current monitoring region from the primary window / MainForm.
        /// </summary>
        private Rectangle GetCurrentRegion() {
            if (Form == null) return Rectangle.Empty;

            try {
                var region = Form.SelectedThumbnailRegion;
                if (region == null) return Rectangle.Empty;

                var primary = PrimaryWindow;
                if (primary == null) return Rectangle.Empty;

                Rectangle clientRect;
                WindowManagerMethods.GetClientRect(primary.WindowHandle.Handle, out clientRect);
                var clientSize = new Size(clientRect.Width, clientRect.Height);

                return region.ComputeRegionRectangle(clientSize);
            }
            catch {
                return Rectangle.Empty;
            }
        }

        #endregion

        #region Window Capture

        /// <summary>
        /// Captures the specified region of a window into a Bitmap. Caller must dispose.
        /// </summary>
        private Bitmap CaptureWindowRegion(IntPtr hwnd, Rectangle regionRect) {
            Rectangle clientRect;
            if (!WindowManagerMethods.GetClientRect(hwnd, out clientRect))
                return null;

            int clientW = clientRect.Width;
            int clientH = clientRect.Height;
            if (clientW <= 0 || clientH <= 0) return null;

            if (regionRect.IsEmpty)
                regionRect = new Rectangle(0, 0, clientW, clientH);

            regionRect = Rectangle.Intersect(regionRect, new Rectangle(0, 0, clientW, clientH));
            if (regionRect.Width <= 0 || regionRect.Height <= 0) return null;

            Bitmap windowBmp = null;
            try {
                Rectangle windowRect;
                WindowManagerMethods.GetWindowRect(hwnd, out windowRect);
                int windowWidth = windowRect.Width - windowRect.X;
                int windowHeight = windowRect.Height - windowRect.Y;
                if (windowWidth <= 0 || windowHeight <= 0) return null;

                windowBmp = new Bitmap(windowWidth, windowHeight, PixelFormat.Format32bppArgb);
                bool printSuccess = false;

                uint[] flags = new uint[] { PW_RENDERFULLCONTENT, PW_CLIENTONLY, 0 };
                foreach (uint flag in flags) {
                    using (Graphics g = Graphics.FromImage(windowBmp)) {
                        IntPtr hdc = g.GetHdc();
                        try { printSuccess = PrintWindow(hwnd, hdc, flag); }
                        finally { g.ReleaseHdc(hdc); }
                    }
                    if (printSuccess) break;
                }

                if (!printSuccess) {
                    windowBmp.Dispose();
                    return CaptureWindowRegionFallback(hwnd, regionRect);
                }

                var clientOriginScreen = WindowManagerMethods.ClientToScreen(hwnd, new NPoint(0, 0));
                int clientOffsetX = clientOriginScreen.X - windowRect.X;
                int clientOffsetY = clientOriginScreen.Y - windowRect.Y;

                int cropX = clientOffsetX + regionRect.X;
                int cropY = clientOffsetY + regionRect.Y;
                int cropW = Math.Min(regionRect.Width, windowWidth - cropX);
                int cropH = Math.Min(regionRect.Height, windowHeight - cropY);

                if (cropW <= 0 || cropH <= 0) {
                    windowBmp.Dispose();
                    return null;
                }

                Bitmap regionBmp = windowBmp.Clone(
                    new Rectangle(cropX, cropY, cropW, cropH),
                    PixelFormat.Format32bppArgb);
                windowBmp.Dispose();
                return regionBmp;
            }
            catch {
                if (windowBmp != null) windowBmp.Dispose();
                return null;
            }
        }

        private Bitmap CaptureWindowRegionFallback(IntPtr hwnd, Rectangle regionRect) {
            int cropW = Math.Min(regionRect.Width, 800);
            int cropH = Math.Min(regionRect.Height, 600);

            IntPtr hdcSrc = IntPtr.Zero, hdcMem = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero, hOldBmp = IntPtr.Zero;

            try {
                hdcSrc = GetDC(hwnd);
                if (hdcSrc == IntPtr.Zero) return null;

                hdcMem = CreateCompatibleDC(hdcSrc);
                hBitmap = CreateCompatibleBitmap(hdcSrc, cropW, cropH);
                hOldBmp = SelectObject(hdcMem, hBitmap);

                bool ok = BitBlt(hdcMem, 0, 0, cropW, cropH, hdcSrc,
                    regionRect.X, regionRect.Y, SRCCOPY);
                SelectObject(hdcMem, hOldBmp);

                if (!ok) return null;

                return Bitmap.FromHbitmap(hBitmap);
            }
            catch {
                return null;
            }
            finally {
                if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
                if (hdcMem != IntPtr.Zero) DeleteDC(hdcMem);
                if (hdcSrc != IntPtr.Zero) ReleaseDC(hwnd, hdcSrc);
            }
        }

        #endregion

        #region Color Detection

        /// <summary>
        /// Scans a bitmap for color matches using HSV classification.
        /// </summary>
        private bool ScanBitmapForColor(Bitmap bmp, HashSet<ColorCategory> enabledCategories) {
            if (bmp == null) return false;

            int w = bmp.Width;
            int h = bmp.Height;
            int totalPixels = w * h;
            if (totalPixels == 0) return false;

            BitmapData bmpData = null;
            try {
                bmpData = bmp.LockBits(new Rectangle(0, 0, w, h),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int stride = bmpData.Stride;
                int byteCount = stride * h;
                byte[] pixels = new byte[byteCount];
                Marshal.Copy(bmpData.Scan0, pixels, 0, byteCount);

                bmp.UnlockBits(bmpData);
                bmpData = null;

                int redCount = 0, orangeCount = 0, grayCount = 0;

                for (int y = 0; y < h; y++) {
                    int rowOffset = y * stride;
                    for (int x = 0; x < w; x++) {
                        int idx = rowOffset + x * 4;
                        byte pb = pixels[idx];
                        byte pg = pixels[idx + 1];
                        byte pr = pixels[idx + 2];

                        if (pr <= 15 && pg <= 15 && pb <= 15) continue;
                        if (pr >= 240 && pg >= 240 && pb >= 240) continue;

                        var category = ClassifyPixelColor(pr, pg, pb);
                        switch (category) {
                            case ColorCategory.Red: redCount++; break;
                            case ColorCategory.Orange: orangeCount++; break;
                            case ColorCategory.Gray: grayCount++; break;
                        }
                    }
                }

                if (enabledCategories.Contains(ColorCategory.Red) && redCount >= 1) {
                    Log.Write("MultiWindow: Red match, {0}px", redCount);
                    return true;
                }
                if (enabledCategories.Contains(ColorCategory.Orange) && orangeCount >= 1) {
                    Log.Write("MultiWindow: Orange match, {0}px", orangeCount);
                    return true;
                }
                if (enabledCategories.Contains(ColorCategory.Gray)) {
                    int grayDensityPct = (int)((long)grayCount * 100 / totalPixels);
                    if (grayDensityPct >= 8) {
                        Log.Write("MultiWindow: Gray match, density={0}%", grayDensityPct);
                        return true;
                    }
                }

                return false;
            }
            finally {
                if (bmpData != null) bmp.UnlockBits(bmpData);
            }
        }

        private static ColorCategory ClassifyPixelColor(byte r, byte g, byte b) {
            float h, s, v;
            RgbToHsv(r, g, b, out h, out s, out v);

            if (s >= 40 && v >= 25) {
                if (h <= 15 || h >= 345)
                    return ColorCategory.Red;
                if (h > 15 && h <= 55)
                    return ColorCategory.Orange;
            }

            if (s < 25 && v >= 15 && v <= 83)
                return ColorCategory.Gray;

            return ColorCategory.None;
        }

        private static void RgbToHsv(int r, int g, int b, out float h, out float s, out float v) {
            float rf = r / 255f, gf = g / 255f, bf = b / 255f;
            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            float delta = max - min;

            v = max * 100f;
            if (max < 0.0001f) { h = 0; s = 0; return; }
            s = (delta / max) * 100f;

            if (delta < 0.0001f) { h = 0; return; }
            if (Math.Abs(max - rf) < 0.0001f)
                h = 60f * (((gf - bf) / delta) % 6f);
            else if (Math.Abs(max - gf) < 0.0001f)
                h = 60f * (((bf - rf) / delta) + 2f);
            else
                h = 60f * (((rf - gf) / delta) + 4f);

            if (h < 0) h += 360f;
        }

        #endregion

        #region Icon Template Matching

        /// <summary>
        /// Searches for the icon template in the given bitmap using sliding window + color histogram matching.
        /// Returns true if a match above threshold is found. bestScore receives the highest found score.
        /// </summary>
        private bool MatchIconTemplate(Bitmap source, out float bestScore) {
            bestScore = 0;
            if (_iconTemplatePixels == null || source == null) return false;

            // --- Scale normalization ---
            // The template was captured from ThumbnailPanel at display scale.
            // The source is captured from the actual full-resolution game window.
            // Resize source to _iconCaptureNormalizeW x _iconCaptureNormalizeH so scales match.
            Bitmap normalizedSource = null;
            if (_iconCaptureNormalizeW > 0 && _iconCaptureNormalizeH > 0 &&
                (source.Width != _iconCaptureNormalizeW || source.Height != _iconCaptureNormalizeH)) {
                normalizedSource = new Bitmap(source, _iconCaptureNormalizeW, _iconCaptureNormalizeH);
                source = normalizedSource;
            }

            try {

            int srcW = source.Width;
            int srcH = source.Height;
            int tplW = _iconTemplateW;
            int tplH = _iconTemplateH;

            if (srcW < tplW || srcH < tplH) return false;

            // Read source pixels
            BitmapData srcData = null;
            byte[] srcPixels;
            int srcStride;

            try {
                srcData = source.LockBits(new Rectangle(0, 0, srcW, srcH),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                srcStride = srcData.Stride;
                int byteCount = srcStride * srcH;
                srcPixels = new byte[byteCount];
                Marshal.Copy(srcData.Scan0, srcPixels, 0, byteCount);
            }
            finally {
                if (srcData != null) source.UnlockBits(srcData);
            }

            // Pre-compute template color histogram (16-bin per channel = 4096 bins)
            int[] tplHist = ComputeColorHistogram(_iconTemplatePixels, _iconTemplateStride, 0, 0, tplW, tplH);

            // Slide the template window across the source, step by 2px for speed
            int step = Math.Max(1, Math.Min(tplW, tplH) / 4);

            for (int sy = 0; sy <= srcH - tplH; sy += step) {
                for (int sx = 0; sx <= srcW - tplW; sx += step) {
                    int[] srcHist = ComputeColorHistogram(srcPixels, srcStride, sx, sy, tplW, tplH);
                    float score = CompareHistograms(tplHist, srcHist);

                    if (score > bestScore) bestScore = score;
                    if (score >= _iconMatchThreshold) return true;
                }
            }

            return false;

            } finally {
                if (normalizedSource != null) normalizedSource.Dispose();
            }
        }

        /// <summary>
        /// Computes a 16-bin-per-channel color histogram (R,G,B) = 4096 bins total.
        /// </summary>
        private static int[] ComputeColorHistogram(byte[] pixels, int stride, int startX, int startY, int w, int h) {
            int[] hist = new int[16 * 16 * 16]; // 4096 bins

            for (int y = 0; y < h; y++) {
                int rowOffset = (startY + y) * stride;
                for (int x = 0; x < w; x++) {
                    int idx = rowOffset + (startX + x) * 4;
                    byte b = pixels[idx];
                    byte g = pixels[idx + 1];
                    byte r = pixels[idx + 2];

                    int rBin = r >> 4; // 0-15
                    int gBin = g >> 4;
                    int bBin = b >> 4;
                    hist[rBin * 256 + gBin * 16 + bBin]++;
                }
            }
            return hist;
        }

        /// <summary>
        /// Compares two histograms using histogram intersection (Swain-Ballard).
        /// Returns a similarity score from 0.0 to 1.0.
        /// </summary>
        private static float CompareHistograms(int[] hist1, int[] hist2) {
            long intersectionSum = 0;
            long totalSum = 0;

            for (int i = 0; i < hist1.Length; i++) {
                intersectionSum += Math.Min(hist1[i], hist2[i]);
                totalSum += hist1[i];
            }

            if (totalSum == 0) return 0;
            return (float)intersectionSum / totalSum;
        }

        #endregion

        #region Alarm Sound

        /// <summary>
        /// Plays the icon disappearance alarm sound.
        /// </summary>
        private void PlayIconDisappearAlarm() {
            _alarmStopTimer?.Dispose();
            _alarmStopTimer = new System.Threading.Timer(_ => StopAlarm(), null, AlarmDuration, System.Threading.Timeout.Infinite);

            Log.Write("MultiWindow icon alarm triggered! volume={0}, file={1}", _alarmVolume, _alarmSoundFile);

            if (!string.IsNullOrEmpty(_alarmSoundFile) && File.Exists(_alarmSoundFile)) {
                var soundFile = _alarmSoundFile;
                var volume = _alarmVolume;
                if (_uiDispatcher != null) {
                    _uiDispatcher.BeginInvoke((Action)(() => {
                        try {
                            if (_mediaPlayer == null)
                                _mediaPlayer = new System.Windows.Media.MediaPlayer();
                            _mediaPlayer.Open(new Uri(soundFile));
                            _mediaPlayer.Volume = volume;
                            _mediaPlayer.Play();
                        }
                        catch (Exception ex) {
                            Log.Write("MultiWindow alarm play error: {0}", ex.Message);
                        }
                    }));
                }
                else {
                    System.Media.SystemSounds.Exclamation.Play();
                }
            }
            else {
                System.Media.SystemSounds.Exclamation.Play();
            }
        }

        private void StopAlarm() {
            _alarmStopTimer?.Dispose();
            _alarmStopTimer = null;

            if (_uiDispatcher != null) {
                _uiDispatcher.BeginInvoke((Action)(() => {
                    try {
                        if (_mediaPlayer != null)
                            _mediaPlayer.Stop();
                    }
                    catch { }
                }));
            }
        }

        #endregion

        public void Dispose() {
            StopDetection();
            _windows.Clear();
            _alarmStopTimer?.Dispose();
            if (_iconTemplate != null) {
                _iconTemplate.Dispose();
                _iconTemplate = null;
            }
            if (_uiDispatcher != null) {
                _uiDispatcher.BeginInvoke((Action)(() => {
                    try {
                        if (_mediaPlayer != null) {
                            _mediaPlayer.Stop();
                            _mediaPlayer.Close();
                            _mediaPlayer = null;
                        }
                    }
                    catch { }
                }));
            }
        }
    }
}
