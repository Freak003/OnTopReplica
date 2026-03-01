namespace OnTopReplica.SidePanels {
    partial class ColorAlertPanel {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.groupColor = new System.Windows.Forms.GroupBox();
            this.checkEnabled = new System.Windows.Forms.CheckBox();
            this.labelColorValue = new System.Windows.Forms.Label();
            this.panelColorPreview = new System.Windows.Forms.Panel();
            this.btnChooseColor = new System.Windows.Forms.Button();
            this.labelTargetColor = new System.Windows.Forms.Label();
            this.labelTolerance = new System.Windows.Forms.Label();
            this.trackBarTolerance = new System.Windows.Forms.TrackBar();
            this.labelToleranceValue = new System.Windows.Forms.Label();
            this.labelInterval = new System.Windows.Forms.Label();
            this.numInterval = new System.Windows.Forms.NumericUpDown();
            this.labelIntervalUnit = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.tooltipInfo = new System.Windows.Forms.ToolTip(this.components);
            
            this.groupColor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).BeginInit();
            this.SuspendLayout();
            
            // groupColor
            this.groupColor.Controls.Add(this.checkEnabled);
            this.groupColor.Controls.Add(this.labelColorValue);
            this.groupColor.Controls.Add(this.panelColorPreview);
            this.groupColor.Controls.Add(this.btnChooseColor);
            this.groupColor.Controls.Add(this.labelTargetColor);
            this.groupColor.Controls.Add(this.labelTolerance);
            this.groupColor.Controls.Add(this.trackBarTolerance);
            this.groupColor.Controls.Add(this.labelToleranceValue);
            this.groupColor.Controls.Add(this.labelInterval);
            this.groupColor.Controls.Add(this.numInterval);
            this.groupColor.Controls.Add(this.labelIntervalUnit);
            this.groupColor.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupColor.Location = new System.Drawing.Point(0, 0);
            this.groupColor.Name = "groupColor";
            this.groupColor.Padding = new System.Windows.Forms.Padding(10);
            this.groupColor.Size = new System.Drawing.Size(350, 280);
            this.groupColor.TabIndex = 0;
            this.groupColor.TabStop = false;
            this.groupColor.Text = "Color Alert";
            
            // checkEnabled
            this.checkEnabled.AutoSize = true;
            this.checkEnabled.Location = new System.Drawing.Point(13, 22);
            this.checkEnabled.Name = "checkEnabled";
            this.checkEnabled.Size = new System.Drawing.Size(150, 17);
            this.checkEnabled.TabIndex = 0;
            this.checkEnabled.Text = "Enable Color Detection";
            this.checkEnabled.UseVisualStyleBackColor = true;
            this.checkEnabled.CheckedChanged += new System.EventHandler(this.CheckEnabled_CheckedChanged);
            
            // labelTargetColor
            this.labelTargetColor.AutoSize = true;
            this.labelTargetColor.Location = new System.Drawing.Point(13, 50);
            this.labelTargetColor.Name = "labelTargetColor";
            this.labelTargetColor.Size = new System.Drawing.Size(75, 13);
            this.labelTargetColor.TabIndex = 1;
            this.labelTargetColor.Text = "Target Color:";
            
            // panelColorPreview
            this.panelColorPreview.BackColor = System.Drawing.Color.Red;
            this.panelColorPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelColorPreview.Location = new System.Drawing.Point(100, 45);
            this.panelColorPreview.Name = "panelColorPreview";
            this.panelColorPreview.Size = new System.Drawing.Size(30, 30);
            this.panelColorPreview.TabIndex = 2;
            
            // btnChooseColor
            this.btnChooseColor.Location = new System.Drawing.Point(140, 45);
            this.btnChooseColor.Name = "btnChooseColor";
            this.btnChooseColor.Size = new System.Drawing.Size(90, 30);
            this.btnChooseColor.TabIndex = 3;
            this.btnChooseColor.Text = "Choose...";
            this.btnChooseColor.UseVisualStyleBackColor = true;
            this.btnChooseColor.Click += new System.EventHandler(this.BtnChooseColor_Click);
            
            // labelColorValue
            this.labelColorValue.AutoSize = true;
            this.labelColorValue.Location = new System.Drawing.Point(240, 55);
            this.labelColorValue.Name = "labelColorValue";
            this.labelColorValue.Size = new System.Drawing.Size(60, 13);
            this.labelColorValue.TabIndex = 4;
            this.labelColorValue.Text = "#FF0000";
            
            // labelTolerance
            this.labelTolerance.AutoSize = true;
            this.labelTolerance.Location = new System.Drawing.Point(13, 90);
            this.labelTolerance.Name = "labelTolerance";
            this.labelTolerance.Size = new System.Drawing.Size(90, 13);
            this.labelTolerance.TabIndex = 5;
            this.labelTolerance.Text = "Color Tolerance:";
            
            // trackBarTolerance
            this.trackBarTolerance.Location = new System.Drawing.Point(13, 110);
            this.trackBarTolerance.Maximum = 255;
            this.trackBarTolerance.Name = "trackBarTolerance";
            this.trackBarTolerance.Size = new System.Drawing.Size(310, 45);
            this.trackBarTolerance.TabIndex = 6;
            this.trackBarTolerance.Value = 30;
            this.trackBarTolerance.ValueChanged += new System.EventHandler(this.TrackBarTolerance_ValueChanged);
            
            // labelToleranceValue
            this.labelToleranceValue.AutoSize = true;
            this.labelToleranceValue.Location = new System.Drawing.Point(320, 110);
            this.labelToleranceValue.Name = "labelToleranceValue";
            this.labelToleranceValue.Size = new System.Drawing.Size(19, 13);
            this.labelToleranceValue.TabIndex = 7;
            this.labelToleranceValue.Text = "30";
            
            // labelInterval
            this.labelInterval.AutoSize = true;
            this.labelInterval.Location = new System.Drawing.Point(13, 165);
            this.labelInterval.Name = "labelInterval";
            this.labelInterval.Size = new System.Drawing.Size(125, 13);
            this.labelInterval.TabIndex = 8;
            this.labelInterval.Text = "Sample Interval (ms):";
            
            // numInterval
            this.numInterval.Location = new System.Drawing.Point(150, 160);
            this.numInterval.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.numInterval.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numInterval.Name = "numInterval";
            this.numInterval.Size = new System.Drawing.Size(80, 20);
            this.numInterval.TabIndex = 9;
            this.numInterval.Value = new decimal(new int[] { 500, 0, 0, 0 });
            
            // labelIntervalUnit
            this.labelIntervalUnit.AutoSize = true;
            this.labelIntervalUnit.Location = new System.Drawing.Point(235, 165);
            this.labelIntervalUnit.Name = "labelIntervalUnit";
            this.labelIntervalUnit.Size = new System.Drawing.Size(98, 13);
            this.labelIntervalUnit.TabIndex = 10;
            this.labelIntervalUnit.Text = "(100-10000 range)";
            
            // btnClose
            this.btnClose.Location = new System.Drawing.Point(135, 200);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 11;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            
            // ColorAlertPanel
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupColor);
            this.Controls.Add(this.btnClose);
            this.Name = "ColorAlertPanel";
            this.Size = new System.Drawing.Size(350, 400);
            
            this.groupColor.ResumeLayout(false);
            this.groupColor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupColor;
        private System.Windows.Forms.CheckBox checkEnabled;
        private System.Windows.Forms.Label labelColorValue;
        private System.Windows.Forms.Panel panelColorPreview;
        private System.Windows.Forms.Button btnChooseColor;
        private System.Windows.Forms.Label labelTargetColor;
        private System.Windows.Forms.Label labelTolerance;
        private System.Windows.Forms.TrackBar trackBarTolerance;
        private System.Windows.Forms.Label labelToleranceValue;
        private System.Windows.Forms.Label labelInterval;
        private System.Windows.Forms.NumericUpDown numInterval;
        private System.Windows.Forms.Label labelIntervalUnit;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ToolTip tooltipInfo;
    }
}
