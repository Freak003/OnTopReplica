namespace OnTopReplica.SidePanels {
    partial class MultiWindowPanel {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent() {
            this.listWindows = new System.Windows.Forms.ListView();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.colPrimary = new System.Windows.Forms.ColumnHeader();
            this.btnSetPrimary = new System.Windows.Forms.Button();
            this.btnToggleMonitor = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelHelp = new System.Windows.Forms.Label();
            this.panelButtons = new System.Windows.Forms.FlowLayoutPanel();
            // Color detection controls
            this.grpColor = new System.Windows.Forms.GroupBox();
            this.chkColorEnabled = new System.Windows.Forms.CheckBox();
            this.chkRed = new System.Windows.Forms.CheckBox();
            this.chkOrange = new System.Windows.Forms.CheckBox();
            this.chkGray = new System.Windows.Forms.CheckBox();
            // Icon detection controls
            this.grpIcon = new System.Windows.Forms.GroupBox();
            this.chkIconEnabled = new System.Windows.Forms.CheckBox();
            this.btnCaptureIcon = new System.Windows.Forms.Button();
            this.btnLoadIcon = new System.Windows.Forms.Button();
            this.btnClearIcon = new System.Windows.Forms.Button();
            this.picIconPreview = new System.Windows.Forms.PictureBox();
            this.lblIconStatus = new System.Windows.Forms.Label();
            this.lblAlarmSound = new System.Windows.Forms.Label();
            this.cmbAlarmSound = new System.Windows.Forms.ComboBox();
            // Bottom panel
            this.panelBottom = new System.Windows.Forms.Panel();

            this.panelButtons.SuspendLayout();
            this.grpColor.SuspendLayout();
            this.grpIcon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIconPreview)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();

            // =============================================
            // labelHelp (Top)
            // =============================================
            this.labelHelp.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelHelp.Location = new System.Drawing.Point(0, 0);
            this.labelHelp.Name = "labelHelp";
            this.labelHelp.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.labelHelp.Size = new System.Drawing.Size(500, 36);
            this.labelHelp.TabIndex = 0;
            this.labelHelp.Text = "勾选要监控的窗口，双击或点【设为主窗口】设置预览窗口，所有窗口共享同一监控区域。";

            // =============================================
            // listWindows (Fill)
            // =============================================
            this.listWindows.CheckBoxes = true;
            this.listWindows.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colName, this.colStatus, this.colPrimary});
            this.listWindows.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listWindows.FullRowSelect = true;
            this.listWindows.HideSelection = false;
            this.listWindows.Location = new System.Drawing.Point(0, 36);
            this.listWindows.MultiSelect = false;
            this.listWindows.Name = "listWindows";
            this.listWindows.Size = new System.Drawing.Size(500, 230);
            this.listWindows.TabIndex = 1;
            this.listWindows.UseCompatibleStateImageBehavior = false;
            this.listWindows.View = System.Windows.Forms.View.Details;
            this.listWindows.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listWindows_ItemCheck);
            this.listWindows.DoubleClick += new System.EventHandler(this.listWindows_DoubleClick);
            //
            this.colName.Text = "窗口";
            this.colName.Width = 310;
            this.colStatus.Text = "状态";
            this.colStatus.Width = 80;
            this.colPrimary.Text = "主窗口";
            this.colPrimary.Width = 60;

            // =============================================
            // panelBottom (Bottom - holds settings + buttons + status)
            // =============================================
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 266);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(500, 361);
            this.panelBottom.TabIndex = 10;
            this.panelBottom.Controls.Add(this.grpColor);
            this.panelBottom.Controls.Add(this.grpIcon);
            this.panelBottom.Controls.Add(this.panelButtons);
            this.panelBottom.Controls.Add(this.labelStatus);

            // =============================================
            // grpColor - Color Detection Group
            // =============================================
            this.grpColor.Location = new System.Drawing.Point(6, 4);
            this.grpColor.Name = "grpColor";
            this.grpColor.Size = new System.Drawing.Size(488, 70);
            this.grpColor.TabIndex = 0;
            this.grpColor.TabStop = false;
            this.grpColor.Text = "颜色检测";
            this.grpColor.Controls.Add(this.chkColorEnabled);
            this.grpColor.Controls.Add(this.chkRed);
            this.grpColor.Controls.Add(this.chkOrange);
            this.grpColor.Controls.Add(this.chkGray);
            //
            this.chkColorEnabled.Location = new System.Drawing.Point(12, 20);
            this.chkColorEnabled.Name = "chkColorEnabled";
            this.chkColorEnabled.Size = new System.Drawing.Size(90, 20);
            this.chkColorEnabled.Text = "启用";
            this.chkColorEnabled.TabIndex = 0;
            this.chkColorEnabled.CheckedChanged += new System.EventHandler(this.chkColorEnabled_CheckedChanged);
            //
            this.chkRed.Location = new System.Drawing.Point(12, 44);
            this.chkRed.Name = "chkRed";
            this.chkRed.Size = new System.Drawing.Size(70, 20);
            this.chkRed.Text = "红色";
            this.chkRed.ForeColor = System.Drawing.Color.Red;
            this.chkRed.TabIndex = 1;
            this.chkRed.Enabled = false;
            this.chkRed.CheckedChanged += new System.EventHandler(this.chkRed_CheckedChanged);
            //
            this.chkOrange.Location = new System.Drawing.Point(90, 44);
            this.chkOrange.Name = "chkOrange";
            this.chkOrange.Size = new System.Drawing.Size(70, 20);
            this.chkOrange.Text = "橙色";
            this.chkOrange.ForeColor = System.Drawing.Color.OrangeRed;
            this.chkOrange.TabIndex = 2;
            this.chkOrange.Enabled = false;
            this.chkOrange.CheckedChanged += new System.EventHandler(this.chkOrange_CheckedChanged);
            //
            this.chkGray.Location = new System.Drawing.Point(168, 44);
            this.chkGray.Name = "chkGray";
            this.chkGray.Size = new System.Drawing.Size(70, 20);
            this.chkGray.Text = "灰色";
            this.chkGray.ForeColor = System.Drawing.Color.Gray;
            this.chkGray.TabIndex = 3;
            this.chkGray.Enabled = false;
            this.chkGray.CheckedChanged += new System.EventHandler(this.chkGray_CheckedChanged);

            // =============================================
            // grpIcon - Icon/Graphic Detection Group
            // =============================================
            this.grpIcon.Location = new System.Drawing.Point(6, 78);
            this.grpIcon.Name = "grpIcon";
            this.grpIcon.Size = new System.Drawing.Size(488, 217);
            this.grpIcon.TabIndex = 1;
            this.grpIcon.TabStop = false;
            this.grpIcon.Text = "图形检测（图形从所有窗口消失时报警）";
            this.grpIcon.Controls.Add(this.chkIconEnabled);
            this.grpIcon.Controls.Add(this.btnCaptureIcon);
            this.grpIcon.Controls.Add(this.btnLoadIcon);
            this.grpIcon.Controls.Add(this.btnClearIcon);
            this.grpIcon.Controls.Add(this.picIconPreview);
            this.grpIcon.Controls.Add(this.lblIconStatus);
            this.grpIcon.Controls.Add(this.lblAlarmSound);
            this.grpIcon.Controls.Add(this.cmbAlarmSound);
            //
            this.chkIconEnabled.Location = new System.Drawing.Point(12, 22);
            this.chkIconEnabled.Name = "chkIconEnabled";
            this.chkIconEnabled.Size = new System.Drawing.Size(90, 20);
            this.chkIconEnabled.Text = "启用";
            this.chkIconEnabled.TabIndex = 0;
            this.chkIconEnabled.CheckedChanged += new System.EventHandler(this.chkIconEnabled_CheckedChanged);
            //
            this.btnCaptureIcon.Location = new System.Drawing.Point(12, 48);
            this.btnCaptureIcon.Name = "btnCaptureIcon";
            this.btnCaptureIcon.Size = new System.Drawing.Size(100, 28);
            this.btnCaptureIcon.Text = "从预览截取";
            this.btnCaptureIcon.UseVisualStyleBackColor = true;
            this.btnCaptureIcon.Enabled = false;
            this.btnCaptureIcon.TabIndex = 1;
            this.btnCaptureIcon.Click += new System.EventHandler(this.btnCaptureIcon_Click);
            //
            this.btnLoadIcon.Location = new System.Drawing.Point(118, 48);
            this.btnLoadIcon.Name = "btnLoadIcon";
            this.btnLoadIcon.Size = new System.Drawing.Size(100, 28);
            this.btnLoadIcon.Text = "从文件加载";
            this.btnLoadIcon.UseVisualStyleBackColor = true;
            this.btnLoadIcon.Enabled = false;
            this.btnLoadIcon.TabIndex = 2;
            this.btnLoadIcon.Click += new System.EventHandler(this.btnLoadIcon_Click);
            //
            this.btnClearIcon.Location = new System.Drawing.Point(224, 48);
            this.btnClearIcon.Name = "btnClearIcon";
            this.btnClearIcon.Size = new System.Drawing.Size(80, 28);
            this.btnClearIcon.Text = "清除模板";
            this.btnClearIcon.UseVisualStyleBackColor = true;
            this.btnClearIcon.Enabled = false;
            this.btnClearIcon.TabIndex = 3;
            this.btnClearIcon.Click += new System.EventHandler(this.btnClearIcon_Click);
            //
            this.picIconPreview.Location = new System.Drawing.Point(12, 82);
            this.picIconPreview.Name = "picIconPreview";
            this.picIconPreview.Size = new System.Drawing.Size(100, 100);
            this.picIconPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picIconPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picIconPreview.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.picIconPreview.TabIndex = 4;
            this.picIconPreview.TabStop = false;
            //
            this.lblIconStatus.Location = new System.Drawing.Point(118, 88);
            this.lblIconStatus.Name = "lblIconStatus";
            this.lblIconStatus.Size = new System.Drawing.Size(360, 55);
            this.lblIconStatus.TabIndex = 5;
            this.lblIconStatus.Text = "未设置模板\n\n当参考图形从所有监控窗口的监控区域中消失时，将触发提示音。";
            //
            this.lblAlarmSound.Location = new System.Drawing.Point(12, 188);
            this.lblAlarmSound.Name = "lblAlarmSound";
            this.lblAlarmSound.Size = new System.Drawing.Size(65, 20);
            this.lblAlarmSound.TabIndex = 6;
            this.lblAlarmSound.Text = "报警声音:";
            this.lblAlarmSound.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            this.cmbAlarmSound.Location = new System.Drawing.Point(80, 186);
            this.cmbAlarmSound.Name = "cmbAlarmSound";
            this.cmbAlarmSound.Size = new System.Drawing.Size(400, 23);
            this.cmbAlarmSound.TabIndex = 7;
            this.cmbAlarmSound.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAlarmSound.SelectedIndexChanged += new System.EventHandler(this.cmbAlarmSound_SelectedIndexChanged);

            // =============================================
            // panelButtons
            // =============================================
            this.panelButtons.Controls.Add(this.btnSetPrimary);
            this.panelButtons.Controls.Add(this.btnToggleMonitor);
            this.panelButtons.Controls.Add(this.btnRefresh);
            this.panelButtons.Controls.Add(this.btnClose);
            this.panelButtons.Location = new System.Drawing.Point(0, 299);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelButtons.Padding = new System.Windows.Forms.Padding(3);
            this.panelButtons.Size = new System.Drawing.Size(500, 36);
            this.panelButtons.TabIndex = 2;
            //
            this.btnSetPrimary.Size = new System.Drawing.Size(85, 28);
            this.btnSetPrimary.Name = "btnSetPrimary";
            this.btnSetPrimary.Text = "设为主窗口";
            this.btnSetPrimary.UseVisualStyleBackColor = true;
            this.btnSetPrimary.Click += new System.EventHandler(this.btnSetPrimary_Click);
            //
            this.btnToggleMonitor.Size = new System.Drawing.Size(75, 28);
            this.btnToggleMonitor.Name = "btnToggleMonitor";
            this.btnToggleMonitor.Text = "开始监控";
            this.btnToggleMonitor.UseVisualStyleBackColor = true;
            this.btnToggleMonitor.Click += new System.EventHandler(this.btnToggleMonitor_Click);
            //
            this.btnRefresh.Size = new System.Drawing.Size(60, 28);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            //
            this.btnClose.Size = new System.Drawing.Size(60, 28);
            this.btnClose.Name = "btnClose";
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // =============================================
            // labelStatus
            // =============================================
            this.labelStatus.Location = new System.Drawing.Point(0, 337);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
            this.labelStatus.Size = new System.Drawing.Size(500, 24);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "监控中：0 个窗口";

            // =============================================
            // MultiWindowPanel
            // =============================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listWindows);
            this.Controls.Add(this.labelHelp);
            this.Controls.Add(this.panelBottom);
            this.Name = "MultiWindowPanel";
            this.MinimumSize = new System.Drawing.Size(500, 627);
            this.Size = new System.Drawing.Size(500, 627);

            this.panelButtons.ResumeLayout(false);
            this.grpColor.ResumeLayout(false);
            this.grpIcon.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picIconPreview)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ListView listWindows;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colPrimary;
        private System.Windows.Forms.Button btnSetPrimary;
        private System.Windows.Forms.Button btnToggleMonitor;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelHelp;
        private System.Windows.Forms.FlowLayoutPanel panelButtons;
        private System.Windows.Forms.Panel panelBottom;
        // Color detection
        private System.Windows.Forms.GroupBox grpColor;
        private System.Windows.Forms.CheckBox chkColorEnabled;
        private System.Windows.Forms.CheckBox chkRed;
        private System.Windows.Forms.CheckBox chkOrange;
        private System.Windows.Forms.CheckBox chkGray;
        // Icon detection
        private System.Windows.Forms.GroupBox grpIcon;
        private System.Windows.Forms.CheckBox chkIconEnabled;
        private System.Windows.Forms.Button btnCaptureIcon;
        private System.Windows.Forms.Button btnLoadIcon;
        private System.Windows.Forms.Button btnClearIcon;
        private System.Windows.Forms.PictureBox picIconPreview;
        private System.Windows.Forms.Label lblIconStatus;
        private System.Windows.Forms.Label lblAlarmSound;
        private System.Windows.Forms.ComboBox cmbAlarmSound;
    }
}
