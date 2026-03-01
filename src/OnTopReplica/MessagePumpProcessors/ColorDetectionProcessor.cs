using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using OnTopReplica.Native;
using OnTopReplica.Properties;

namespace OnTopReplica.MessagePumpProcessors {
    /// <summary>
    /// Monitors the cloned window for a specific color and triggers an alarm when detected.
    /// </summary>
    class ColorDetectionProcessor : BaseMessagePumpProcessor {

        private bool _enabled = false;
        private List<Color> _targetColors = new List<Color>() { Color.Red };
        private int _colorTolerance = 30; // Tolerance for color matching (0-255)
        private int _sampleInterval = 500; // Sampling interval in milliseconds
        private float _alarmVolume = 1.0f; // 0.0 - 1.0
        private string _alarmSoundFile = string.Empty;
        private long _lastSampleTick = 0;
        private bool _alarmActive = false;
        private long _alarmStartTick = 0;
        private const int AlarmDuration = 3000; // 3 seconds in milliseconds
        private System.Windows.Media.MediaPlayer _mediaPlayer;

        public bool Enabled {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// Primary color for backwards compatibility. Setting this will clear the list and
        /// keep only the supplied value as the single target color.
        /// </summary>
        public Color TargetColor {
            get { return _targetColors.Count > 0 ? _targetColors[0] : Color.Red; }
            set {
                _targetColors.Clear();
                _targetColors.Add(value);
            }
        }

        /// <summary>
        /// List of colors that should trigger the alarm when found in the window.
        /// </summary>
        public List<Color> TargetColors {
            get { return _targetColors; }
            set { _targetColors = value ?? new List<Color>(); }
        }

        public int ColorTolerance {
            get { return _colorTolerance; }
            set { _colorTolerance = Math.Max(0, Math.Min(255, value)); }
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
                // skip when disabled
                return false;
            }
            if (Form.CurrentThumbnailWindowHandle == null) {
                // nothing to monitor yet
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

            // Check for any of the target colors in the window
            Log.Write("Performing color detection (targets={0}, tol={1})", string.Join(",", _targetColors), _colorTolerance);
            if (DetectColorInWindow(Form.CurrentThumbnailWindowHandle.Handle)) {
                StartAlarm();
            }

            return false;
        }

        /// <summary>
        /// Detects if the target color exists in the monitored window.
        /// </summary>
        private bool DetectColorInWindow(IntPtr windowHandle) {
            try {
                // Get window rectangle
                Rectangle rect;
                if (!WindowManagerMethods.GetClientRect(windowHandle, out rect)) {
                    return false;
                }
                
                if (rect.Right <= 0 || rect.Bottom <= 0)
                    return false;

                // Create a bitmap from the window's screen content
                Bitmap bmp = null;
                try {
                    // Get screen coordinates
                    var scrRect = WindowManagerMethods.ClientToScreenRect(windowHandle, rect);
                    
                    if (scrRect.Width <= 0 || scrRect.Height <= 0)
                        return false;

                    // Limit bitmap size for performance
                    int maxWidth = Math.Min(scrRect.Width, 800);
                    int maxHeight = Math.Min(scrRect.Height, 600);
                    
                    // Create bitmap from screen
                    bmp = new Bitmap(maxWidth, maxHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    Graphics g = Graphics.FromImage(bmp);
                    g.CopyFromScreen(scrRect.X, scrRect.Y, 0, 0, new Size(maxWidth, maxHeight));
                    g.Dispose();

                    // Sample pixels to detect color
                    return SampleBitmapForColor(bmp);
                }
                finally {
                    if (bmp != null)
                        bmp.Dispose();
                }
            }
            catch (Exception ex) {
                Log.Write("Error during color detection: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Samples bitmap pixels to find the target color.
        /// </summary>
        private bool SampleBitmapForColor(Bitmap bmp) {
            if (bmp == null || bmp.Width <= 0 || bmp.Height <= 0)
                return false;

            // Scan every pixel for maximum reliability (bitmap is capped to 800x600 for performance)
            for (int y = 0; y < bmp.Height; y++) {
                for (int x = 0; x < bmp.Width; x++) {
                    Color pixelColor = bmp.GetPixel(x, y);
                    foreach (var target in _targetColors) {
                        if (IsColorMatch(pixelColor, target, _colorTolerance)) {
                            Log.Write("Color {2} detected at ({0}, {1})", x, y, target);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if two colors match within the tolerance threshold.
        /// </summary>
        private bool IsColorMatch(Color color1, Color color2, int tolerance) {
            int rDiff = Math.Abs(color1.R - color2.R);
            int gDiff = Math.Abs(color1.G - color2.G);
            int bDiff = Math.Abs(color1.B - color2.B);

            return rDiff <= tolerance && gDiff <= tolerance && bDiff <= tolerance;
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
