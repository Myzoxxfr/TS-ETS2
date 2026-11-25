using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace TS_SE_Tool
{
    public class MaterialMessageDialog : MaterialForm
    {
        internal Label MessageLabel { get; private set; }

        public MaterialMessageDialog(string message, string caption, MessageBoxButtons buttons,
            MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, IDictionary<DialogResult, string> labels = null)
        {
            Text = string.IsNullOrEmpty(caption) ? "TS-ETS2" : caption;
            Name = "MaterialMessageDialog";
            Font = AppTypography.Create(10);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            MinimizeBox = MaximizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            int width = Math.Min(560, Math.Max(320, Screen.PrimaryScreen.WorkingArea.Width - 64));
            var content = new Panel { Name = "messageBody", Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(24) };
            MessageLabel = new Label { Name = "messageText", Text = message ?? "", AutoSize = true, MaximumSize = new Size(width - 72, 0), Location = new Point(24, 24), UseMnemonic = false };
            content.Controls.Add(MessageLabel);
            int bodyHeight = TextRenderer.MeasureText(MessageLabel.Text, Font, new Size(width - 72, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + 52;
            ClientSize = new Size(width, Math.Max(148, Math.Min(bodyHeight + 72, Screen.PrimaryScreen.WorkingArea.Height - 120)));
            var actions = new FlowLayoutPanel { Name = "messageActions", Dock = DockStyle.Bottom, Height = 72, Padding = new Padding(16), FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            DialogResult[] results;
            switch (buttons)
            {
                case MessageBoxButtons.YesNo: results = new[] { DialogResult.Yes, DialogResult.No }; break;
                case MessageBoxButtons.YesNoCancel: results = new[] { DialogResult.Yes, DialogResult.No, DialogResult.Cancel }; break;
                case MessageBoxButtons.OKCancel: results = new[] { DialogResult.OK, DialogResult.Cancel }; break;
                case MessageBoxButtons.RetryCancel: results = new[] { DialogResult.Retry, DialogResult.Cancel }; break;
                case MessageBoxButtons.AbortRetryIgnore: results = new[] { DialogResult.Abort, DialogResult.Retry, DialogResult.Ignore }; break;
                default: results = new[] { DialogResult.OK }; break;
            }
            var controls = new List<Button>();
            foreach (var result in results)
            {
                string label = labels != null && labels.TryGetValue(result, out var localized) ? localized : result.ToString();
                var button = new MaterialButton { Name = "action" + result, Text = label, DialogResult = result, Size = new Size(Math.Max(96, TextRenderer.MeasureText(label, Font).Width + 32), 38), TabIndex = controls.Count };
                controls.Add(button);
                if (result == DialogResult.Cancel || results.Length == 1) CancelButton = button;
            }
            foreach (var button in controls.AsEnumerable().Reverse()) actions.Controls.Add(button);
            int index = Math.Min(controls.Count - 1, Math.Max(0, (int)defaultButton / 256));
            AcceptButton = controls[index];
            Shown += (sender, e) => controls[index].Focus();
            ControlBox = CancelButton != null;
            FormClosing += (sender, e) => { if (e.CloseReason == CloseReason.UserClosing && CancelButton == null && !results.Contains(DialogResult)) e.Cancel = true; };
            Controls.Add(content);
            Controls.Add(actions);
            if (icon == MessageBoxIcon.Error && !SystemInformation.HighContrast) MessageLabel.ForeColor = Color.FromArgb(130, 30, 30);
            Resize += (sender, e) => MessageLabel.MaximumSize = new Size(Math.Max(1, content.ClientSize.Width - 72), 0);
            KeyPreview = true;
            KeyDown += (sender, e) => { if (e.Control && e.KeyCode == Keys.C) { Clipboard.SetText(Text + Environment.NewLine + MessageLabel.Text); e.Handled = true; } };
        }
    }

    internal class ColorSurface : Control
    {
        internal bool HueStrip;
        internal double Hue, Saturation = 1, Brightness = 1;
        internal event EventHandler ColorChanged;
        internal Color SelectedColor => FromHsv(Hue, Saturation, Brightness);

        internal ColorSurface(bool hueStrip)
        {
            HueStrip = hueStrip;
            TabStop = true;
            AccessibleName = hueStrip ? "Hue; use left and right arrows" : "Saturation and brightness; use arrow keys";
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        internal static Color FromHsv(double hue, double saturation, double value)
        {
            double c = value * saturation, x = c * (1 - Math.Abs((hue / 60) % 2 - 1)), m = value - c;
            double r = 0, g = 0, b = 0;
            switch ((int)(hue % 360) / 60)
            {
                case 0: r = c; g = x; break;
                case 1: r = x; g = c; break;
                case 2: g = c; b = x; break;
                case 3: g = x; b = c; break;
                case 4: r = x; b = c; break;
                case 5: r = c; b = x; break;
            }
            return Color.FromArgb((int)Math.Round((r + m) * 255), (int)Math.Round((g + m) * 255), (int)Math.Round((b + m) * 255));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 2 || Height < 2) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            MaterialYou.PaintActionBackground(this, e.Graphics);
            var clipState = e.Graphics.Save();
            using (var path = MaterialYou.Rounded(new Rectangle(0, 0, Width - 1, Height - 1), 10))
            {
                e.Graphics.SetClip(path, CombineMode.Intersect);
                if (HueStrip)
                {
                    using (var rainbow = new LinearGradientBrush(ClientRectangle, Color.Red, Color.Red, 0f))
                    {
                        rainbow.InterpolationColors = new ColorBlend { Colors = new[] { Color.Red, Color.Yellow, Color.Lime, Color.Cyan, Color.Blue, Color.Magenta, Color.Red }, Positions = new[] { 0f, 1f / 6, 2f / 6, 3f / 6, 4f / 6, 5f / 6, 1f } };
                        e.Graphics.FillRectangle(rainbow, ClientRectangle);
                    }
                }
                else
                {
                    using (var fill = new SolidBrush(FromHsv(Hue, 1, 1))) e.Graphics.FillRectangle(fill, ClientRectangle);
                    using (var white = new LinearGradientBrush(ClientRectangle, Color.White, Color.Transparent, 0f)) e.Graphics.FillRectangle(white, ClientRectangle);
                    using (var black = new LinearGradientBrush(ClientRectangle, Color.Transparent, Color.Black, 90f)) e.Graphics.FillRectangle(black, ClientRectangle);
                }
                e.Graphics.Restore(clipState);
            }
            float px = (float)(HueStrip ? Hue / 359 : Saturation) * (Width - 1);
            float py = HueStrip ? Height / 2f : (float)(1 - Brightness) * (Height - 1);
            px = Math.Max(8, Math.Min(Width - 9, px));
            py = Math.Max(8, Math.Min(Height - 9, py));
            using (var dark = new Pen(Color.Black, 4)) e.Graphics.DrawEllipse(dark, px - 6, py - 6, 12, 12);
            using (var light = new Pen(Color.White, Focused ? 3 : 2)) e.Graphics.DrawEllipse(light, px - 6, py - 6, 12, 12);
        }

        private void SelectAt(Point point)
        {
            double horizontal = Math.Max(0, Math.Min(1, point.X / (double)Math.Max(1, Width - 1)));
            if (HueStrip) Hue = horizontal * 359;
            else { Saturation = horizontal; Brightness = 1 - Math.Max(0, Math.Min(1, point.Y / (double)Math.Max(1, Height - 1))); }
            Invalidate();
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if (e.Button == MouseButtons.Left) { Focus(); Capture = true; SelectAt(e.Location); } }
        protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); if (Capture) SelectAt(e.Location); }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); Capture = false; }
        protected override bool IsInputKey(Keys keyData) => (keyData & Keys.KeyCode) == Keys.Left || (keyData & Keys.KeyCode) == Keys.Right || (keyData & Keys.KeyCode) == Keys.Up || (keyData & Keys.KeyCode) == Keys.Down || base.IsInputKey(keyData);
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                double delta = e.KeyCode == Keys.Left ? -1 : 1;
                if (HueStrip) Hue = Math.Max(0, Math.Min(359, Hue + delta));
                else Saturation = Math.Max(0, Math.Min(1, Saturation + delta / 100));
            }
            else if (!HueStrip && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)) Brightness = Math.Max(0, Math.Min(1, Brightness + (e.KeyCode == Keys.Up ? 0.01 : -0.01)));
            else return;
            e.Handled = true;
            Invalidate();
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class MaterialColorDialog : MaterialForm
    {
        private readonly ColorSurface plane = new ColorSurface(false), hue = new ColorSurface(true);
        private readonly TextBox hex = new MaterialTextBox { Name = "colorHex", MaxLength = 7, Width = 120 };
        private readonly NumericUpDown[] rgb = new NumericUpDown[3];
        private readonly Label preview = new MaterialColorPreview { Name = "colorPreview", AutoSize = false, Height = 42, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
        private readonly Button accept = new MaterialButton { Text = "Apply", Name = "acceptColor", DialogResult = DialogResult.OK, Width = 110, Height = 38 };
        private bool updating;
        public Color PrimaryColor { get; private set; }

        public MaterialColorDialog(Color startingColor)
        {
            Name = "MaterialColorDialog";
            Text = "Choose color";
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            Font = AppTypography.Create(10);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ClientSize = new Size(460, 466);
            MinimizeBox = MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var layout = new TableLayoutPanel { Name = "colorPickerLayout", Dock = DockStyle.Fill, Padding = new Padding(20, 12, 20, 12), ColumnCount = 1, RowCount = 7 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            foreach (int height in new[] { 30, 40, 38, 38, 44, 48 })
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));

            plane.Name = "colorPlane";
            hue.Name = "colorHue";
            plane.Dock = hue.Dock = DockStyle.Fill;
            preview.Margin = plane.Margin = hue.Margin = new Padding(3);
            layout.Controls.Add(plane, 0, 0);
            layout.Controls.Add(hue, 0, 1);
            layout.Controls.Add(preview, 0, 2);

            var hexRow = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, ColumnCount = 2 };
            hexRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
            hexRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            hexRow.Controls.Add(new Label { Text = "Hex", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            hex.Anchor = AnchorStyles.Left;
            hexRow.Controls.Add(hex, 1, 0);
            layout.Controls.Add(hexRow, 0, 3);

            var rgbRow = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, ColumnCount = 6 };
            for (int i = 0; i < 3; i++)
            {
                string channel = "RGB"[i].ToString();
                rgbRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, i == 0 ? 44 : 26));
                rgbRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
                rgbRow.Controls.Add(new Label { Text = channel, AutoSize = true, Anchor = AnchorStyles.Left }, i * 2, 0);
                rgb[i] = new MaterialNumber { Name = "color" + channel, AccessibleName = channel, Minimum = 0, Maximum = 255, Anchor = AnchorStyles.Left | AnchorStyles.Right };
                rgbRow.Controls.Add(rgb[i], i * 2 + 1, 0);
                rgb[i].ValueChanged += (sender, e) => { if (!updating) SetColor(Color.FromArgb((int)rgb[0].Value, (int)rgb[1].Value, (int)rgb[2].Value)); };
            }
            layout.Controls.Add(rgbRow, 0, 4);

            var palette = new[] { "#000000", "#FFFFFF", "#6750A4", "#0061A4", "#386A20", "#984061", "#825500", "#006A60", "#BA1A1A" };
            var swatches = new TableLayoutPanel { Name = "colorSwatches", Dock = DockStyle.Fill, Margin = Padding.Empty, ColumnCount = palette.Length, RowCount = 1 };
            for (int i = 0; i < palette.Length; i++)
            {
                string value = palette[i];
                MaterialYou.TryParseHex(value, out var color);
                swatches.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / palette.Length));
                var swatch = new MaterialButton { Name = "buttonUCColor", AccessibleName = value, BackColor = color, Dock = DockStyle.Fill, Margin = new Padding(5, 7, 5, 7) };
                swatch.FlatStyle = FlatStyle.Flat;
                swatch.FlatAppearance.BorderSize = 1;
                swatch.FlatAppearance.BorderColor = MaterialYou.Outline;
                swatch.Click += (sender, e) => SetColor(color);
                swatches.Controls.Add(swatch, i, 0);
            }
            layout.Controls.Add(swatches, 0, 5);

            var actions = new FlowLayoutPanel { Name = "colorActions", Dock = DockStyle.Fill, Margin = Padding.Empty, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            var cancel = new MaterialButton { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 110, Height = 38 };
            actions.Controls.Add(accept);
            actions.Controls.Add(cancel);
            layout.Controls.Add(actions, 0, 6);
            Controls.Add(layout);
            ResumeLayout(true);
            AcceptButton = accept;
            CancelButton = cancel;
            hue.ColorChanged += (sender, e) => { plane.Hue = hue.Hue; plane.Invalidate(); SetColor(plane.SelectedColor, true); };
            plane.ColorChanged += (sender, e) => SetColor(plane.SelectedColor, true);
            hex.TextChanged += (sender, e) =>
            {
                if (updating) return;
                accept.Enabled = MaterialYou.TryParseHex(hex.Text, out var color);
                if (accept.Enabled) SetColor(color);
            };
            SetColor(startingColor);
        }

        private void SetColor(Color color, bool preserveHsv = false)
        {
            if (updating) return;
            updating = true;
            try
            {
                PrimaryColor = color;
                if (!preserveHsv)
                {
                    plane.Hue = hue.Hue = color.GetHue();
                    double max = Math.Max(color.R, Math.Max(color.G, color.B)), min = Math.Min(color.R, Math.Min(color.G, color.B));
                    plane.Saturation = max == 0 ? 0 : 1 - min / max;
                    plane.Brightness = max / 255;
                }
                hex.Text = MaterialYou.Hex(color);
                rgb[0].Value = color.R; rgb[1].Value = color.G; rgb[2].Value = color.B;
                preview.BackColor = color; preview.ForeColor = MaterialYou.TextOn(color); preview.Text = hex.Text;
                plane.Invalidate(); hue.Invalidate(); accept.Enabled = true;
            }
            finally { updating = false; }
        }
    }
}
