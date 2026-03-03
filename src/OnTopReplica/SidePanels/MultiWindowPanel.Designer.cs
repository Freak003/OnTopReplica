namespace OnTopReplica.SidePanels {
    partial class MultiWindowPanel {
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
            this.listWindows = new System.Windows.Forms.ListView();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.colPrimary = new System.Windows.Forms.ColumnHeader();
            this.btnSetPrimary = new System.Windows.Forms.Button();
            this.btnStartMonitor = new System.Windows.Forms.Button();
            this.btnStopMonitor = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelHelp = new System.Windows.Forms.Label();
            this.panelButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelHelp
            // 
            this.labelHelp.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelHelp.Location = new System.Drawing.Point(0, 0);
            this.labelHelp.Name = "labelHelp";
            this.labelHelp.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.labelHelp.Size = new System.Drawing.Size(460, 40);
            this.labelHelp.TabIndex = 0;
            this.labelHelp.Text = "勾选要监控的窗口，双击或点【设为主窗口】设置预览窗口，所有窗口共享同一监控区域。";
            // 
            // listWindows
            // 
            this.listWindows.CheckBoxes = true;
            this.listWindows.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colName,
                this.colStatus,
                this.colPrimary});
            this.listWindows.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listWindows.FullRowSelect = true;
            this.listWindows.HideSelection = false;
            this.listWindows.Location = new System.Drawing.Point(0, 40);
            this.listWindows.MultiSelect = false;
            this.listWindows.Name = "listWindows";
            this.listWindows.Size = new System.Drawing.Size(460, 340);
            this.listWindows.TabIndex = 1;
            this.listWindows.UseCompatibleStateImageBehavior = false;
            this.listWindows.View = System.Windows.Forms.View.Details;
            this.listWindows.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listWindows_ItemCheck);
            this.listWindows.DoubleClick += new System.EventHandler(this.listWindows_DoubleClick);
            // 
            // colName
            // 
            this.colName.Text = "窗口";
            this.colName.Width = 290;
            // 
            // colStatus
            // 
            this.colStatus.Text = "状态";
            this.colStatus.Width = 80;
            // 
            // colPrimary
            // 
            this.colPrimary.Text = "主窗口";
            this.colPrimary.Width = 60;
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.btnSetPrimary);
            this.panelButtons.Controls.Add(this.btnStartMonitor);
            this.panelButtons.Controls.Add(this.btnStopMonitor);
            this.panelButtons.Controls.Add(this.btnRefresh);
            this.panelButtons.Controls.Add(this.btnClose);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelButtons.Location = new System.Drawing.Point(0, 380);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new System.Windows.Forms.Padding(3);
            this.panelButtons.Size = new System.Drawing.Size(460, 60);
            this.panelButtons.TabIndex = 2;
            // 
            // btnSetPrimary
            // 
            this.btnSetPrimary.Size = new System.Drawing.Size(85, 28);
            this.btnSetPrimary.Name = "btnSetPrimary";
            this.btnSetPrimary.Text = "设为主窗口";
            this.btnSetPrimary.UseVisualStyleBackColor = true;
            this.btnSetPrimary.Click += new System.EventHandler(this.btnSetPrimary_Click);
            // 
            // btnStartMonitor
            // 
            this.btnStartMonitor.Size = new System.Drawing.Size(75, 28);
            this.btnStartMonitor.Name = "btnStartMonitor";
            this.btnStartMonitor.Text = "开始监控";
            this.btnStartMonitor.UseVisualStyleBackColor = true;
            this.btnStartMonitor.Click += new System.EventHandler(this.btnStartMonitor_Click);
            // 
            // btnStopMonitor
            // 
            this.btnStopMonitor.Size = new System.Drawing.Size(75, 28);
            this.btnStopMonitor.Name = "btnStopMonitor";
            this.btnStopMonitor.Text = "停止监控";
            this.btnStopMonitor.UseVisualStyleBackColor = true;
            this.btnStopMonitor.Click += new System.EventHandler(this.btnStopMonitor_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Size = new System.Drawing.Size(60, 28);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnClose
            // 
            this.btnClose.Size = new System.Drawing.Size(60, 28);
            this.btnClose.Name = "btnClose";
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelStatus.Location = new System.Drawing.Point(0, 440);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
            this.labelStatus.Size = new System.Drawing.Size(460, 24);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "监控中：0 个窗口";
            // 
            // MultiWindowPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listWindows);
            this.Controls.Add(this.labelHelp);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.labelStatus);
            this.Name = "MultiWindowPanel";
            this.MinimumSize = new System.Drawing.Size(460, 464);
            this.Size = new System.Drawing.Size(460, 464);
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ListView listWindows;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colPrimary;
        private System.Windows.Forms.Button btnSetPrimary;
        private System.Windows.Forms.Button btnStartMonitor;
        private System.Windows.Forms.Button btnStopMonitor;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelHelp;
        private System.Windows.Forms.FlowLayoutPanel panelButtons;
    }
}
