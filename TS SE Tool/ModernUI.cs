using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TS_SE_Tool
{
    internal static class AppTypography
    {
        private static readonly PrivateFontCollection fonts = new PrivateFontCollection();
        private static readonly FontFamily family = Load();
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        private static extern int AddFontResourceEx(string path, uint flags, IntPtr reserved);

        private static FontFamily Load()
        {
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fonts");
            if (!Directory.Exists(directory)) directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "fonts");
            if (!Directory.Exists(directory)) directory = Path.Combine(Directory.GetCurrentDirectory(), "fonts");
            foreach (string name in new[] { "NotoSans-Regular.ttf", "NotoSans-Bold.ttf" })
            {
                string path = Path.GetFullPath(Path.Combine(directory, name));
                if (!File.Exists(path)) throw new FileNotFoundException("The bundled Noto Sans font is missing. Extract the complete app folder.", path);
                AddFontResourceEx(path, 0x10, IntPtr.Zero); // FR_PRIVATE: available only to this process.
                fonts.AddFontFile(path);
            }
            return fonts.Families[0];
        }

        internal static string FamilyName => family.Name;
        internal static Font Create(float size, FontStyle style = FontStyle.Regular) => new Font(family, size, style);
    }

    internal static class MaterialIcons
    {
        internal static Bitmap Create(string name, int size = 20)
        {
            var image = new Bitmap(size, size);
            using (var graphics = Graphics.FromImage(image)) Draw(graphics, new Rectangle(0, 0, size, size), name, SystemInformation.HighContrast ? SystemColors.MenuText : MaterialYou.OnSurface);
            return image;
        }

        internal static void Draw(Graphics graphics, Rectangle bounds, string name, Color color)
        {
            var state = graphics.Save();
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.TranslateTransform(bounds.X, bounds.Y);
            graphics.ScaleTransform(bounds.Width / 24f, bounds.Height / 24f);
            using (var pen = new Pen(color, 1.7f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
            {
                if (name.Contains("Profile"))
                {
                    graphics.DrawEllipse(pen, 8, 3, 8, 8);
                    graphics.DrawArc(pen, 4, 13, 16, 14, 180, 180);
                }
                else if (name.Contains("Company"))
                {
                    graphics.DrawRectangle(pen, 5, 3, 14, 18);
                    for (int y = 7; y <= 15; y += 4) { graphics.DrawLine(pen, 9, y, 10, y); graphics.DrawLine(pen, 14, y, 15, y); }
                }
                else if (name.Contains("Trailer"))
                {
                    graphics.DrawRectangle(pen, 2, 5, 20, 12);
                    graphics.DrawEllipse(pen, 5, 16, 4, 4); graphics.DrawEllipse(pen, 16, 16, 4, 4);
                }
                else if (name.Contains("Truck"))
                {
                    graphics.DrawRectangle(pen, 2, 5, 12, 12);
                    graphics.DrawLines(pen, new[] { new Point(14, 10), new Point(19, 10), new Point(22, 14), new Point(22, 17), new Point(14, 17) });
                    graphics.DrawEllipse(pen, 5, 16, 4, 4); graphics.DrawEllipse(pen, 16, 16, 4, 4);
                }
                else if (name.Contains("Convoy"))
                {
                    graphics.DrawEllipse(pen, 2, 3, 5, 5); graphics.DrawEllipse(pen, 17, 16, 5, 5);
                    graphics.DrawBezier(pen, 7, 5, 25, 4, 0, 22, 17, 18);
                }
                else if (name.Contains("Freight"))
                {
                    graphics.DrawRectangle(pen, 2, 4, 13, 16);
                    graphics.DrawLine(pen, 6, 8, 11, 8);
                    graphics.DrawLine(pen, 11, 15, 22, 15);
                    graphics.DrawLines(pen, new[] { new Point(18, 11), new Point(22, 15), new Point(18, 19) });
                }
                else if (name == "program")
                {
                    for (int x = 4; x <= 14; x += 10) for (int y = 4; y <= 14; y += 10) graphics.DrawRectangle(pen, x, y, 6, 6);
                }
                else if (name == "help")
                {
                    graphics.DrawEllipse(pen, 3, 3, 18, 18);
                    graphics.DrawArc(pen, 8, 7, 8, 6, 190, 290); graphics.DrawLine(pen, 12, 13, 12, 15); graphics.DrawLine(pen, 12, 18, 12, 18.2f);
                }
                else
                {
                    graphics.DrawPolygon(pen, new[] { new Point(3, 7), new Point(12, 3), new Point(21, 7), new Point(21, 17), new Point(12, 21), new Point(3, 17) });
                    graphics.DrawLines(pen, new[] { new Point(3, 7), new Point(12, 11), new Point(21, 7) }); graphics.DrawLine(pen, 12, 11, 12, 21);
                }
            }
            graphics.Restore(state);
        }
    }

    public class MaterialComboBox : ComboBox
    {
        internal int MinimumItemHeight { get; set; } = 16;
        public MaterialComboBox()
        {
            if (SystemInformation.HighContrast) return;
            FlatStyle = FlatStyle.Flat;
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = Math.Max(MinimumItemHeight, Font.Height);
        }
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (!SystemInformation.HighContrast) ItemHeight = Math.Max(MinimumItemHeight, Font.Height);
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (SystemInformation.HighContrast) { base.OnDrawItem(e); return; }
            bool selected = (e.State & DrawItemState.Selected) != 0 && (e.State & DrawItemState.ComboBoxEdit) == 0;
            using (var fill = new SolidBrush(selected ? MaterialYou.PrimaryContainer : MaterialYou.Container)) e.Graphics.FillRectangle(fill, e.Bounds);
            string text = e.Index >= 0 && e.Index < Items.Count ? GetItemText(Items[e.Index]) : Text;
            TextRenderer.DrawText(e.Graphics, text, Font, Rectangle.Inflate(e.Bounds, -8, 0),
                Enabled ? MaterialYou.OnSurface : SystemColors.GrayText, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
            base.OnDrawItem(e);
        }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); if (DropDownStyle == ComboBoxStyle.DropDown) SelectionLength = 0; Invalidate(); }
    }

    public class MaterialTextBox : TextBox
    {
        public MaterialTextBox()
        {
            BorderStyle = BorderStyle.FixedSingle;
        }
    }

    public class MaterialColorPreview : Label
    {
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            MaterialYou.PaintActionBackground(this, e.Graphics);
            if (Width < 2 || Height < 2) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var shape = MaterialYou.Rounded(ClientRectangle, 8))
            using (var fill = new SolidBrush(BackColor)) e.Graphics.FillPath(fill, shape);
        }
    }

    public class MaterialNumber : NumericUpDown
    {
        public MaterialNumber()
        {
            BorderStyle = BorderStyle.FixedSingle;
        }
    }
}
