using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace TS_SE_Tool
{
    public partial class MaterialForm
    {
        private bool chromeReady, resizeWindow;
        private Label windowTitle;
        [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

        internal void EnsureChrome()
        {
            if (chromeReady || SystemInformation.HighContrast) return;
            chromeReady = true;
            resizeWindow = FormBorderStyle == FormBorderStyle.Sizable && (MaximumSize.IsEmpty || MaximumSize != MinimumSize);
            bool showControls = ControlBox;
            Size contentSize = ClientSize;
            Size oldMinimum = MinimumSize, oldMaximum = MaximumSize;
            MinimumSize = MaximumSize = Size.Empty;
            if (AutoScaleMode != AutoScaleMode.Dpi)
            {
                AutoScaleMode = AutoScaleMode.Dpi;
                AutoScaleDimensions = new SizeF(96, 96);
            }
            FormBorderStyle = FormBorderStyle.None;
            var header = new Panel { Name = "appTitleBar", Dock = DockStyle.Top, Height = 42, Padding = new Padding(16, 5, 6, 5) };
            windowTitle = new Label { Text = Text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = AppTypography.Create(11, FontStyle.Bold), AutoEllipsis = true };
            header.Controls.Add(windowTitle);
            if (showControls)
            {
                if (MinimizeBox) header.Controls.Add(ChromeButton("minimize", "Minimize window", () => WindowState = FormWindowState.Minimized));
                if (MaximizeBox && resizeWindow)
                    header.Controls.Add(ChromeButton("maximize", "Maximize or restore window", () => ToggleMaximize()));
                header.Controls.Add(ChromeButton("close", "Close window", () => Close()));
            }
            MouseEventHandler drag = (sender, e) =>
            {
                if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, new IntPtr(2), IntPtr.Zero); }
            };
            header.MouseDown += drag;
            windowTitle.MouseDown += drag;
            windowTitle.DoubleClick += (sender, e) => { if (resizeWindow && MaximizeBox) ToggleMaximize(); };
            var content = new Panel { Name = "appContent", Dock = DockStyle.Fill, Padding = Padding, Size = contentSize };
            var originalControls = new Control[Controls.Count];
            Controls.CopyTo(originalControls, 0);
            Controls.Clear();
            content.Controls.AddRange(originalControls);
            Padding = Padding.Empty;
            Controls.Add(content);
            Controls.Add(header);
            ClientSize = new Size(contentSize.Width, contentSize.Height + header.Height);
            MinimumSize = oldMinimum.IsEmpty ? new Size(240, 160) : new Size(oldMinimum.Width, oldMinimum.Height + header.Height);
            MaximumSize = oldMaximum.IsEmpty ? Size.Empty : new Size(oldMaximum.Width, oldMaximum.Height + header.Height);
            if (resizeWindow)
            {
                var available = Screen.FromControl(this).WorkingArea.Size;
                MinimumSize = new Size(Math.Min(MinimumSize.Width, available.Width - 16), Math.Min(MinimumSize.Height, available.Height - 16));
                Size = new Size(Math.Min(Width, available.Width - 16), Math.Min(Height, available.Height - 16));
            }
            UpdateWindowShape();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (!chromeReady || WindowState != FormWindowState.Normal) return;
            var available = Screen.FromControl(this).WorkingArea.Size;
            var limit = new Size(Math.Max(1, available.Width - 16), Math.Max(1, available.Height - 16));
            if (MinimumSize.Width > limit.Width || MinimumSize.Height > limit.Height)
                MinimumSize = new Size(Math.Min(MinimumSize.Width, limit.Width), Math.Min(MinimumSize.Height, limit.Height));
            if (Width > limit.Width || Height > limit.Height)
                Size = new Size(Math.Min(Width, limit.Width), Math.Min(Height, limit.Height));
        }

        private Button ChromeButton(string kind, string accessibleName, Action action)
        {
            var button = new WindowButton(kind) { AccessibleName = accessibleName, Dock = DockStyle.Right, Width = 34, TabStop = false };
            button.Click += (sender, e) => action();
            return button;
        }

        private sealed class WindowButton : Button
        {
            private readonly string kind;
            private bool hovered;
            internal WindowButton(string kind)
            {
                this.kind = kind;
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                UseVisualStyleBackColor = false;
                SetStyle(ControlStyles.Opaque, false);
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            }
            protected override void OnMouseEnter(EventArgs e) { hovered = true; base.OnMouseEnter(e); Invalidate(); }
            protected override void OnMouseLeave(EventArgs e) { hovered = false; base.OnMouseLeave(e); Invalidate(); }
            protected override void OnPaintBackground(PaintEventArgs e) { MaterialYou.PaintActionBackground(this, e.Graphics); }
            protected override void OnPaint(PaintEventArgs e)
            {
                MaterialYou.PaintActionBackground(this, e.Graphics);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                if (hovered)
                    using (var path = MaterialYou.Rounded(new Rectangle(1, 1, Width - 2, Height - 2), 7))
                    using (var fill = new SolidBrush(kind == "close" ? Color.FromArgb(252, 225, 225) : MaterialYou.Container)) e.Graphics.FillPath(fill, path);
                float x = Width / 2f, y = Height / 2f;
                using (var pen = new Pen(MaterialYou.OnSurface, 1.4f))
                {
                    if (kind == "minimize") e.Graphics.DrawLine(pen, x - 5, y + 2, x + 5, y + 2);
                    else if (kind == "close") { e.Graphics.DrawLine(pen, x - 4, y - 4, x + 4, y + 4); e.Graphics.DrawLine(pen, x - 4, y + 4, x + 4, y - 4); }
                    else if (FindForm()?.WindowState == FormWindowState.Maximized)
                    { e.Graphics.DrawRectangle(pen, x - 2, y - 5, 8, 8); e.Graphics.DrawRectangle(pen, x - 5, y - 2, 8, 8); }
                    else e.Graphics.DrawRectangle(pen, x - 5, y - 5, 10, 10);
                }
            }
        }

        private void ToggleMaximize()
        {
            MaximizedBounds = Screen.FromControl(this).WorkingArea;
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (windowTitle != null) windowTitle.Text = Text;
        }
        protected override void OnResize(EventArgs e) { base.OnResize(e); if (chromeReady) UpdateWindowShape(); }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); if (chromeReady) UpdateWindowShape(); }
        private void UpdateWindowShape()
        {
            if (Width < 2 || Height < 2) return;
            var previous = Region;
            int corner = 2;
            bool smoothCorners = IsHandleCreated && DwmSetWindowAttribute(Handle, 33, ref corner, sizeof(int)) == 0;
            if (WindowState == FormWindowState.Maximized || smoothCorners) Region = null;
            else using (var shape = MaterialYou.Rounded(new Rectangle(0, 0, Width, Height), 16)) Region = new Region(shape);
            previous?.Dispose();
        }

        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            if (!chromeReady || !resizeWindow || WindowState != FormWindowState.Normal || message.Msg != 0x84) return;
            long location = message.LParam.ToInt64();
            var point = PointToClient(new Point((short)(location & 0xFFFF), (short)((location >> 16) & 0xFFFF)));
            int edge = Math.Max(6, DeviceDpi * 6 / 96);
            bool left = point.X < edge, right = point.X >= ClientSize.Width - edge;
            bool top = point.Y < edge, bottom = point.Y >= ClientSize.Height - edge;
            int hit = top ? (left ? 13 : right ? 14 : 12) : bottom ? (left ? 16 : right ? 17 : 15) : left ? 10 : right ? 11 : 1;
            message.Result = new IntPtr(hit);
        }
    }
}
