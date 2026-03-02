using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using OnTopReplica.Native;
using OnTopReplica.Properties;

namespace OnTopReplica.MessagePumpProcessors {

    /// <summary>
    /// Predefined color categories for detection.
    /// </summary>
    public enum ColorCategory {
        None,
        Red,    // 红色
        Orange, // 橙色
        Gray    // 灰色
    }

    /// <summary>
    /// Monitors the cloned window for predefined color categories and triggers an alarm when detected.
    /// Uses LockBits for fast pixel scanning to avoid blocking the UI thread.
    /// </summary>
    class ColorDetectionProcessor : BaseMessagePumpProcessor {

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
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, uint rop);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        private const uint SRCCOPY = 0x00CC0020;

        // PW_RENDERFULLCONTENT = 2: full rendering including DirectComposition content
        private const uint PW_RENDERFULLCONTENT = 2;
        // PW_CLIENTONLY = 1: only client area
        private const uint PW_CLIENTONLY = 1;

        private bool _enabled = false;
        private HashSet<ColorCategory> _enabledCategories = new HashSet<ColorCategory>() { ColorCategory.Red };
        private int _sampleInterval = 500; // Sampling interval in milliseconds
        private float _alarmVolume = 1.0f; // 0.0 - 1.0
        private string _alarmSoundFile = string.Empty;
        private long _lastSampleTick = 0;
        private bool _alarmActive = false;
        private long _alarmStartTick = 0;
        private const int AlarmDuration = 3000; // 3 seconds in milliseconds
        private const int MinMatchPixels = 50; // Minimum matching pixels to trigger alarm
        private System.Windows.Media.MediaPlayer _mediaPlayer;

