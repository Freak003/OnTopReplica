using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OnTopReplica.Properties;
using OnTopReplica.MessagePumpProcessors;

namespace OnTopReplica.SidePanels {
    partial class ColorAlertPanel : SidePanel {

        public ColorAlertPanel() {
            InitializeComponent();
            LocalizePanel();
        }

        private void LocalizePanel() {
            groupColor.Text = "Color Alert";
            labelTargetColor.Text = "Target Color:";
            labelTolerance.Text = "Color Tolerance:";
            labelInterval.Text = "Sample Interval (ms):";
            checkEnabled.Text = "Enable Color Detection";
            
            tooltipInfo.SetToolTip(labelTolerance, "Higher value = more colors match (0-255)");
            tooltipInfo.SetToolTip(labelInterval, "Time between color checks in milliseconds");
        }

        ColorDetectionProcessor _processor;
        private Color _selectedColor = Color.Red;

        public override void OnFirstShown(MainForm form) {
            base.OnFirstShown(form);

            // Get or create the color detection processor
            try {
                _processor = form.MessagePumpManager.Get<ColorDetectionProcessor>();
            }
            catch {
                // Processor not found, will be added later
                _processor = null;
            }

            // Initialize controls from processor state
            if (_processor != null) {
                _selectedColor = _processor.TargetColor;
                checkEnabled.Checked = _processor.Enabled;
                trackBarTolerance.Value = _processor.ColorTolerance;
                numInterval.Value = _processor.SampleInterval;
                UpdateColorPreview();
            }
        }

        public override string Title {
            get {
                return "Color Alert";
            }
        }

        public override void OnClosing(MainForm form) {
            base.OnClosing(form);

            if (_processor != null) {
                _processor.TargetColor = _selectedColor;
                _processor.ColorTolerance = trackBarTolerance.Value;
                _processor.SampleInterval = (int)numInterval.Value;
                _processor.Enabled = checkEnabled.Checked;
            }
        }

        private void UpdateColorPreview() {
            panelColorPreview.BackColor = _selectedColor;
            labelColorValue.Text = string.Format("#{0:X6}", _selectedColor.ToArgb() & 0xFFFFFF);
        }

        private void BtnChooseColor_Click(object sender, EventArgs e) {
            using (ColorDialog dlg = new ColorDialog()) {
                dlg.Color = _selectedColor;
                dlg.AllowFullOpen = true;
                
                if (dlg.ShowDialog(this) == DialogResult.OK) {
                    _selectedColor = dlg.Color;
                    UpdateColorPreview();
                }
            }
        }

        private void TrackBarTolerance_ValueChanged(object sender, EventArgs e) {
            labelToleranceValue.Text = trackBarTolerance.Value.ToString();
        }

        private void CheckEnabled_CheckedChanged(object sender, EventArgs e) {
            // UI will be updated when panel closes
        }

        private void BtnClose_Click(object sender, EventArgs e) {
            OnRequestClosing();
        }
    }
}
