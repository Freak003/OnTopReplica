using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OnTopReplica.Properties;
using OnTopReplica.MessagePumpProcessors;

namespace OnTopReplica.SidePanels {
    partial class ColorAlertPanel : SidePanel {

        public ColorAlertPanel() {
            InitializeComponent();
            LocalizePanel();
        }

        private void LocalizePanel() {
            // use resource strings where possible for translation support
            groupColor.Text = Strings.ColorAlert_Title;
            labelTargetColor.Text = Strings.ColorAlert_TargetColor;
            labelTolerance.Text = Strings.ColorAlert_Tolerance;
            labelInterval.Text = Strings.ColorAlert_Interval;
            checkEnabled.Text = Strings.ColorAlert_Enable;
            btnChooseColor.Text = Strings.ColorAlert_Choose;
            labelVolume.Text = Strings.ColorAlert_Volume;
            labelSoundFile.Text = Strings.ColorAlert_SoundFile;
            btnClose.Text = Strings.MenuClose;

            tooltipInfo.SetToolTip(labelTolerance, Strings.ColorAlert_ToleranceTooltip);
            tooltipInfo.SetToolTip(labelInterval, Strings.ColorAlert_IntervalTooltip);
        }

        ColorDetectionProcessor _processor;
        private Color _selectedColor = Color.Red;
        private List<Color> _targetColors = new List<Color>();
        private bool _loading = false; // prevent recursion during init
        private const string SoundsRelativePath = "Sounds";
        private const string ColorConfigFileName = "ColorAlertConfig.json";

        private string GetColorConfigPath() {
            // store config in AppData\OnTopReplica subfolder for user-specific settings
            return Path.Combine(AppPaths.PrivateRoamingFolderPath, ColorConfigFileName);
        }

        public override void OnFirstShown(MainForm form) {
            base.OnFirstShown(form);
            Log.Write("ColorAlertPanel shown size {0}", this.Size);

            // Get or create the color detection processor; if panel is shown before the manager is ready
            // this may return null, so we still want the UI to function independently.
            try {
                _processor = form.MessagePumpManager.Get<ColorDetectionProcessor>();
            }
            catch {
                _processor = null;
            }

            // Initialize color/tolerance/interval only if processor exists (real state)
            if (_processor != null) {
                // migrate processor to support multiple colors
                _targetColors = new List<Color>(_processor.TargetColors);
                if (_targetColors.Count > 0)
                    _selectedColor = _targetColors[0];
                checkEnabled.Checked = _processor.Enabled;
                trackBarTolerance.Value = _processor.ColorTolerance;
                numInterval.Value = _processor.SampleInterval;
                UpdateColorPreview();
            }

            // populate the color list textbox either from processor or previous settings
            if (_targetColors.Count == 0 && !string.IsNullOrEmpty(Settings.Default.ColorAlertTargetColors)) {
                ParseColorsFromString(Settings.Default.ColorAlertTargetColors);
            }
            UpdateColorListText();

            // also try loading from dedicated JSON config file (newer format)
            if (_targetColors.Count == 0) {
                LoadColorsFromFile();
            }

            if (_targetColors.Count > 0) {
                var list = string.Join(",", _targetColors.Select(c => string.Format("#{0:X6}", c.ToArgb() & 0xFFFFFF)));
                Log.Write("Loaded ColorAlert target colors: {0}", list);
            }

            // irrespective of processor availability, load persisted volume and sound settings,
            // and populate the dropdown so the UI shows something immediately.
            trackBarVolume.Value = (int)(Settings.Default.ColorAlertVolume * 100);
            PopulateSoundList();
            if (!string.IsNullOrEmpty(Settings.Default.ColorAlertSoundFile)) {
                var f = Path.GetFileName(Settings.Default.ColorAlertSoundFile);
                if (comboSound.Items.Contains(f))
                    comboSound.SelectedItem = f;
            }

            // log current control list for diagnostics
            var names = new List<string>();
            foreach (Control c in groupColor.Controls) names.Add(c.Name);
            Log.Write("groupColor contains: {0}", string.Join(",", names));
        }

        public override string Title {
            get {
                return "Color Alert";
            }
        }

        public override void OnClosing(MainForm form) {
            base.OnClosing(form);

            // update processor if available
            if (_processor != null) {
                _processor.TargetColors = new List<Color>(_targetColors);
                _processor.ColorTolerance = trackBarTolerance.Value;
                _processor.SampleInterval = (int)numInterval.Value;
                _processor.Enabled = checkEnabled.Checked;
                _processor.AlarmVolume = trackBarVolume.Value / 100f;
                if (comboSound.SelectedItem != null) {
                    string file = comboSound.SelectedItem.ToString();
                    _processor.AlarmSoundFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), SoundsRelativePath, file);
                }
            }

