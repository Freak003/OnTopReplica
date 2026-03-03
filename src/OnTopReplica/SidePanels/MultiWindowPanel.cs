using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OnTopReplica.MultiWindow;
using OnTopReplica.MessagePumpProcessors;
using OnTopReplica.WindowSeekers;

namespace OnTopReplica.SidePanels {

    /// <summary>
    /// Side panel for configuring multi-window monitoring.
    /// Includes independent color detection settings and icon template matching.
    /// </summary>
    partial class MultiWindowPanel : SidePanel {

        private MultiWindowManager _manager;
        private ImageList _imageList;

        public MultiWindowPanel() {
            InitializeComponent();
        }

        public override string Title {
            get { return "多窗口监控"; }
        }

        public override void OnFirstShown(MainForm form) {
            base.OnFirstShown(form);

            _manager = form.MultiWindowManager;
            _manager.CaptureUIDispatcher();
            _manager.WindowListChanged += RefreshList;
            _manager.IconDisappearedAlarm += OnIconDisappearedAlarm;

            LoadAvailableWindows();
            SyncCheckStatesFromManager();
            SyncColorSettingsFromManager();
            SyncIconSettingsFromManager();
            UpdateStatus();
        }

        public override void OnClosing(MainForm form) {
            base.OnClosing(form);

            if (_manager != null) {
                _manager.WindowListChanged -= RefreshList;
                _manager.IconDisappearedAlarm -= OnIconDisappearedAlarm;
            }
        }

        #region Window List

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

                item.SubItems.Add("");
                item.SubItems.Add(IsPrimaryWindow(w) ? "\u2605" : "");

                listWindows.Items.Add(item);
            }

