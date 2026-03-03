using System;
using System.Drawing;

namespace OnTopReplica.MultiWindow {

    /// <summary>
    /// Represents a single window being monitored in multi-window mode.
    /// One window is designated as the "primary" (shown in ThumbnailPanel preview),
    /// all others are monitored in the background using the same region.
    /// </summary>
    class MonitoredWindow {

        public MonitoredWindow(WindowHandle handle) {
            WindowHandle = handle ?? throw new ArgumentNullException(nameof(handle));
            IsPrimary = false;
            IsColorDetectionEnabled = true;
            LastDetectionResult = false;
            LastDetectionTime = DateTime.MinValue;
            LastIconDetected = false;
        }

        /// <summary>
        /// The window handle being monitored.
        /// </summary>
        public WindowHandle WindowHandle { get; private set; }

        /// <summary>
        /// Whether this is the primary window (shown in ThumbnailPanel real-time preview).
        /// Only one window can be primary at a time.
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Whether color detection is enabled for this window.
        /// </summary>
        public bool IsColorDetectionEnabled { get; set; }

        /// <summary>
        /// Result of the last color detection scan.
        /// </summary>
        public bool LastDetectionResult { get; set; }

        /// <summary>
        /// Timestamp of the last color detection scan.
        /// </summary>
        public DateTime LastDetectionTime { get; set; }

        /// <summary>
        /// Whether the reference icon was detected in the last scan.
        /// </summary>
        public bool LastIconDetected { get; set; }

        /// <summary>
        /// Gets the window title for display.
        /// </summary>
        public string Title {
            get { return WindowHandle.Title ?? "(unknown)"; }
        }

        /// <summary>
        /// Gets the window icon for display.
        /// </summary>
        public Icon Icon {
            get { return WindowHandle.Icon; }
        }

        public override bool Equals(object obj) {
            if (obj is MonitoredWindow other)
                return WindowHandle.Handle == other.WindowHandle.Handle;
            return false;
        }

        public override int GetHashCode() {
            return WindowHandle.Handle.GetHashCode();
        }

        public override string ToString() {
            return string.Format("[{0}] {1}{2}", 
                WindowHandle.Handle, Title, 
                IsPrimary ? " (Primary)" : "");
        }
    }
}