            // persist settings regardless of processor presence
            Settings.Default.ColorAlertVolume = trackBarVolume.Value / 100f;
            if (comboSound.SelectedItem != null) {
                string file = comboSound.SelectedItem.ToString();
                Settings.Default.ColorAlertSoundFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), SoundsRelativePath, file);
            }
            // persist color list as comma-separated hex values
            Settings.Default.ColorAlertTargetColors = ColorsToString(_targetColors);
            
            // also persist to dedicated JSON file for better manageability
            SaveColorsToFile();
            
            Settings.Default.Save();
        }

        private void SaveColorsToFile() {
            try {
                string path = GetColorConfigPath();
                var json = ColorsToString(_targetColors);
                File.WriteAllText(path, json, Encoding.UTF8);
                Log.Write("Saved color config to {0}", path);
            }
            catch (Exception ex) {
                Log.Write("Error saving color config: {0}", ex.Message);
            }
        }

        private void LoadColorsFromFile() {
            try {
                string path = GetColorConfigPath();
                if (!File.Exists(path)) return;
                string json = File.ReadAllText(path, Encoding.UTF8);
                ParseColorsFromString(json);
                if (_targetColors.Count > 0) {
                    Log.Write("Loaded color config from {0}", path);
                }
            }
            catch (Exception ex) {
                Log.Write("Error loading color config: {0}", ex.Message);
            }
        }

        private void UpdateColorPreview() {
            panelColorPreview.BackColor = _selectedColor;
            labelColorValue.Text = string.Format("#{0:X6}", _selectedColor.ToArgb() & 0xFFFFFF);
        }

        private void UpdateColorListText() {
            if (lstColors == null) return;
            lstColors.BeginUpdate();
            try {
                lstColors.Items.Clear();
                foreach (var c in _targetColors) {
                    // store Color objects in the list so owner-draw can render them
                    lstColors.Items.Add(c);
                }
            }
            finally {
                lstColors.EndUpdate();
            }
        }

        private string ColorsToString(List<Color> colors) {
            // encode as simple JSON array of hex strings for forward-compatibility
            var sb = new StringBuilder();
            sb.Append('[');
            for (int i = 0; i < colors.Count; i++) {
                if (i > 0) sb.Append(',');
                sb.Append('"');
                sb.AppendFormat("#{0:X6}", colors[i].ToArgb() & 0xFFFFFF);
                sb.Append('"');
            }
            sb.Append(']');
            return sb.ToString();
        }

        private void ParseColorsFromString(string text) {
            _targetColors.Clear();
            if (string.IsNullOrWhiteSpace(text)) return;
            string trimmed = text.Trim();
            try {
                // JSON array: ["#RRGGBB","#RRGGBB"]
                if (trimmed.StartsWith("[")) {
                    trimmed = trimmed.Trim('[', ']');
                    var parts = trimmed.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in parts) {
                        string s = p.Trim().Trim('"').Trim();
                        if (s.StartsWith("#")) s = s.Substring(1);
                        if (s.Length == 6) {
                            int rgb = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                            Color c = Color.FromArgb(255, (rgb>>16)&0xFF, (rgb>>8)&0xFF, rgb&0xFF);
                            _targetColors.Add(c);
                        }
                    }
                    return;
                }
            }
            catch { /* fallthrough to legacy format */ }

            // legacy comma/semicolon separated format
            var partsLegacy = text.Split(new[] {',',';'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in partsLegacy) {
                string s = part.Trim();
                try {
                    if (s.StartsWith("#")) s = s.Substring(1);
                    if (s.Length == 6) {
                        int rgb = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                        Color c = Color.FromArgb(255, (rgb>>16)&0xFF, (rgb>>8)&0xFF, rgb&0xFF);
                        _targetColors.Add(c);
                    }
                }
                catch { }
            }
            if (_targetColors.Count > 0) {
                _selectedColor = _targetColors[0];
                UpdateColorPreview();
            }
        }

        private void BtnChooseColor_Click(object sender, EventArgs e) {
            using (ColorDialog dlg = new ColorDialog()) {
                dlg.Color = _selectedColor;
                dlg.AllowFullOpen = true;
                
                if (dlg.ShowDialog(this) == DialogResult.OK) {
                    _selectedColor = dlg.Color;
                    UpdateColorPreview();
                    AddColorToList(_selectedColor);
                }
            }
        }

        private void AddColorToList(Color c) {
            if (!_targetColors.Contains(c)) {
                _targetColors.Add(c);
                UpdateColorListText();
                if (_processor != null)
                    _processor.TargetColors = new List<Color>(_targetColors);
                Log.Write("Added color to alert list: #{0:X6}", c.ToArgb() & 0xFFFFFF);
            }
        }

        private void BtnAddColor_Click(object sender, EventArgs e) {
            using (ColorDialog dlg = new ColorDialog()) {
                dlg.Color = _selectedColor;
                dlg.AllowFullOpen = true;
                if (dlg.ShowDialog(this) == DialogResult.OK) {
                    AddColorToList(dlg.Color);
                }
            }
        }

        private void BtnMoveUp_Click(object sender, EventArgs e) {
            MoveSelected(-1);
        }

        private void BtnMoveDown_Click(object sender, EventArgs e) {
            MoveSelected(1);
        }

        private void MoveSelected(int direction) {
            if (lstColors == null) return;
            int idx = lstColors.SelectedIndex;
            if (idx < 0) return;
            int newIdx = idx + direction;
            if (newIdx < 0 || newIdx >= _targetColors.Count) return;
            var item = _targetColors[idx];
            _targetColors.RemoveAt(idx);
            _targetColors.Insert(newIdx, item);
            UpdateColorListText();
            lstColors.SelectedIndex = newIdx;
            if (_processor != null) _processor.TargetColors = new List<Color>(_targetColors);
            Log.Write("Moved color #{0:X6} to index {1}", item.ToArgb() & 0xFFFFFF, newIdx);
        }

        private void BtnRemoveColor_Click(object sender, EventArgs e) {
            if (lstColors == null) return;
            if (lstColors.SelectedIndex < 0) return;
            int idx = lstColors.SelectedIndex;
            if (idx >= 0 && idx < _targetColors.Count) {
                var removed = _targetColors[idx];
                _targetColors.RemoveAt(idx);
                UpdateColorListText();
                if (_processor != null) _processor.TargetColors = new List<Color>(_targetColors);
                Log.Write("Removed color from alert list: #{0:X6}", removed.ToArgb() & 0xFFFFFF);
            }
        }

        private void LstColors_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index < 0) return;
            e.DrawBackground();
            var lb = sender as ListBox;
            object item = lb.Items[e.Index];
            Color c = Color.Empty;
            if (item is Color) c = (Color)item;
            else {
                // fallback: try parse string
                try {
                    string s = item.ToString();
                    if (s.StartsWith("#")) s = s.Substring(1);
                    int rgb = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                    c = Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
                }
                catch { c = Color.Black; }
            }

            // draw color swatch
            Rectangle swatch = new Rectangle(e.Bounds.Left + 2, e.Bounds.Top + 2, 16, e.Bounds.Height - 4);
            using (var brush = new SolidBrush(c)) {
                e.Graphics.FillRectangle(brush, swatch);
            }
            e.Graphics.DrawRectangle(SystemPens.ControlDark, swatch);

            // draw hex text
            string hex = string.Format("#{0:X6}", c.ToArgb() & 0xFFFFFF);
            using (var textBrush = new SolidBrush(e.ForeColor)) {
                var textRect = new Rectangle(e.Bounds.Left + 22, e.Bounds.Top + 2, e.Bounds.Width - 24, e.Bounds.Height - 4);
                e.Graphics.DrawString(hex, e.Font, textBrush, textRect);
            }

            e.DrawFocusRectangle();
        }

        private void TrackBarTolerance_ValueChanged(object sender, EventArgs e) {
            labelToleranceValue.Text = trackBarTolerance.Value.ToString();
            if (_processor != null) {
                _processor.ColorTolerance = trackBarTolerance.Value;
            }
        }

        private void TrackBarVolume_Scroll(object sender, EventArgs e) {
            if (_loading) return;
            if (_processor != null) {
                _processor.AlarmVolume = trackBarVolume.Value / 100f;
                Log.Write("Alarm volume set to {0}", _processor.AlarmVolume);
            }
        }

        private void CheckEnabled_CheckedChanged(object sender, EventArgs e) {
            if (_processor != null) {
                _processor.Enabled = checkEnabled.Checked;
            }
        }

        private void BtnClose_Click(object sender, EventArgs e) {
            OnRequestClosing();
        }

        private void NumInterval_ValueChanged(object sender, EventArgs e) {
            if (_processor != null) {
                _processor.SampleInterval = (int)numInterval.Value;
            }
        }

        private void ComboSound_SelectedIndexChanged(object sender, EventArgs e) {
            if (_processor != null && comboSound.SelectedItem != null) {
                string file = comboSound.SelectedItem.ToString();
                string path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), SoundsRelativePath, file);
                _processor.AlarmSoundFile = path;
            }
        }

        private void PopulateSoundList() {
            comboSound.Items.Clear();
            string dir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), SoundsRelativePath);
            if (Directory.Exists(dir)) {
                foreach (var ext in new[] {"*.wav", "*.mp3"}) {
                    foreach (var f in Directory.GetFiles(dir, ext)) {
                        comboSound.Items.Add(Path.GetFileName(f));
                    }
                }
            }
            // write debug info so we can verify enumeration succeeded at runtime
            Log.Write("PopulateSoundList found {0} files", comboSound.Items.Count);
        }

        private void BtnTestAlarm_Click(object sender, EventArgs e) {
            // Play alarm using current settings: selected sound file and volume
            if (comboSound.SelectedItem != null) {
                string file = comboSound.SelectedItem.ToString();
                string path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), SoundsRelativePath, file);
                float volume = trackBarVolume.Value / 100f;
                try {
                    var player = new System.Windows.Media.MediaPlayer();
                    player.Open(new Uri(path));
                    player.Volume = Math.Max(0, Math.Min(1, volume));
                    player.Play();
                    Log.Write("Test alarm: playing {0} at volume {1}", file, volume);
                }
                catch (Exception ex) {
                    Log.Write("Test alarm error: {0}", ex.Message);
                }
            }
            else {
                Log.Write("Test alarm: no sound file selected");
            }
        }
    }
}
