using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace TS_SE_Tool
{
    internal static class MaterialYou
    {
        internal static readonly Color Surface = Color.White;
        internal static bool IsPaleAccent => TextOn(Primary) == Color.Black && Primary.R + Primary.G + Primary.B > 500;
        internal static Color Container => IsPaleAccent ? Mix(Primary, Color.FromArgb(232, 232, 232), 0.45) : Mix(Primary, Color.White, 0.94);
        internal static Color Primary { get; private set; } = Color.Black;
        internal static Color PrimaryContainer => IsPaleAccent ? Mix(Primary, Color.FromArgb(220, 220, 220), 0.35) : Mix(Primary, Color.White, 0.86);
        internal static readonly Color OnSurface = Color.FromArgb(29, 27, 32);
        internal static readonly Color OnContainer = Color.FromArgb(40, 40, 40);
        internal static readonly Color Outline = Color.FromArgb(150, 150, 150);
        private sealed class PanelColor { internal Color Value; }
        private static readonly ConditionalWeakTable<Control, PanelColor> panelColors = new ConditionalWeakTable<Control, PanelColor>();

        internal static Color Mix(Color first, Color second, double amount) => Color.FromArgb(
            (int)Math.Round(first.R + (second.R - first.R) * amount),
            (int)Math.Round(first.G + (second.G - first.G) * amount),
            (int)Math.Round(first.B + (second.B - first.B) * amount));

        internal static bool TryParseHex(string text, out Color color)
        {
            color = Color.Black;
            string hex = (text ?? "").Trim();
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length != 6 || !int.TryParse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int rgb)) return false;
            color = Color.FromArgb((rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255);
            return true;
        }

        internal static string Hex(Color color) => "#" + (color.ToArgb() & 0xFFFFFF).ToString("X6", CultureInfo.InvariantCulture);
        internal static void SetAccent(string hex) { Primary = TryParseHex(hex, out var color) ? color : Color.Black; }

        internal static Color TextOn(Color background)
        {
            Func<byte, double> linear = value => value / 255.0 <= 0.04045 ? value / 255.0 / 12.92 : Math.Pow((value / 255.0 + 0.055) / 1.055, 2.4);
            double luminance = 0.2126 * linear(background.R) + 0.7152 * linear(background.G) + 0.0722 * linear(background.B);
            return luminance > 0.179 ? Color.Black : Color.White;
        }

        internal static GraphicsPath Rounded(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = Math.Max(1, Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height)));
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        internal static void Apply(Control control)
        {
            if (SystemInformation.HighContrast) return;
            control.SuspendLayout();
            try
            {
                // Tiny fonts and custom colors are also used as data in the existing owner-drawn controls.
                if (control.Font.Size >= 7 && control.Font.FontFamily.Name != AppTypography.FamilyName)
                {
                    var font = AppTypography.Create(control.Font.Size, control.Font.Style);
                    control.Font = font;
                    control.Disposed += (sender, e) => font.Dispose();
                }
                if (control is MaterialTabControl navigation) navigation.PrepareNavigation();
                if (control.ForeColor.IsSystemColor) control.ForeColor = OnSurface;

                if (control is Form || control is TabPage)
                {
                    control.BackColor = Surface;
                    if (control is TabPage page) page.UseVisualStyleBackColor = false;
                }
                else if (control is GroupBox || control.Name == "pictureBoxCompanyLogo") control.BackColor = Container;
                else if (control is TextBoxBase || control is ListBox || control is ComboBox || control is NumericUpDown)
                {
                    control.BackColor = control is NumericUpDown || control.Parent is NumericUpDown ? Container : Color.White;
                    if (control is TextBoxBase text && !(control is MaterialTextBox) && !(control.Parent is NumericUpDown)) text.BorderStyle = BorderStyle.FixedSingle;
                    if (control is ListBox list) list.BorderStyle = BorderStyle.None;
                    if (control is ComboBox combo) combo.FlatStyle = FlatStyle.Flat;
                }
                else if (control is Panel && (control.BackColor.IsSystemColor ||
                    (panelColors.TryGetValue(control, out var previous) && previous.Value == control.BackColor)))
                {
                    control.BackColor = control.Parent == null ? Surface : control.Parent.BackColor;
                    panelColors.GetValue(control, key => new PanelColor()).Value = control.BackColor;
                }

                if (control is MaterialButton button) button.ApplyColors();
                if (control is MaterialRadioButton radio) radio.ApplyColors();
                if (control is MaterialSkillButton skill)
                {
                    skill.FlatAppearance.BorderSize = IsPaleAccent ? 1 : 0;
                    skill.FlatAppearance.BorderColor = Outline;
                }
                if (control is LinkLabel link)
                {
                    link.LinkColor = Primary;
                    link.ActiveLinkColor = OnContainer;
                    link.VisitedLinkColor = Primary;
                }
                if (control is ToolStrip strip)
                {
                    strip.BackColor = Container;
                    strip.ForeColor = OnSurface;
                    strip.Renderer = new ToolStripProfessionalRenderer(new MaterialMenuColors());
                }
                if (control is DataGridView grid)
                {
                    grid.BackgroundColor = Surface;
                    grid.BorderStyle = BorderStyle.None;
                    grid.GridColor = Outline;
                    grid.EnableHeadersVisualStyles = false;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = Container;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = OnSurface;
                    grid.DefaultCellStyle.BackColor = Color.White;
                    grid.DefaultCellStyle.ForeColor = OnSurface;
                    grid.DefaultCellStyle.SelectionBackColor = PrimaryContainer;
                    grid.DefaultCellStyle.SelectionForeColor = OnSurface;
                }
                foreach (Control child in control.Controls) Apply(child);
            }
            finally { control.ResumeLayout(true); }
        }

        internal static void PaintActionBackground(Control control, Graphics graphics)
        {
            Control parent = control.Parent;
            while (parent != null && parent.BackColor.A < 255) parent = parent.Parent;
            using (var background = new SolidBrush(parent == null ? Surface : parent.BackColor)) graphics.FillRectangle(background, control.ClientRectangle);
        }

        internal static void PaintAction(ButtonBase control, Graphics graphics, Color fill)
        {
            if (control.Width < 2 || control.Height < 2) return;
            var saved = graphics.Save();
            PaintActionBackground(control, graphics);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            using (var shape = Rounded(new Rectangle(0, 0, control.Width - 1, control.Height - 1), Math.Min(10, control.Height / 3)))
            using (var brush = new SolidBrush(fill))
            {
                graphics.FillPath(brush, shape);
                if (control.FlatAppearance.BorderSize > 0)
                    using (var pen = new Pen(control.FlatAppearance.BorderColor, control.FlatAppearance.BorderSize)) graphics.DrawPath(pen, shape);
            }
            var content = Rectangle.Inflate(control.ClientRectangle, -4, -1);
            Image backgroundImage = control.BackgroundImage;
            if (backgroundImage != null) PaintActionImage(graphics, backgroundImage, content);
            if (control.Image != null)
            {
                var imageArea = content;
                if (!string.IsNullOrEmpty(control.Text) && (control.TextImageRelation == TextImageRelation.ImageBeforeText || (control.ImageAlign == ContentAlignment.MiddleLeft && control.TextAlign == ContentAlignment.MiddleRight)))
                {
                    int side = Math.Min(32, Math.Max(1, content.Height - 4));
                    imageArea = new Rectangle(content.Left + 3, content.Top + (content.Height - side) / 2, side, side);
                    content.X += side + 8; content.Width -= side + 8;
                }
                PaintActionImage(graphics, control.Image, imageArea);
            }
            if (content.Width > 0 && content.Height > 0 && !string.IsNullOrEmpty(control.Text))
            {
                var flags = TextFormatFlags.NoPadding | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                flags |= content.Height >= control.Font.Height * 2 ? TextFormatFlags.WordBreak : TextFormatFlags.SingleLine;
                if (!control.UseMnemonic) flags |= TextFormatFlags.NoPrefix;
                TextRenderer.DrawText(graphics, control.Text, control.Font, content,
                    control.Enabled ? control.ForeColor : Mix(OnSurface, fill, 0.42), flags);
            }
            if (control.Focused)
                using (var path = Rounded(Rectangle.Inflate(control.ClientRectangle, -3, -3), 7))
                using (var pen = new Pen(TextOn(fill), 1.5f)) graphics.DrawPath(pen, path);
            graphics.Restore(saved);
        }

        private static void PaintActionImage(Graphics graphics, Image image, Rectangle bounds)
        {
            if (bounds.Width < 1 || bounds.Height < 1) return;
            double scale = Math.Min(1, Math.Min(bounds.Width / (double)image.Width, bounds.Height / (double)image.Height));
            int width = Math.Max(1, (int)(image.Width * scale)), height = Math.Max(1, (int)(image.Height * scale));
            graphics.DrawImage(image, new Rectangle(bounds.Left + (bounds.Width - width) / 2, bounds.Top + (bounds.Height - height) / 2, width, height));
        }

        internal static void StyleSkillImages(Image[] images)
        {
            if (SystemInformation.HighContrast) return;
            Color[] colors = { IsPaleAccent ? PrimaryContainer : Mix(Primary, Color.White, 0.88), PrimaryContainer, PrimaryContainer, IsPaleAccent ? Mix(Primary, OnSurface, 0.20) : Mix(Primary, Color.White, 0.18), Mix(Primary, Color.White, 0.12) };
            for (int i = 0; i < images.Length; i++)
            {
                // Repaint existing images so live skill controls never retain disposed backgrounds.
                var image = images[i] as Bitmap;
                if (image == null || image.Width != 46 || image.Height != 46 || image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    images[i]?.Dispose();
                    image = new Bitmap(46, 46);
                    images[i] = image;
                }
                using (var graphics = Graphics.FromImage(image))
                using (var path = Rounded(new Rectangle(1, 1, 44, 44), 12))
                using (var fill = new SolidBrush(colors[i]))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.Clear(Color.Transparent);
                    graphics.FillPath(fill, path);
                }
            }
        }

        private sealed class MaterialMenuColors : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground => Surface;
            public override Color ImageMarginGradientBegin => Surface;
            public override Color ImageMarginGradientMiddle => Surface;
            public override Color ImageMarginGradientEnd => Surface;
            public override Color MenuItemSelected => PrimaryContainer;
            public override Color MenuItemBorder => PrimaryContainer;
            public override Color MenuBorder => Outline;
            public override Color MenuItemSelectedGradientBegin => PrimaryContainer;
            public override Color MenuItemSelectedGradientEnd => PrimaryContainer;
            public override Color MenuItemPressedGradientBegin => PrimaryContainer;
            public override Color MenuItemPressedGradientMiddle => PrimaryContainer;
            public override Color MenuItemPressedGradientEnd => PrimaryContainer;
        }
    }

    public partial class MaterialForm : Form
    {
        private bool visualsPrepared;
        public MaterialForm()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        internal void PrepareVisuals()
        {
            if (visualsPrepared) return;
            visualsPrepared = true;
            SuspendLayout();
            try { EnsureChrome(); MaterialYou.Apply(this); }
            finally { ResumeLayout(true); }
        }
        protected override void OnShown(EventArgs e)
        {
            // Paint the owner before a startup handler opens a modal window over it.
            Refresh();
            base.OnShown(e);
        }
        protected override void SetVisibleCore(bool value)
        {
            if (value) PrepareVisuals();
            base.SetVisibleCore(value);
        }
    }

    public class MaterialButton : Button
    {
        private bool hovered, pressed;
        protected override bool ShowFocusCues => SystemInformation.HighContrast && base.ShowFocusCues;
        public MaterialButton()
        {
            SetStyle(ControlStyles.Opaque, false);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); ApplyColors(); Invalidate(); }
        protected override void OnCreateControl() { base.OnCreateControl(); ApplyColors(); }
        internal void ApplyColors()
        {
            if (SystemInformation.HighContrast || Name.StartsWith("buttonUC") || Name.StartsWith("buttonIC")) return;
            FlatStyle = FlatStyle.Flat;
            bool primary = Name == "buttonMainWriteSave" || Name == "buttonMainLoadSave";
            FlatAppearance.BorderSize = MaterialYou.IsPaleAccent || (primary && MaterialYou.TextOn(MaterialYou.Primary) == Color.Black) ? 1 : 0;
            FlatAppearance.BorderColor = MaterialYou.Outline;
            BackColor = primary ? (Enabled ? MaterialYou.Mix(MaterialYou.Primary, Color.White, 0.22) : MaterialYou.Mix(MaterialYou.Primary, Color.White, 0.80)) : MaterialYou.PrimaryContainer;
            ForeColor = primary && Enabled ? MaterialYou.TextOn(BackColor) : MaterialYou.OnContainer;
            FlatAppearance.MouseOverBackColor = MaterialYou.Mix(BackColor, MaterialYou.TextOn(BackColor), 0.06);
            FlatAppearance.MouseDownBackColor = MaterialYou.Mix(BackColor, MaterialYou.TextOn(BackColor), 0.12);
        }
        protected override void OnMouseEnter(EventArgs e) { hovered = true; base.OnMouseEnter(e); Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { hovered = pressed = false; base.OnMouseLeave(e); Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e) { pressed = e.Button == MouseButtons.Left; base.OnMouseDown(e); Invalidate(); }
        protected override void OnMouseUp(MouseEventArgs e) { pressed = false; base.OnMouseUp(e); Invalidate(); }
        protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode == Keys.Space) pressed = true; base.OnKeyDown(e); Invalidate(); }
        protected override void OnKeyUp(KeyEventArgs e) { pressed = false; base.OnKeyUp(e); Invalidate(); }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (SystemInformation.HighContrast) { base.OnPaintBackground(e); return; }
            MaterialYou.PaintActionBackground(this, e.Graphics);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (SystemInformation.HighContrast) { base.OnPaint(e); return; }
            Color fill = !Enabled ? BackColor : pressed ? FlatAppearance.MouseDownBackColor : hovered ? FlatAppearance.MouseOverBackColor : BackColor;
            if (fill.IsEmpty) fill = BackColor;
            MaterialYou.PaintAction(this, e.Graphics, fill);
        }
    }

    public class MaterialSkillButton : CheckBox
    {
        public MaterialSkillButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Opaque, false);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (SystemInformation.HighContrast) { base.OnPaint(e); return; }
            MaterialYou.PaintAction(this, e.Graphics, Parent == null ? MaterialYou.Container : Parent.BackColor);
        }
    }

    public class MaterialRadioButton : RadioButton
    {
        public MaterialRadioButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Opaque, false);
        }
        protected override bool ShowFocusCues => SystemInformation.HighContrast && base.ShowFocusCues;
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ApplyColors();
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            ApplyColors();
            base.OnCheckedChanged(e);
        }

        internal void ApplyColors()
        {
            if (SystemInformation.HighContrast) return;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = MaterialYou.IsPaleAccent ? 1 : 0;
            FlatAppearance.BorderColor = MaterialYou.Outline;
            BackColor = Checked ? MaterialYou.PrimaryContainer : MaterialYou.Container;
            ForeColor = MaterialYou.OnContainer;
            FlatAppearance.MouseOverBackColor = MaterialYou.PrimaryContainer;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (SystemInformation.HighContrast || Appearance != Appearance.Button) { base.OnPaint(e); return; }
            MaterialYou.PaintAction(this, e.Graphics, BackColor);
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (SystemInformation.HighContrast || Appearance != Appearance.Button) { base.OnPaintBackground(e); return; }
            MaterialYou.PaintActionBackground(this, e.Graphics);
        }
    }

    public class MaterialGroupBox : GroupBox
    {
        public MaterialGroupBox()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (SystemInformation.HighContrast) { base.OnPaint(e); return; }
            if (Width < 2 || Height < 2) return;
            MaterialYou.PaintActionBackground(this, e.Graphics);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = MaterialYou.Rounded(bounds, 12))
            using (var fill = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(fill, path);
                if (MaterialYou.IsPaleAccent)
                    using (var border = new Pen(MaterialYou.Outline)) e.Graphics.DrawPath(border, path);
            }
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(10, 1, Math.Max(1, Width - 20), Font.Height + 2),
                Enabled ? MaterialYou.OnContainer : SystemColors.GrayText,
                TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter);
        }
    }

    public class MaterialTabControl : TabControl
    {
        private bool fittingTabs;
        private bool typographyReady;
        internal void PrepareNavigation()
        {
            if (SystemInformation.HighContrast) return;
            if (!typographyReady)
            {
                typographyReady = true;
                foreach (TabPage page in TabPages) page.Font = AppTypography.Create(Math.Max(9, page.Font.Size), page.Font.Style);
                Font = AppTypography.Create(9.5f);
            }
            FitTabs();
        }
        public MaterialTabControl()
        {
            if (!SystemInformation.HighContrast)
            {
                DrawMode = TabDrawMode.OwnerDrawFixed;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                SizeMode = TabSizeMode.Fixed;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (!SystemInformation.HighContrast)
            {
                Padding = new Point(8, 8);
                PrepareNavigation();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            FitTabs();
        }

        private void FitTabs()
        {
            if (SystemInformation.HighContrast || TabCount == 0 || fittingTabs) return;
            var size = new Size(Math.Max(48, (ClientSize.Width - 8) / TabCount), Name == "tabControlMain" ? 68 : 44);
            if (ItemSize == size) return;
            fittingTabs = true;
            try { ItemSize = size; }
            finally { fittingTabs = false; }
            Invalidate();
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (SystemInformation.HighContrast) { base.OnPaint(e); return; }
            using (var background = new SolidBrush(MaterialYou.Surface)) e.Graphics.FillRectangle(background, ClientRectangle);
            for (int i = 0; i < TabCount; i++)
                OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, GetTabRect(i), i,
                    i == SelectedIndex ? DrawItemState.Selected : DrawItemState.Default));
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (SystemInformation.HighContrast) { base.OnDrawItem(e); return; }
            if (e.Index < 0 || e.Index >= TabPages.Count) return;
            var bounds = GetTabRect(e.Index);
            using (var background = new SolidBrush(MaterialYou.Surface)) e.Graphics.FillRectangle(background, bounds);
            bool selected = SelectedIndex == e.Index;
            if (selected)
            {
                var pill = Rectangle.Inflate(bounds, -2, -2);
                if (pill.Width > 0 && pill.Height > 0)
                    using (var path = MaterialYou.Rounded(pill, 14))
                    using (var fill = new SolidBrush(MaterialYou.PrimaryContainer))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(fill, path);
                    }
            }
            var page = TabPages[e.Index];
            var textBounds = Rectangle.Inflate(bounds, -8, 0);
            if (page.ImageIndex >= 0)
            {
                Size size = new Size(20, 20);
                MaterialIcons.Draw(e.Graphics, new Rectangle(bounds.Left + (bounds.Width - size.Width) / 2, bounds.Top + 8, size.Width, size.Height), page.Name, MaterialYou.OnContainer);
                textBounds = new Rectangle(bounds.Left + 4, bounds.Top + 32, bounds.Width - 8, bounds.Height - 34);
            }
            TextRenderer.DrawText(e.Graphics, page.Text, Font, textBounds,
                Enabled ? MaterialYou.OnContainer : SystemColors.GrayText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
            if (Focused && selected && keyboardFocus)
                using (var path = MaterialYou.Rounded(Rectangle.Inflate(bounds, -3, -3), 10))
                using (var pen = new Pen(MaterialYou.Primary, 2)) e.Graphics.DrawPath(pen, path);
            base.OnDrawItem(e);
        }
        private bool keyboardFocus;
        protected override void OnKeyDown(KeyEventArgs e) { keyboardFocus = true; base.OnKeyDown(e); Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e) { keyboardFocus = false; base.OnMouseDown(e); Invalidate(); }
        protected override bool ShowFocusCues => SystemInformation.HighContrast && base.ShowFocusCues;
    }
}