            listWindows.SmallImageList = _imageList;
        }

        private bool IsPrimaryWindow(WindowHandle handle) {
            var primary = _manager.PrimaryWindow;
            return primary != null && primary.WindowHandle.Handle == handle.Handle;
        }

        private void SyncCheckStatesFromManager() {
            _updatingChecks = true;
            try {
                foreach (ListViewItem item in listWindows.Items) {
                    var wh = (WindowHandle)item.Tag;
                    bool isMonitored = _manager.Windows.Any(mw => mw.WindowHandle.Handle == wh.Handle);
                    item.Checked = isMonitored;

                    bool isPrimary = IsPrimaryWindow(wh);
                    if (item.SubItems.Count > 2)
                        item.SubItems[2].Text = isPrimary ? "\u2605" : "";
                }
            }
            finally {
                _updatingChecks = false;
            }
        }

        private bool _updatingChecks = false;

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

            BeginInvoke((Action)(() => {
                SyncCheckStatesFromManager();
                UpdateStatus();
            }));
        }

        private void listWindows_DoubleClick(object sender, EventArgs e) {
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

        #endregion

        #region Color Detection Settings

        private bool _updatingColorSettings = false;

        private void SyncColorSettingsFromManager() {
            _updatingColorSettings = true;
            try {
                chkColorEnabled.Checked = _manager.ColorDetectionEnabled;
                chkRed.Checked = _manager.EnabledCategories.Contains(ColorCategory.Red);
                chkOrange.Checked = _manager.EnabledCategories.Contains(ColorCategory.Orange);
                chkGray.Checked = _manager.EnabledCategories.Contains(ColorCategory.Gray);

                chkRed.Enabled = _manager.ColorDetectionEnabled;
                chkOrange.Enabled = _manager.ColorDetectionEnabled;
                chkGray.Enabled = _manager.ColorDetectionEnabled;
            }
            finally {
                _updatingColorSettings = false;
            }
        }

        private void chkColorEnabled_CheckedChanged(object sender, EventArgs e) {
            if (_updatingColorSettings) return;
            _manager.ColorDetectionEnabled = chkColorEnabled.Checked;
            chkRed.Enabled = chkColorEnabled.Checked;
            chkOrange.Enabled = chkColorEnabled.Checked;
            chkGray.Enabled = chkColorEnabled.Checked;
        }

        private void chkRed_CheckedChanged(object sender, EventArgs e) {
            if (_updatingColorSettings) return;
            _manager.SetCategoryEnabled(ColorCategory.Red, chkRed.Checked);
        }

        private void chkOrange_CheckedChanged(object sender, EventArgs e) {
            if (_updatingColorSettings) return;
            _manager.SetCategoryEnabled(ColorCategory.Orange, chkOrange.Checked);
        }

        private void chkGray_CheckedChanged(object sender, EventArgs e) {
            if (_updatingColorSettings) return;
            _manager.SetCategoryEnabled(ColorCategory.Gray, chkGray.Checked);
        }

        #endregion

        #region Icon Detection Settings

        private void SyncIconSettingsFromManager() {
            chkIconEnabled.Checked = _manager.IconDetectionEnabled;
            LoadAlarmSounds();
            UpdateIconPreview();
            UpdateIconControlsEnabled();
        }

        private void UpdateIconControlsEnabled() {
            bool enabled = chkIconEnabled.Checked;
            btnCaptureIcon.Enabled = enabled;
            btnLoadIcon.Enabled = enabled;
            btnClearIcon.Enabled = enabled && _manager.IconTemplate != null;
            picIconPreview.Enabled = enabled;
        }

        private void UpdateIconPreview() {
            if (_manager.IconTemplate != null) {
                picIconPreview.Image = _manager.IconTemplate;
                picIconPreview.SizeMode = PictureBoxSizeMode.Zoom;
                lblIconStatus.Text = string.Format("模板: {0}x{1}", _manager.IconTemplate.Width, _manager.IconTemplate.Height);
            }
            else {
                picIconPreview.Image = null;
                lblIconStatus.Text = "未设置模板";
            }
            btnClearIcon.Enabled = chkIconEnabled.Checked && _manager.IconTemplate != null;
        }

        private void chkIconEnabled_CheckedChanged(object sender, EventArgs e) {
            _manager.IconDetectionEnabled = chkIconEnabled.Checked;
            UpdateIconControlsEnabled();
        }

        /// <summary>
        /// Loads available sound files from the Sounds directory next to the executable
        /// and populates the alarm sound ComboBox.
        /// </summary>
        private void LoadAlarmSounds() {
            cmbAlarmSound.SelectedIndexChanged -= cmbAlarmSound_SelectedIndexChanged;
            cmbAlarmSound.Items.Clear();

            // Default: use system sound
            cmbAlarmSound.Items.Add(new SoundItem("（系统默认）", string.Empty));

            var soundsDir = Path.Combine(
                Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "Sounds");

            if (Directory.Exists(soundsDir)) {
                foreach (var file in Directory.GetFiles(soundsDir, "*.*")
                    .Where(f => {
                        var ext = Path.GetExtension(f).ToLowerInvariant();
                        return ext == ".mp3" || ext == ".wav" || ext == ".wma";
                    })
                    .OrderBy(f => f)) {
                    cmbAlarmSound.Items.Add(
                        new SoundItem(Path.GetFileNameWithoutExtension(file), file));
                }
            }

            // Select the item matching the current manager setting
            int selected = 0;
            for (int i = 0; i < cmbAlarmSound.Items.Count; i++) {
                var item = (SoundItem)cmbAlarmSound.Items[i];
                if (string.Equals(item.FilePath, _manager.AlarmSoundFile,
                        StringComparison.OrdinalIgnoreCase)) {
                    selected = i;
                    break;
                }
            }
            cmbAlarmSound.SelectedIndex = selected;
            cmbAlarmSound.SelectedIndexChanged += cmbAlarmSound_SelectedIndexChanged;
        }

        private void cmbAlarmSound_SelectedIndexChanged(object sender, EventArgs e) {
            if (cmbAlarmSound.SelectedItem is SoundItem item) {
                _manager.AlarmSoundFile = item.FilePath;
            }
        }

        /// <summary>Represents a selectable alarm sound file.</summary>
        private class SoundItem {
            public string DisplayName { get; }
            public string FilePath { get; }
            public SoundItem(string displayName, string filePath) {
                DisplayName = displayName;
                FilePath = filePath;
            }
            public override string ToString() { return DisplayName; }
        }

        /// <summary>
        /// Capture icon template from the current ThumbnailPanel preview.
        /// User selects a rectangular region by drawing on a snapshot.
        /// </summary>
        private void btnCaptureIcon_Click(object sender, EventArgs e) {
            if (ParentMainForm == null) return;

            // Take a snapshot of the current ThumbnailPanel
            Bitmap snapshot = null;
            try {
                var panel = ParentMainForm.ThumbnailPanel;
                if (panel == null || panel.Width <= 0 || panel.Height <= 0) {
                    MessageBox.Show("请先选择一个预览窗口。", "图形捕获", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // DWM thumbnails are rendered by the compositor and are invisible to GDI DrawToBitmap.
                // Capture the actual screen pixels at the ThumbnailPanel's screen location instead.
                snapshot = new Bitmap(panel.Width, panel.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var screenPt = panel.PointToScreen(Point.Empty);
                using (var g = Graphics.FromImage(snapshot)) {
                    g.CopyFromScreen(screenPt, Point.Empty, new Size(panel.Width, panel.Height));
                }
            }
            catch (Exception ex) {
                MessageBox.Show("截图失败: " + ex.Message, "图形捕获", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Show capture dialog
            using (var dlg = new IconCaptureDialog(snapshot)) {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.CapturedRegion != null) {
                    // Pass panel display dimensions so the manager can normalize
                    // captured full-res window to the same scale for matching.
                    var panel2 = ParentMainForm.ThumbnailPanel;
                    _manager.SetIconTemplate(dlg.CapturedRegion, panel2.Width, panel2.Height);
                    UpdateIconPreview();
                }
            }

            snapshot.Dispose();
        }

        /// <summary>
        /// Load icon template from file.
        /// </summary>
        private void btnLoadIcon_Click(object sender, EventArgs e) {
            using (var ofd = new OpenFileDialog()) {
                ofd.Title = "选择图形模板图片";
                ofd.Filter = "图片文件|*.png;*.bmp;*.jpg;*.jpeg;*.gif|所有文件|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK) {
                    try {
                        using (var img = new Bitmap(ofd.FileName)) {
                            // Use current ThumbnailPanel dimensions as normalization target.
                            var panel = ParentMainForm?.ThumbnailPanel;
                            int nw = panel != null ? panel.Width : 0;
                            int nh = panel != null ? panel.Height : 0;
                            _manager.SetIconTemplate(img, nw, nh);
                        }
                        UpdateIconPreview();
                    }
                    catch (Exception ex) {
                        MessageBox.Show("加载图片失败: " + ex.Message, "图形模板", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnClearIcon_Click(object sender, EventArgs e) {
            _manager.SetIconTemplate(null);
            UpdateIconPreview();
        }

        private void OnIconDisappearedAlarm() {
            if (InvokeRequired) {
                BeginInvoke((Action)OnIconDisappearedAlarm);
                return;
            }

            lblIconStatus.Text = "!! 图形已从所有窗口消失 !!";
            lblIconStatus.ForeColor = Color.Red;

            // Reset after 5 seconds
            var timer = new Timer { Interval = 5000 };
            timer.Tick += (s, ev) => {
                timer.Stop();
                timer.Dispose();
                if (!IsDisposed) {
                    lblIconStatus.ForeColor = SystemColors.ControlText;
                    UpdateIconPreview();
                }
            };
            timer.Start();
        }

        #endregion

        #region Buttons

        private void btnToggleMonitor_Click(object sender, EventArgs e) {
            if (_manager.IsActive) {
                _manager.StopDetection();
            } else {
                if (_manager.Windows.Count == 0) {
                    MessageBox.Show("请先勾选至少一个要监控的窗口。",
                        "多窗口监控", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                bool hasColor = _manager.ColorDetectionEnabled && _manager.EnabledCategories.Count > 0;
                bool hasIcon = _manager.IconDetectionEnabled && _manager.IconTemplate != null;

                if (!hasColor && !hasIcon) {
                    MessageBox.Show("请至少启用一种检测方式（颜色检测或图形检测）。",
                        "多窗口监控", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _manager.StartDetection();
            }
            UpdateStatus();
        }

        private void btnRefresh_Click(object sender, EventArgs e) {
            LoadAvailableWindows();
            SyncCheckStatesFromManager();
        }

        private void btnClose_Click(object sender, EventArgs e) {
            OnRequestClosing();
        }

        #endregion

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
            string primaryName = primary != null ? primary.Title : "（无）";
            labelStatus.Text = string.Format("监控中：{0} 个窗口  |  主窗口：{1}",
                total, primaryName);

            btnToggleMonitor.Text = _manager.IsActive ? "停止监控" : "开始监控";
        }
    }
}
