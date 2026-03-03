using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
    /// - Multi-window color detection (reuses ColorDetectionProcessor logic)
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
        private int _sampleInterval = 500;

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
        /// Gets whether multi-window monitoring is active.
        /// </summary>
        public bool IsActive {
            get { return _windows.Count > 0; }
        }

        /// <summary>
        /// Fired when any secondary window's color detection triggers an alarm.
        /// The parameter is the MonitoredWindow that matched.
        /// </summary>
        public event Action<MonitoredWindow> AlarmTriggered;

        /// <summary>
        /// Fired when the monitored window list changes.
        /// </summary>
        public event Action WindowListChanged;

        /// <summary>
        /// Adds a window to the monitored list.
        /// If this is the first window, it becomes primary automatically.
        /// </summary>
        public void AddWindow(WindowHandle handle) {
            if (handle == null) return;
            // Avoid duplicates
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
        /// If the removed window was primary, the first remaining window becomes primary.
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

        /// <summary>
        /// Updates the ThumbnailPanel to show the current primary window.
        /// </summary>
        private void SwitchPrimaryToThumbnail() {
            var primary = PrimaryWindow;
            if (primary == null || Form == null) return;

            try {
                Form.SetThumbnail(primary.WindowHandle, null);
                // Re-apply the current region if one exists
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

        #region Multi-window Color Detection

        /// <summary>
        /// Starts background color detection for all non-primary monitored windows.
        /// Reuses the ColorDetectionProcessor's classification logic.
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
            Log.Write("MultiWindowManager: Detection thread started, monitoring {0} windows",
                _windows.Count);
        }

        /// <summary>
        /// Stops background color detection.
        /// </summary>
        public void StopDetection() {
            _detectionRunning = false;
            Log.Write("MultiWindowManager: Detection thread stop requested");
        }

        /// <summary>
        /// Background detection loop. Iterates all non-primary windows and checks for color matches.
        /// </summary>
        private void DetectionLoop() {
            while (_detectionRunning) {
                System.Threading.Thread.Sleep(_sampleInterval);
                if (!_detectionRunning) break;

                // Get the region from the primary window's current region (synchronized across all windows)
                Rectangle regionRect = GetCurrentRegion();

                // Get color detection settings from ColorDetectionProcessor
                ColorDetectionProcessor processor = null;
                try {
                    if (Form != null)
                        processor = Form.MessagePumpManager.Get<ColorDetectionProcessor>();
                }
                catch { }

                if (processor == null || !processor.Enabled) continue;

                var enabledCategories = processor.EnabledCategories;
                if (enabledCategories == null || enabledCategories.Count == 0) continue;

                // Scan each non-primary window
                foreach (var mw in _windows.ToArray()) {
                    if (!_detectionRunning) break;
                    if (mw.IsPrimary) continue; // Primary window is handled by ColorDetectionProcessor
                    if (!mw.IsColorDetectionEnabled) continue;

                    // Verify window is still valid
                    if (!IsWindow(mw.WindowHandle.Handle)) {
                        Log.Write("MultiWindowManager: Window '{0}' no longer valid, removing", mw.Title);
                        _windows.Remove(mw);
                        WindowListChanged?.Invoke();
                        continue;
                    }

                    try {
                        bool match = DetectColorInWindow(mw.WindowHandle.Handle, regionRect, enabledCategories);
                        mw.LastDetectionResult = match;
                        mw.LastDetectionTime = DateTime.Now;

                        if (match) {
                            Log.Write("MultiWindowManager: Color match in '{0}'!", mw.Title);
                            AlarmTriggered?.Invoke(mw);
                        }
                    }
                    catch (Exception ex) {
                        Log.Write("MultiWindowManager: Detection error for '{0}': {1}", mw.Title, ex.Message);
                    }
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

                // Get client size of primary window
                Rectangle clientRect;
                WindowManagerMethods.GetClientRect(primary.WindowHandle.Handle, out clientRect);
                var clientSize = new Size(clientRect.Width, clientRect.Height);

                return region.ComputeRegionRectangle(clientSize);
            }
            catch {
                return Rectangle.Empty;
            }
        }

        /// <summary>
        /// Detects color in a specific window, reusing ColorDetectionProcessor's classification logic.
        /// Uses PrintWindow API to capture the window regardless of occlusion.
        /// </summary>
        private bool DetectColorInWindow(IntPtr hwnd, Rectangle regionRect,
            HashSet<ColorCategory> enabledCategories) {

            // Get window client size
            Rectangle clientRect;
            if (!WindowManagerMethods.GetClientRect(hwnd, out clientRect))
                return false;

            int clientW = clientRect.Width;
            int clientH = clientRect.Height;
            if (clientW <= 0 || clientH <= 0) return false;

            // If no region specified, use full client area
            if (regionRect.IsEmpty) {
                regionRect = new Rectangle(0, 0, clientW, clientH);
            }

            // Clamp region to client area
            regionRect = Rectangle.Intersect(regionRect, new Rectangle(0, 0, clientW, clientH));
            if (regionRect.Width <= 0 || regionRect.Height <= 0) return false;

            // Capture using PrintWindow
            Bitmap windowBmp = null;
            Bitmap regionBmp = null;
            try {
                // Try PrintWindow to capture window content (occlusion-independent)
                Rectangle windowRect;
                WindowManagerMethods.GetWindowRect(hwnd, out windowRect);
                int windowWidth = windowRect.Width - windowRect.X;
                int windowHeight = windowRect.Height - windowRect.Y;

                if (windowWidth <= 0 || windowHeight <= 0)
                    return false;

                windowBmp = new Bitmap(windowWidth, windowHeight, PixelFormat.Format32bppArgb);
                bool printSuccess = false;

                uint[] flags = new uint[] { PW_RENDERFULLCONTENT, PW_CLIENTONLY, 0 };
                foreach (uint flag in flags) {
                    using (Graphics g = Graphics.FromImage(windowBmp)) {
                        IntPtr hdc = g.GetHdc();
                        try {
                            printSuccess = PrintWindow(hwnd, hdc, flag);
                        }
                        finally {
                            g.ReleaseHdc(hdc);
                        }
                    }
                    if (printSuccess) break;
                }

                if (!printSuccess) {
                    // Fallback: BitBlt from window DC
                    return DetectColorFallback(hwnd, regionRect, enabledCategories);
                }

                // Calculate client offset in window bitmap
                var clientOriginScreen = WindowManagerMethods.ClientToScreen(hwnd, new NPoint(0, 0));
                int clientOffsetX = clientOriginScreen.X - windowRect.X;
                int clientOffsetY = clientOriginScreen.Y - windowRect.Y;

                int cropX = clientOffsetX + regionRect.X;
                int cropY = clientOffsetY + regionRect.Y;
                int cropW = Math.Min(regionRect.Width, windowWidth - cropX);
                int cropH = Math.Min(regionRect.Height, windowHeight - cropY);

                if (cropW <= 0 || cropH <= 0) return false;

                regionBmp = windowBmp.Clone(new Rectangle(cropX, cropY, cropW, cropH),
                    PixelFormat.Format32bppArgb);

                return ScanBitmapForColor(regionBmp, enabledCategories);
            }
            catch (Exception ex) {
                Log.Write("MultiWindowManager DetectColor error: {0}", ex.Message);
                return false;
            }
            finally {
                if (regionBmp != null) regionBmp.Dispose();
                if (windowBmp != null) windowBmp.Dispose();
            }
        }

        /// <summary>
        /// Fallback: BitBlt from window DC.
        /// </summary>
        private bool DetectColorFallback(IntPtr hwnd, Rectangle regionRect,
            HashSet<ColorCategory> enabledCategories) {
            int cropW = Math.Min(regionRect.Width, 800);
            int cropH = Math.Min(regionRect.Height, 600);

            IntPtr hdcSrc = IntPtr.Zero, hdcMem = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero, hOldBmp = IntPtr.Zero;
            Bitmap bmp = null;

            try {
                hdcSrc = GetDC(hwnd);
                if (hdcSrc == IntPtr.Zero) return false;

                hdcMem = CreateCompatibleDC(hdcSrc);
                hBitmap = CreateCompatibleBitmap(hdcSrc, cropW, cropH);
                hOldBmp = SelectObject(hdcMem, hBitmap);

                bool ok = BitBlt(hdcMem, 0, 0, cropW, cropH, hdcSrc,
                    regionRect.X, regionRect.Y, SRCCOPY);
                SelectObject(hdcMem, hOldBmp);

                if (!ok) return false;

                bmp = Bitmap.FromHbitmap(hBitmap);
                return ScanBitmapForColor(bmp, enabledCategories);
            }
            catch {
                return false;
            }
            finally {
                if (bmp != null) bmp.Dispose();
                if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
                if (hdcMem != IntPtr.Zero) DeleteDC(hdcMem);
                if (hdcSrc != IntPtr.Zero) ReleaseDC(hwnd, hdcSrc);
            }
        }

        /// <summary>
        /// Scans a bitmap for color matches using the same HSV classification as ColorDetectionProcessor.
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
                        byte pb = pixels[idx];      // B
                        byte pg = pixels[idx + 1];  // G
                        byte pr = pixels[idx + 2];  // R

                        // Skip black and white
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

                // Check thresholds (same as ColorDetectionProcessor)
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
                    if (grayDensityPct >= 8) { // GrayMinDensityPct = 8
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

        /// <summary>
        /// HSV-based pixel color classification (same as ColorDetectionProcessor.ClassifyPixelColor).
        /// </summary>
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

        /// <summary>
        /// RGB to HSV conversion (same as ColorDetectionProcessor.RgbToHsv).
        /// </summary>
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

        public void Dispose() {
            StopDetection();
            _windows.Clear();
        }
    }
}
