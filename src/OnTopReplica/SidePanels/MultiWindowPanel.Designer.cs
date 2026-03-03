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
            this.labelHelp.Size = new System.Drawing.Size(320, 45);
            this.labelHelp.TabIndex = 0;
            this.labelHelp.Text = "Check windows to monitor. Double-click or use \"Set Primary\" to set the preview window. Region is shared across all windows.";
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
            this.listWindows.Location = new System.Drawing.Point(0, 45);
            this.listWindows.MultiSelect = false;
            this.listWindows.Name = "listWindows";
            this.listWindows.Size = new System.Drawing.Size(320, 255);
            this.listWindows.TabIndex = 1;
            this.listWindows.UseCompatibleStateImageBehavior = false;
            this.listWindows.View = System.Windows.Forms.View.Details;
            this.listWindows.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listWindows_ItemCheck);
            this.listWindows.DoubleClick += new System.EventHandler(this.listWindows_DoubleClick);
            // 
            // colName
            // 
            this.colName.Text = "Window";
            this.colName.Width = 200;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 60;
            // 
            // colPrimary
            // 
            this.colPrimary.Text = "Primary";
            this.colPrimary.Width = 50;
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
            this.panelButtons.Location = new System.Drawing.Point(0, 300);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new System.Windows.Forms.Padding(3);
            this.panelButtons.Size = new System.Drawing.Size(320, 66);
            this.panelButtons.TabIndex = 2;
            // 
            // btnSetPrimary
            // 
            this.btnSetPrimary.Size = new System.Drawing.Size(90, 25);
            this.btnSetPrimary.Name = "btnSetPrimary";
            this.btnSetPrimary.Text = "Set Primary";
            this.btnSetPrimary.UseVisualStyleBackColor = true;
            this.btnSetPrimary.Click += new System.EventHandler(this.btnSetPrimary_Click);
            // 
            // btnStartMonitor
            // 
            this.btnStartMonitor.Size = new System.Drawing.Size(90, 25);
            this.btnStartMonitor.Name = "btnStartMonitor";
            this.btnStartMonitor.Text = "Start";
            this.btnStartMonitor.UseVisualStyleBackColor = true;
            this.btnStartMonitor.Click += new System.EventHandler(this.btnStartMonitor_Click);
            // 
            // btnStopMonitor
            // 
            this.btnStopMonitor.Size = new System.Drawing.Size(90, 25);
            this.btnStopMonitor.Name = "btnStopMonitor";
            this.btnStopMonitor.Text = "Stop";
            this.btnStopMonitor.UseVisualStyleBackColor = true;
            this.btnStopMonitor.Click += new System.EventHandler(this.btnStopMonitor_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Size = new System.Drawing.Size(90, 25);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnClose
            // 
            this.btnClose.Size = new System.Drawing.Size(90, 25);
            this.btnClose.Name = "btnClose";
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelStatus.Location = new System.Drawing.Point(0, 366);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
            this.labelStatus.Size = new System.Drawing.Size(320, 24);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "Monitored: 0 window(s)";
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
            this.Size = new System.Drawing.Size(320, 390);
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
