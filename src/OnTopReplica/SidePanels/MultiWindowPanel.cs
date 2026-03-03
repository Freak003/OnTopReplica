using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OnTopReplica.MultiWindow;
using OnTopReplica.MessagePumpProcessors;
using OnTopReplica.WindowSeekers;

namespace OnTopReplica.SidePanels {

    /// <summary>
    /// Side panel for configuring multi-window monitoring.
    /// Shows a list of available windows with checkboxes for selection,
    /// radio-style selection for primary window, and controls for
    /// enabling/disabling color detection per window.
    /// </summary>
    partial class MultiWindowPanel : SidePanel {

        private MultiWindowManager _manager;
        private ImageList _imageList;

        public MultiWindowPanel() {
            InitializeComponent();
        }

        public override string Title {
            get { return "Multi-Window Monitor"; }
        }

        public override void OnFirstShown(MainForm form) {
            base.OnFirstShown(form);

            _manager = form.MultiWindowManager;
            _manager.WindowListChanged += RefreshList;

            LoadAvailableWindows();
            SyncCheckStatesFromManager();
            UpdateStatus();
        }

        public override void OnClosing(MainForm form) {
            base.OnClosing(form);

            if (_manager != null) {
                _manager.WindowListChanged -= RefreshList;
            }
        }

        /// <summary>
        /// Loads available system windows into the ListView.
        /// </summary>
        private void LoadAvailableWindows() {
            listWindows.Items.Clear();
            if (_imageList != null) _imageList.Dispose();

            _imageList = new ImageList();
            _imageList.ImageSize = new Size(16, 16);
            _imageList.ColorDepth = ColorDepth.Depth32Bit;

            var seeker = new TaskWindowSeeker {
                SkipNotVisibleWindows = true
            };
            seeker.Refresh();

            foreach (var w in seeker.Windows) {
                var item = new ListViewItem(w.Title) {
                    Tag = w,
                    Checked = _manager.Windows.Any(mw => mw.WindowHandle.Handle == w.Handle)
                };

                if (w.Icon != null) {
                    _imageList.Images.Add(w.Handle.ToString(), w.Icon);
                    item.ImageKey = w.Handle.ToString();
                }

                // SubItems: [0]=Title, [1]=Status, [2]=Primary
                item.SubItems.Add(""); // Status column
                item.SubItems.Add(IsPrimaryWindow(w) ? "★" : ""); // Primary indicator

                listWindows.Items.Add(item);
            }

            listWindows.SmallImageList = _imageList;
        }

        private bool IsPrimaryWindow(WindowHandle handle) {
            var primary = _manager.PrimaryWindow;
            return primary != null && primary.WindowHandle.Handle == handle.Handle;
        }

        /// <summary>
        /// Syncs checkbox states from the manager's current list.
        /// </summary>
        private void SyncCheckStatesFromManager() {
            _updatingChecks = true;
            try {
                foreach (ListViewItem item in listWindows.Items) {
                    var wh = (WindowHandle)item.Tag;
                    bool isMonitored = _manager.Windows.Any(mw => mw.WindowHandle.Handle == wh.Handle);
                    item.Checked = isMonitored;

                    bool isPrimary = IsPrimaryWindow(wh);
                    if (item.SubItems.Count > 2)
                        item.SubItems[2].Text = isPrimary ? "★" : "";
                }
            }
            finally {
                _updatingChecks = false;
            }
        }

        private bool _updatingChecks = false;

        /// <summary>
        /// Handles the checkbox ItemCheck event to add/remove windows from monitoring.
        /// </summary>
        private void listWindows_ItemCheck(object sender, ItemCheckEventArgs e) {
            if (_updatingChecks) return;

            var item = listWindows.Items[e.Index];
            var handle = (WindowHandle)item.Tag;

            if (e.NewValue == CheckState.Checked) {
                _manager.AddWindow(handle);
            }
            else {
                _manager.RemoveWindow(handle);
            }

            // Defer UI update to avoid flicker during event
            BeginInvoke((Action)(() => {
                SyncCheckStatesFromManager();
                UpdateStatus();
            }));
        }

        /// <summary>
        /// Double-click sets a window as primary.
        /// </summary>
        private void listWindows_DoubleClick(object sender, EventArgs e) {
            if (listWindows.SelectedItems.Count == 0) return;

            var item = listWindows.SelectedItems[0];
            var handle = (WindowHandle)item.Tag;

            // Ensure it's in the monitored list first
            if (!_manager.Windows.Any(mw => mw.WindowHandle.Handle == handle.Handle)) {
                _manager.AddWindow(handle);
            }

            _manager.SetPrimary(handle);
            SyncCheckStatesFromManager();
            UpdateStatus();
        }

        /// <summary>
        /// "Set Primary" button click handler.
        /// </summary>
        private void btnSetPrimary_Click(object sender, EventArgs e) {
            if (listWindows.SelectedItems.Count == 0) return;

            var item = listWindows.SelectedItems[0];
            var handle = (WindowHandle)item.Tag;

            if (!_manager.Windows.Any(mw => mw.WindowHandle.Handle == handle.Handle)) {
                _manager.AddWindow(handle);
            }

            _manager.SetPrimary(handle);
            SyncCheckStatesFromManager();
            UpdateStatus();
        }

        /// <summary>
        /// "Start Monitor" button click handler — begins multi-window detection.
        /// </summary>
        private void btnStartMonitor_Click(object sender, EventArgs e) {
            if (_manager.Windows.Count == 0) {
                MessageBox.Show("Please check at least one window to monitor.",
                    "Multi-Window Monitor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Ensure ColorDetectionProcessor is enabled
            ColorDetectionProcessor processor = null;
            try {
                processor = ParentMainForm.MessagePumpManager.Get<ColorDetectionProcessor>();
            }
            catch { }

            if (processor == null || !processor.Enabled) {
                MessageBox.Show("Please enable Color Alert first (it provides the color detection settings).",
                    "Multi-Window Monitor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _manager.StartDetection();
            UpdateStatus();
        }

        /// <summary>
        /// "Stop Monitor" button click handler.
        /// </summary>
        private void btnStopMonitor_Click(object sender, EventArgs e) {
            _manager.StopDetection();
            UpdateStatus();
        }

        /// <summary>
        /// "Refresh" button — reload available windows.
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e) {
            LoadAvailableWindows();
            SyncCheckStatesFromManager();
        }

        /// <summary>
        /// "Close" button.
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e) {
            OnRequestClosing();
        }

        private void RefreshList() {
            if (InvokeRequired) {
                BeginInvoke((Action)RefreshList);
                return;
            }
            SyncCheckStatesFromManager();
            UpdateStatus();
        }

        private void UpdateStatus() {
            int total = _manager.Windows.Count;
            var primary = _manager.PrimaryWindow;
            string primaryName = primary != null ? primary.Title : "(none)";
            labelStatus.Text = string.Format("Monitored: {0} window(s) | Primary: {1}",
                total, primaryName);
        }
    }
}
