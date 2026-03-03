using System;
using System.Drawing;
using System.Windows.Forms;

namespace OnTopReplica.SidePanels {

    /// <summary>
    /// Dialog that displays a snapshot of the ThumbnailPanel and lets the user
    /// draw a rectangle to select the icon/graphic template region.
    /// </summary>
    class IconCaptureDialog : Form {

        private Bitmap _snapshot;
        private PictureBox _pictureBox;
        private Label _instructions;
        private Button _btnOK;
        private Button _btnCancel;

        private Point _startPoint;
        private Rectangle _selection;
        private bool _dragging = false;

        /// <summary>
        /// The captured region bitmap. Null if cancelled.
        /// </summary>
        public Bitmap CapturedRegion { get; private set; }

        public IconCaptureDialog(Bitmap snapshot) {
            _snapshot = snapshot;

            this.Text = "截取图形模板";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(400, 350);

            // Scale form to fit snapshot reasonably
            int formW = Math.Min(Math.Max(snapshot.Width + 40, 400), 900);
            int formH = Math.Min(Math.Max(snapshot.Height + 120, 350), 700);
            this.ClientSize = new Size(formW, formH);

            _instructions = new Label {
                Text = "在下方图片上拖动鼠标框选要检测的图形区域：",
                Dock = DockStyle.Top,
                Padding = new Padding(8, 6, 8, 6),
                Height = 30
            };

            _pictureBox = new PictureBox {
                Image = _snapshot,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                Cursor = Cursors.Cross,
                BorderStyle = BorderStyle.FixedSingle
            };
            _pictureBox.MouseDown += PictureBox_MouseDown;
            _pictureBox.MouseMove += PictureBox_MouseMove;
            _pictureBox.MouseUp += PictureBox_MouseUp;
            _pictureBox.Paint += PictureBox_Paint;

            var buttonPanel = new FlowLayoutPanel {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(6, 4, 6, 4)
            };

            _btnCancel = new Button { Text = "取消", Size = new Size(75, 28), DialogResult = DialogResult.Cancel };
            _btnOK = new Button { Text = "确定", Size = new Size(75, 28), Enabled = false };
            _btnOK.Click += BtnOK_Click;
            _btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            buttonPanel.Controls.Add(_btnCancel);
            buttonPanel.Controls.Add(_btnOK);

            this.Controls.Add(_pictureBox);
            this.Controls.Add(_instructions);
            this.Controls.Add(buttonPanel);

            this.AcceptButton = _btnOK;
            this.CancelButton = _btnCancel;
        }

        /// <summary>
        /// Converts display coordinates to image coordinates based on Zoom mode.
        /// </summary>
        private Point DisplayToImage(Point displayPoint) {
            if (_snapshot == null || _pictureBox.Width <= 0 || _pictureBox.Height <= 0)
                return displayPoint;

            float imgW = _snapshot.Width;
            float imgH = _snapshot.Height;
            float boxW = _pictureBox.ClientSize.Width;
            float boxH = _pictureBox.ClientSize.Height;

            float scale = Math.Min(boxW / imgW, boxH / imgH);
            float scaledW = imgW * scale;
            float scaledH = imgH * scale;
            float offsetX = (boxW - scaledW) / 2f;
            float offsetY = (boxH - scaledH) / 2f;

            int ix = (int)((displayPoint.X - offsetX) / scale);
            int iy = (int)((displayPoint.Y - offsetY) / scale);

            ix = Math.Max(0, Math.Min(ix, _snapshot.Width - 1));
            iy = Math.Max(0, Math.Min(iy, _snapshot.Height - 1));

            return new Point(ix, iy);
        }

        /// <summary>
        /// Converts image coordinates back to display coordinates for painting the selection rectangle.
        /// </summary>
        private Rectangle ImageToDisplay(Rectangle imgRect) {
            if (_snapshot == null || _pictureBox.Width <= 0 || _pictureBox.Height <= 0)
                return imgRect;

            float imgW = _snapshot.Width;
            float imgH = _snapshot.Height;
            float boxW = _pictureBox.ClientSize.Width;
            float boxH = _pictureBox.ClientSize.Height;

            float scale = Math.Min(boxW / imgW, boxH / imgH);
            float offsetX = (boxW - imgW * scale) / 2f;
            float offsetY = (boxH - imgH * scale) / 2f;

            return new Rectangle(
                (int)(imgRect.X * scale + offsetX),
                (int)(imgRect.Y * scale + offsetY),
                (int)(imgRect.Width * scale),
                (int)(imgRect.Height * scale)
            );
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                _startPoint = DisplayToImage(e.Location);
                _selection = Rectangle.Empty;
                _dragging = true;
                _pictureBox.Invalidate();
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e) {
            if (!_dragging) return;

            var current = DisplayToImage(e.Location);
            int x = Math.Min(_startPoint.X, current.X);
            int y = Math.Min(_startPoint.Y, current.Y);
            int w = Math.Abs(current.X - _startPoint.X);
            int h = Math.Abs(current.Y - _startPoint.Y);

            _selection = new Rectangle(x, y, w, h);
            _pictureBox.Invalidate();
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e) {
            if (!_dragging) return;
            _dragging = false;

            _btnOK.Enabled = _selection.Width > 2 && _selection.Height > 2;
            _pictureBox.Invalidate();
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e) {
            if (_selection.Width > 0 && _selection.Height > 0) {
                var displayRect = ImageToDisplay(_selection);
                using (var pen = new Pen(Color.Lime, 2f)) {
                    e.Graphics.DrawRectangle(pen, displayRect);
                }
                using (var brush = new SolidBrush(Color.FromArgb(40, 0, 255, 0))) {
                    e.Graphics.FillRectangle(brush, displayRect);
                }
            }
        }

        private void BtnOK_Click(object sender, EventArgs e) {
            if (_selection.Width <= 2 || _selection.Height <= 2) return;

            // Clamp to image bounds
            var clampedRect = Rectangle.Intersect(_selection,
                new Rectangle(0, 0, _snapshot.Width, _snapshot.Height));

            if (clampedRect.Width <= 0 || clampedRect.Height <= 0) return;

            try {
                CapturedRegion = _snapshot.Clone(clampedRect, _snapshot.PixelFormat);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex) {
                MessageBox.Show("截取失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                // Don't dispose _snapshot - caller owns it
                // CapturedRegion ownership transfers to caller
            }
            base.Dispose(disposing);
        }
    }
}