        public bool Enabled {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// Set of color categories that should trigger the alarm when detected.
        /// </summary>
        public HashSet<ColorCategory> EnabledCategories {
            get { return _enabledCategories; }
            set { _enabledCategories = value ?? new HashSet<ColorCategory>(); }
        }

        public int SampleInterval {
            get { return _sampleInterval; }
            set { _sampleInterval = Math.Max(100, value); }
        }

        public bool IsAlarmActive {
            get { return _alarmActive; }
        }

        public float AlarmVolume {
            get { return _alarmVolume; }
            set { _alarmVolume = Math.Max(0, Math.Min(1, value)); }
        }

        public string AlarmSoundFile {
            get { return _alarmSoundFile; }
            set { _alarmSoundFile = value; }
        }

        public override bool Process(ref Message msg) {
            if (!_enabled) {
                return false;
            }
            if (Form.CurrentThumbnailWindowHandle == null) {
                return false;
            }
            if (_enabledCategories.Count == 0) {
                return false;
            }

            // Sample at regular intervals
            long currentTick = Environment.TickCount;
            if (currentTick - _lastSampleTick < _sampleInterval)
                return false;

            _lastSampleTick = currentTick;

            // Check if we should stop the alarm
            if (_alarmActive) {
                if (currentTick - _alarmStartTick >= AlarmDuration) {
                    StopAlarm();
                }
                return false;
            }

            // Detect predefined color categories in the window
            var catList = string.Join(",", _enabledCategories);
            Log.Write("Performing color detection (categories={0})", catList);
            if (DetectColorInWindow(Form.CurrentThumbnailWindowHandle.Handle)) {
                StartAlarm();
            }

            return false;
        }

        /// <summary>
        /// Detects if the target color exists in the monitored window.
        /// Uses PrintWindow API to capture the source window directly (avoids occlusion issues).
        /// </summary>
        private bool DetectColorInWindow(IntPtr windowHandle)
        {
            try
            {
                // 获取窗口客户区大小
                Rectangle clientRect;
                if (!WindowManagerMethods.GetClientRect(windowHandle, out clientRect))
                {
                    Log.Write("ColorDetection: Failed to get client rect");
                    return false;
                }

                Size clientSize = new Size(clientRect.Width, clientRect.Height);
                if (clientSize.Width <= 0 || clientSize.Height <= 0)
                {
                    Log.Write("ColorDetection: Invalid client size {0}x{1}", clientSize.Width, clientSize.Height);
                    return false;
                }

                // 获取主窗体实例
                MainForm mainForm = null;
                foreach (Form form in Application.OpenForms)
                {
                    if (form is MainForm mf)
                    {
                        mainForm = mf;
                        break;
                    }
                }

                Rectangle regionRect;

                // 优先使用选定区域
                if (mainForm != null && mainForm.SelectedThumbnailRegion != null)
                {
                    var selectedRegion = mainForm.SelectedThumbnailRegion;
                    regionRect = selectedRegion.ComputeRegionRectangle(clientSize);
                    Log.Write("ColorDetection: Region={0},{1} {2}x{3} (relative={4})",
                        regionRect.X, regionRect.Y, regionRect.Width, regionRect.Height, selectedRegion.Relative);
                }
                else
                {
                    regionRect = clientRect;
                    Log.Write("ColorDetection: No region, full window {0}x{1}", clientSize.Width, clientSize.Height);
                }

                if (regionRect.Width <= 0 || regionRect.Height <= 0)
                {
                    Log.Write("ColorDetection: Invalid region {0}x{1}", regionRect.Width, regionRect.Height);
                    return false;
                }

                // 使用 PrintWindow 捕获窗口内容（不受遮挡影响）
                Bitmap windowBmp = null;
                Bitmap regionBmp = null;
                try
                {
                    // 获取完整窗口大小（包含非客户区）
                    // 注意: GetWindowRect 返回 RECT (left,top,right,bottom) 映射到 Rectangle (X,Y,Width=right,Height=bottom)
                    Rectangle windowRect;
                    WindowManagerMethods.GetWindowRect(windowHandle, out windowRect);
                    int windowWidth = windowRect.Width - windowRect.X;
                    int windowHeight = windowRect.Height - windowRect.Y;

                    if (windowWidth <= 0 || windowHeight <= 0)
                    {
                        Log.Write("ColorDetection: Invalid window size {0}x{1} (rect={2},{3},{4},{5})",
                            windowWidth, windowHeight, windowRect.X, windowRect.Y, windowRect.Width, windowRect.Height);
                        return false;
                    }

                    // 创建窗口大小的位图，用 PrintWindow 捕获
                    windowBmp = new Bitmap(windowWidth, windowHeight, PixelFormat.Format32bppArgb);
                    bool printSuccess = false;

                    // 尝试不同的 PrintWindow flags
                    uint[] flags = new uint[] { PW_RENDERFULLCONTENT, PW_CLIENTONLY, 0 };
                    foreach (uint flag in flags)
                    {
                        using (Graphics g = Graphics.FromImage(windowBmp))
                        {
                            IntPtr hdc = g.GetHdc();
                            try
                            {
                                printSuccess = PrintWindow(windowHandle, hdc, flag);
                            }
                            finally
                            {
                                g.ReleaseHdc(hdc);
                            }
                        }
                        if (printSuccess)
                        {
                            Log.Write("ColorDetection: PrintWindow OK with flag={0}, winSize={1}x{2}", flag, windowWidth, windowHeight);
                            break;
                        }
                    }

                    if (!printSuccess)
                    {
                        Log.Write("ColorDetection: PrintWindow failed (all flags), falling back to CopyFromScreen");
                        windowBmp.Dispose();
                        windowBmp = null;
                        return DetectColorFallback(windowHandle, regionRect);
                    }

                    // 计算客户区在窗口位图中的偏移量
                    var clientOriginScreen = WindowManagerMethods.ClientToScreen(windowHandle, new NPoint(0, 0));
                    int clientOffsetX = clientOriginScreen.X - windowRect.X;
                    int clientOffsetY = clientOriginScreen.Y - windowRect.Y;

                    // 将区域坐标偏移到窗口位图坐标
                    int cropX = clientOffsetX + regionRect.X;
                    int cropY = clientOffsetY + regionRect.Y;
                    int cropW = Math.Min(regionRect.Width, windowWidth - cropX);
                    int cropH = Math.Min(regionRect.Height, windowHeight - cropY);

                    Log.Write("ColorDetection: PrintWindow OK, winSize={0}x{1}, clientOffset=({2},{3}), crop=({4},{5} {6}x{7})",
                        windowWidth, windowHeight, clientOffsetX, clientOffsetY, cropX, cropY, cropW, cropH);

                    if (cropW <= 0 || cropH <= 0)
                    {
                        Log.Write("ColorDetection: crop region empty");
                        return false;
                    }

                    // 裁剪出选定区域
                    regionBmp = windowBmp.Clone(new Rectangle(cropX, cropY, cropW, cropH), PixelFormat.Format32bppArgb);

                    // 保存调试截图（每20次保存一次，避免IO过多）
                    _debugCounter++;
                    if (_debugCounter % 20 == 1)
                    {
                        SaveDebugBitmap(regionBmp, "region_capture");
                    }

                    return SampleBitmapForColor(regionBmp);
                }
                finally
                {
                    if (regionBmp != null)
                        regionBmp.Dispose();
                    if (windowBmp != null)
                        windowBmp.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Write("ColorDetection Error: {0}\n{1}", ex.Message, ex.StackTrace);
                return false;
            }
        }

        private int _debugCounter = 0;

        /// <summary>
        /// Saves a bitmap to the AppData folder for debugging.
        /// </summary>
        private void SaveDebugBitmap(Bitmap bmp, string prefix)
        {
            try
            {
                string dir = AppPaths.PrivateRoamingFolderPath;
                if (string.IsNullOrEmpty(dir)) return;
                string path = Path.Combine(dir, prefix + "_debug.png");
                bmp.Save(path, ImageFormat.Png);
                Log.Write("ColorDetection: Debug bitmap saved to {0}", path);
            }
            catch (Exception ex)
            {
                Log.Write("ColorDetection: Failed to save debug bitmap: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Fallback: use BitBlt from source window DC to capture client area directly.
        /// This avoids CopyFromScreen which captures the OnTopReplica overlay.
        /// If BitBlt returns all-black image, falls back to CopyFromScreen with opacity trick.
        /// </summary>
        private bool DetectColorFallback(IntPtr windowHandle, Rectangle regionRect)
        {
            Log.Write("ColorDetection Fallback(BitBlt): region={0},{1} {2}x{3}",
                regionRect.X, regionRect.Y, regionRect.Width, regionRect.Height);

            if (regionRect.Width <= 0 || regionRect.Height <= 0)
                return false;

            int cropW = Math.Min(regionRect.Width, 800);
            int cropH = Math.Min(regionRect.Height, 600);

            // 方案1: 使用 BitBlt + GetDC 直接从源窗口客户区捕获（无闪烁）
            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr hdcMem = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr hOldBmp = IntPtr.Zero;
            Bitmap bmp = null;
            try
            {
                hdcSrc = GetDC(windowHandle);
                if (hdcSrc != IntPtr.Zero)
                {
                    hdcMem = CreateCompatibleDC(hdcSrc);
                    hBitmap = CreateCompatibleBitmap(hdcSrc, cropW, cropH);
                    hOldBmp = SelectObject(hdcMem, hBitmap);

                    bool ok = BitBlt(hdcMem, 0, 0, cropW, cropH, hdcSrc, regionRect.X, regionRect.Y, SRCCOPY);
                    SelectObject(hdcMem, hOldBmp);

                    if (ok)
                    {
                        bmp = Bitmap.FromHbitmap(hBitmap);

                        // 检查是否全黑（硬件渲染窗口可能返回黑色图像）
                        if (!IsBitmapAllBlack(bmp))
                        {
                            Log.Write("ColorDetection Fallback: BitBlt OK, size={0}x{1}", cropW, cropH);
                            _debugCounter++;
                            if (_debugCounter % 20 == 1)
                                SaveDebugBitmap(bmp, "bitblt_capture");
                            return SampleBitmapForColor(bmp);
                        }
                        else
                        {
                            Log.Write("ColorDetection Fallback: BitBlt returned all-black, trying CopyFromScreen");
                            bmp.Dispose();
                            bmp = null;
                        }
                    }
                    else
                    {
                        Log.Write("ColorDetection Fallback: BitBlt failed");
                    }
                }
                else
                {
                    Log.Write("ColorDetection Fallback: GetDC failed");
                }
            }
            catch (Exception ex)
            {
                Log.Write("ColorDetection Fallback BitBlt error: {0}", ex.Message);
                if (bmp != null) { bmp.Dispose(); bmp = null; }
            }
            finally
            {
                if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
                if (hdcMem != IntPtr.Zero) DeleteDC(hdcMem);
                if (hdcSrc != IntPtr.Zero) ReleaseDC(windowHandle, hdcSrc);
            }

            // 方案2: CopyFromScreen（最后手段，可能包含覆盖层但不会闪烁）
            return DetectColorScreenCapture(windowHandle, regionRect);
        }

        /// <summary>
        /// Last resort: CopyFromScreen. May capture overlay but does not flicker.
        /// </summary>
        private bool DetectColorScreenCapture(IntPtr windowHandle, Rectangle regionRect)
        {
            var scrRect = WindowManagerMethods.ClientToScreenRect(windowHandle, regionRect);
            Log.Write("ColorDetection ScreenCapture: screen={0},{1} {2}x{3}", scrRect.X, scrRect.Y, scrRect.Width, scrRect.Height);

            if (scrRect.Width <= 0 || scrRect.Height <= 0)
                return false;

            int maxW = Math.Min(scrRect.Width, 800);
            int maxH = Math.Min(scrRect.Height, 600);

            Bitmap bmp = null;
            try
            {
                bmp = new Bitmap(maxW, maxH, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(scrRect.X, scrRect.Y, 0, 0, new Size(maxW, maxH));
                }
                // 始终保存截图用于调试（限频：每5次保存1次）
                _debugCounter++;
                if (_debugCounter % 5 == 1)
                    SaveDebugBitmap(bmp, "screen_capture");
                return SampleBitmapForColor(bmp);
            }
            catch (Exception ex)
            {
                Log.Write("ColorDetection ScreenCapture error: {0}", ex.Message);
                return false;
            }
            finally
            {
                if (bmp != null) bmp.Dispose();
            }
        }

        /// <summary>
        /// Checks if a bitmap is nearly black (indicating failed capture from hardware-rendered window).
        /// A pixel is considered "black" if all channels are &lt;= threshold.
        /// Returns true if more than 50% of sampled pixels are near-black.
        /// Hardware-rendered windows often return mostly-black images with scattered noise.
        /// </summary>
        private bool IsBitmapAllBlack(Bitmap bmp)
        {
            if (bmp == null) return true;
            const int threshold = 15; // 硬件渲染窗口 BitBlt 可能返回 (3,3,3) 等近黑像素
            int stepX = Math.Max(1, bmp.Width / 10);
            int stepY = Math.Max(1, bmp.Height / 10);
            int totalSamples = 0;
            int blackSamples = 0;
            for (int y = 0; y < bmp.Height; y += stepY)
            {
                for (int x = 0; x < bmp.Width; x += stepX)
                {
                    Color c = bmp.GetPixel(x, y);
                    totalSamples++;
                    if (c.R <= threshold && c.G <= threshold && c.B <= threshold)
                        blackSamples++;
                }
            }
            bool result = totalSamples > 0 && (blackSamples * 100 / totalSamples) >= 50;
            if (result)
                Log.Write("ColorDetection: IsBitmapAllBlack=true ({0}/{1} samples near-black={2}%, threshold={3})", blackSamples, totalSamples, blackSamples * 100 / totalSamples, threshold);
            return result;
        }

        /// <summary>
        /// Samples bitmap pixels using LockBits for fast access.
        /// Classifies each pixel into a color category and triggers if enough matching pixels found.
        /// </summary>
        private bool SampleBitmapForColor(Bitmap bmp) {
            if (bmp == null || bmp.Width <= 0 || bmp.Height <= 0)
                return false;

            BitmapData data = null;
            try {
                data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int stride = data.Stride;
                int byteCount = stride * bmp.Height;
                byte[] pixels = new byte[byteCount];
                Marshal.Copy(data.Scan0, pixels, 0, byteCount);

                // Count matching pixels per category
                int matchCount = 0;
                ColorCategory firstMatchCat = ColorCategory.None;
                int firstX = 0, firstY = 0;

                for (int y = 0; y < bmp.Height; y++) {
                    int rowOffset = y * stride;
                    for (int x = 0; x < bmp.Width; x++) {
                        int idx = rowOffset + x * 4;
                        byte b = pixels[idx];
                        byte g = pixels[idx + 1];
                        byte r = pixels[idx + 2];

                        ColorCategory cat = ClassifyPixelColor(r, g, b);
                        if (cat != ColorCategory.None && _enabledCategories.Contains(cat)) {
                            matchCount++;
                            if (matchCount == 1) {
                                firstMatchCat = cat;
                                firstX = x;
                                firstY = y;
                            }
                            if (matchCount >= MinMatchPixels) {
                                Log.Write("ColorDetection MATCH: {0} pixels of category {1} found (first at {2},{3} r={4},g={5},b={6}), bmpSize={7}x{8}",
                                    matchCount, firstMatchCat, firstX, firstY,
                                    pixels[firstY * stride + firstX * 4 + 2],
                                    pixels[firstY * stride + firstX * 4 + 1],
                                    pixels[firstY * stride + firstX * 4],
                                    bmp.Width, bmp.Height);
                                bmp.UnlockBits(data);
                                data = null;
                                SaveDebugBitmap(bmp, "alarm_trigger");
                                return true;
                            }
                        }
                    }
                }

                Log.Write("ColorDetection NO MATCH: only {0} matching pixels (need {1}), bmpSize={2}x{3}",
                    matchCount, MinMatchPixels, bmp.Width, bmp.Height);
                return false;
            }
            finally {
                if (data != null) bmp.UnlockBits(data);
            }
        }

        /// <summary>
        /// Classifies a pixel's RGB color into a predefined color category using HSV ranges.
        /// </summary>
        private static ColorCategory ClassifyPixelColor(byte r, byte g, byte b) {
            float h, s, v;
            RgbToHsv(r, g, b, out h, out s, out v);

            // Red: H in [0,20] or [340,360], S >= 40%, V >= 30%
            if (s >= 40 && v >= 30) {
                if (h <= 20 || h >= 340) {
                    return ColorCategory.Red;
                }
                // Orange: H in [20,45], S >= 40%, V >= 30%
                if (h > 20 && h <= 45) {
                    return ColorCategory.Orange;
                }
            }

            // Gray: S < 15%, V in [25,75%] (medium brightness, low saturation)
            if (s < 15 && v >= 25 && v <= 75) {
                return ColorCategory.Gray;
            }

            return ColorCategory.None;
        }

        /// <summary>
        /// Converts RGB (0-255) to HSV. H in degrees (0-360), S and V in percent (0-100).
        /// </summary>
        private static void RgbToHsv(int r, int g, int b, out float h, out float s, out float v) {
            float rf = r / 255f, gf = g / 255f, bf = b / 255f;
            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            float delta = max - min;

            // Value
            v = max * 100f;

            // Saturation
            if (max < 0.0001f) { h = 0; s = 0; return; }
            s = (delta / max) * 100f;

            // Hue
            if (delta < 0.0001f) { h = 0; return; }
            if (Math.Abs(max - rf) < 0.0001f)
                h = 60f * (((gf - bf) / delta) % 6f);
            else if (Math.Abs(max - gf) < 0.0001f)
                h = 60f * (((bf - rf) / delta) + 2f);
            else
                h = 60f * (((rf - gf) / delta) + 4f);

            if (h < 0) h += 360f;
        }

        /// <summary>
        /// Starts the alarm.
        /// </summary>
        private void StartAlarm() {
            if (_alarmActive)
                return;

            _alarmActive = true;
            _alarmStartTick = Environment.TickCount;

            Log.Write("Color alarm triggered! volume={0}, file={1}", _alarmVolume, _alarmSoundFile);

            try {
                // if a sound file is specified and exists, use MediaPlayer to play it
                if (!string.IsNullOrEmpty(_alarmSoundFile) && File.Exists(_alarmSoundFile)) {
                    if (_mediaPlayer == null) {
                        _mediaPlayer = new System.Windows.Media.MediaPlayer();
                    }
                    _mediaPlayer.Open(new Uri(_alarmSoundFile));
                    _mediaPlayer.Volume = _alarmVolume;
                    _mediaPlayer.Play();
                } else {
                    System.Media.SystemSounds.Beep.Play();
                }
            }
            catch (Exception ex) {
                Log.Write("Error playing alarm: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Plays an alarm tone for the duration.
        /// </summary>
        private void PlayAlarmTone() {
            try {
                // Generate and play a 1000Hz tone for 3 seconds
                int sampleRate = 44100;
                int duration = 3000; // 3 seconds
                int frequency = 1000; // 1000 Hz

                int numSamples = (sampleRate * duration) / 1000;
                byte[] data = new byte[numSamples * 2]; // 16-bit audio

                for (int i = 0; i < numSamples; i++) {
                    double t = (double)i / sampleRate;
                    short sample = (short)(short.MaxValue * 0.5 * Math.Sin(2 * Math.PI * frequency * t));
                    data[i * 2] = (byte)(sample & 0xFF);
                    data[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
                }

                // Play using WaveOut
                PlayAudioData(data, sampleRate);
            }
            catch (Exception ex) {
                Log.Write("Error playing alarm tone: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Plays audio data using Wave API.
        /// </summary>
        private void PlayAudioData(byte[] audioData, int sampleRate) {
            try {
                // Use the simple beep method from System.Media
                // For more complex tones, we would need WaveOut API
                for (int i = 0; i < 3; i++) {
                    System.Media.SystemSounds.Beep.Play();
                    System.Threading.Thread.Sleep(200);
                    if (!_alarmActive)
                        break;
                }
            }
            catch { }
        }

        /// <summary>
        /// Stops the alarm.
        /// </summary>
        private void StopAlarm() {
            _alarmActive = false;
            Log.Write("Color alarm stopped");
            try {
                if (_mediaPlayer != null) {
                    _mediaPlayer.Stop();
                }
            }
            catch { }
        }

        protected override void Shutdown() {
            Enabled = false;
            if (_alarmActive)
                StopAlarm();
            if (_mediaPlayer != null) {
                _mediaPlayer.Close();
                _mediaPlayer = null;
            }
        }
    }
}
