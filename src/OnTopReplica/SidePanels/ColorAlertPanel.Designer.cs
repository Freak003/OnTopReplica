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
            // increased height to fit volume and sound controls and color list
            this.groupColor.Size = new System.Drawing.Size(350, 410);
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
            this.labelTolerance.Location = new System.Drawing.Point(13, 120);
            this.labelTolerance.Name = "labelTolerance";
            this.labelTolerance.Size = new System.Drawing.Size(90, 13);
            this.labelTolerance.TabIndex = 5;
            this.labelTolerance.Text = "Color Tolerance:";
            
            // trackBarTolerance
            this.trackBarTolerance.Location = new System.Drawing.Point(13, 140);
            this.trackBarTolerance.Maximum = 255;
            this.trackBarTolerance.Name = "trackBarTolerance";
            this.trackBarTolerance.Size = new System.Drawing.Size(310, 45);
            this.trackBarTolerance.TabIndex = 6;
            this.trackBarTolerance.Value = 30;
            this.trackBarTolerance.ValueChanged += new System.EventHandler(this.TrackBarTolerance_ValueChanged);
            
            // labelToleranceValue
            this.labelToleranceValue.AutoSize = true;
            this.labelToleranceValue.Location = new System.Drawing.Point(320, 140);
            this.labelToleranceValue.Name = "labelToleranceValue";
            this.labelToleranceValue.Size = new System.Drawing.Size(19, 13);
            this.labelToleranceValue.TabIndex = 7;
            this.labelToleranceValue.Text = "30";
            
            // labelInterval
            this.labelInterval.AutoSize = true;
            this.labelInterval.Location = new System.Drawing.Point(13, 180);
            this.labelInterval.Name = "labelInterval";
            this.labelInterval.Size = new System.Drawing.Size(125, 13);
            this.labelInterval.TabIndex = 8;
            this.labelInterval.Text = "Sample Interval (ms):";
            
            // numInterval
            this.numInterval.Location = new System.Drawing.Point(150, 175);
            this.numInterval.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.numInterval.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numInterval.Name = "numInterval";
            this.numInterval.Size = new System.Drawing.Size(80, 20);
            this.numInterval.TabIndex = 9;
            this.numInterval.Value = new decimal(new int[] { 500, 0, 0, 0 });
            
            // labelIntervalUnit
            this.labelIntervalUnit.AutoSize = true;
            this.labelIntervalUnit.Location = new System.Drawing.Point(235, 180);
            this.labelIntervalUnit.Name = "labelIntervalUnit";
            this.labelIntervalUnit.Size = new System.Drawing.Size(98, 13);
            this.labelIntervalUnit.TabIndex = 10;
            this.labelIntervalUnit.Text = "(100-10000 range)";
            
            // labelVolume
            this.labelVolume = new System.Windows.Forms.Label();
            this.trackBarVolume = new System.Windows.Forms.TrackBar();
            
            // labelVolume
            this.labelVolume.AutoSize = true;
            this.labelVolume.Location = new System.Drawing.Point(13, 220);
            this.labelVolume.Name = "labelVolume";
            this.labelVolume.Size = new System.Drawing.Size(78, 13);
            this.labelVolume.TabIndex = 12;
            this.labelVolume.Text = "Alarm Volume:";
            
            // trackBarVolume
            this.trackBarVolume.Location = new System.Drawing.Point(100, 215);
            this.trackBarVolume.Maximum = 100;
            this.trackBarVolume.Name = "trackBarVolume";
            this.trackBarVolume.Size = new System.Drawing.Size(223, 45);
            this.trackBarVolume.TabIndex = 13;
            this.trackBarVolume.Value = 100;
            this.trackBarVolume.Scroll += new System.EventHandler(this.TrackBarVolume_Scroll);
            
            // labelSound
            this.labelSoundFile = new System.Windows.Forms.Label();
            this.comboSound = new System.Windows.Forms.ComboBox();
            
            // labelSound
            this.labelSoundFile.AutoSize = true;
            this.labelSoundFile.Location = new System.Drawing.Point(13, 260);
            this.labelSoundFile.Name = "labelSoundFile";
            this.labelSoundFile.Size = new System.Drawing.Size(62, 13);
            this.labelSoundFile.TabIndex = 14;
            this.labelSoundFile.Text = "Sound file:";
            
            // comboSound
            this.comboSound.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSound.FormattingEnabled = true;
            this.comboSound.Location = new System.Drawing.Point(100, 256);
            this.comboSound.Name = "comboSound";
            this.comboSound.Size = new System.Drawing.Size(223, 21);
            this.comboSound.TabIndex = 15;
            this.comboSound.SelectedIndexChanged += new System.EventHandler(this.ComboSound_SelectedIndexChanged);
            
            // add volume/sound controls to group after they are created
            this.groupColor.Controls.Add(this.labelVolume);
            this.groupColor.Controls.Add(this.trackBarVolume);
            this.groupColor.Controls.Add(this.labelSoundFile);
            this.groupColor.Controls.Add(this.comboSound);

            // create color list and add/remove buttons
            this.lstColors = new System.Windows.Forms.ListBox();
            this.lstColors.FormattingEnabled = true;
            this.lstColors.Location = new System.Drawing.Point(100, 270);
            this.lstColors.Name = "lstColors";
            this.lstColors.Size = new System.Drawing.Size(160, 82);
            this.lstColors.TabIndex = 16;
            this.lstColors.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstColors.ItemHeight = 20;
            this.lstColors.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.LstColors_DrawItem);

            this.btnAddColor = new System.Windows.Forms.Button();
            this.btnAddColor.Location = new System.Drawing.Point(270, 270);
            this.btnAddColor.Name = "btnAddColor";
            this.btnAddColor.Size = new System.Drawing.Size(60, 25);
            this.btnAddColor.TabIndex = 17;
            this.btnAddColor.Text = "添加";
            this.btnAddColor.UseVisualStyleBackColor = true;
            this.btnAddColor.Click += new System.EventHandler(this.BtnAddColor_Click);

            this.btnRemoveColor = new System.Windows.Forms.Button();
            this.btnRemoveColor.Location = new System.Drawing.Point(270, 305);
            this.btnRemoveColor.Name = "btnRemoveColor";
            this.btnRemoveColor.Size = new System.Drawing.Size(60, 25);
            this.btnRemoveColor.TabIndex = 18;
            this.btnRemoveColor.Text = "移除";
            this.btnRemoveColor.UseVisualStyleBackColor = true;
            this.btnRemoveColor.Click += new System.EventHandler(this.BtnRemoveColor_Click);

            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnMoveUp.Location = new System.Drawing.Point(270, 340);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(60, 25);
            this.btnMoveUp.TabIndex = 19;
            this.btnMoveUp.Text = "上移";
            this.btnMoveUp.UseVisualStyleBackColor = true;
            this.btnMoveUp.Click += new System.EventHandler(this.BtnMoveUp_Click);

            this.btnMoveDown = new System.Windows.Forms.Button();
            this.btnMoveDown.Location = new System.Drawing.Point(270, 375);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(60, 25);
            this.btnMoveDown.TabIndex = 20;
            this.btnMoveDown.Text = "下移";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.BtnMoveDown_Click);

            this.btnTestAlarm = new System.Windows.Forms.Button();
            this.btnTestAlarm.Location = new System.Drawing.Point(100, 410);
            this.btnTestAlarm.Name = "btnTestAlarm";
            this.btnTestAlarm.Size = new System.Drawing.Size(100, 25);
            this.btnTestAlarm.TabIndex = 21;
            this.btnTestAlarm.Text = "测试报警";
            this.btnTestAlarm.UseVisualStyleBackColor = true;
            this.btnTestAlarm.Click += new System.EventHandler(this.BtnTestAlarm_Click);
            
            this.groupColor.Controls.Add(this.btnTestAlarm);

            // add color list and buttons into group
            this.groupColor.Controls.Add(this.lstColors);
            this.groupColor.Controls.Add(this.btnAddColor);
            this.groupColor.Controls.Add(this.btnRemoveColor);
            this.groupColor.Controls.Add(this.btnMoveUp);
            this.groupColor.Controls.Add(this.btnMoveDown);

            // reposition btnClose below new groupHeight and controls
            this.btnClose.Location = new System.Drawing.Point(135, 430);
            this.btnClose.TabIndex = 16;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            
            // ColorAlertPanel
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupColor);
            this.Controls.Add(this.btnClose);
            this.Name = "ColorAlertPanel";
            this.Size = new System.Drawing.Size(350, 480);
            this.MinimumSize = new System.Drawing.Size(350, 480);
            
            // ColorAlertPanel
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupColor);
            this.Controls.Add(this.btnClose);
            this.Name = "ColorAlertPanel";
            this.Size = new System.Drawing.Size(350, 480);
            this.MinimumSize = new System.Drawing.Size(350, 480);
            
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
        private System.Windows.Forms.Label labelVolume;
        private System.Windows.Forms.TrackBar trackBarVolume;
        private System.Windows.Forms.Label labelSoundFile;
        private System.Windows.Forms.ComboBox comboSound;
        private System.Windows.Forms.ListBox lstColors;
        private System.Windows.Forms.Button btnAddColor;
        private System.Windows.Forms.Button btnRemoveColor;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnTestAlarm;
    }
}
