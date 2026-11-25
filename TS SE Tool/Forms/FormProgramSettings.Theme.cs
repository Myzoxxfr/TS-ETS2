using System;
using System.Drawing;
using System.Windows.Forms;

namespace TS_SE_Tool
{
    public partial class FormProgramSettings
    {
        private TextBox themeHex;
        private ComboBox themePresets;
        private Label themePreview;
        private Label themeError;
        private readonly string[] themePresetColors = { "#000000", "#6750A4", "#0061A4", "#386A20", "#984061", "#825500", null };

        private void InitializeAppearanceSettings()
        {
            var group = new MaterialGroupBox { Name = "groupTheme", Text = "Appearance", Dock = DockStyle.Fill, Padding = new Padding(10, 22, 10, 8) };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 5 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            foreach (int height in new[] { 32, 34, 40, 22, 24 }) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            themePresets = new TS_SE_Tool.MaterialComboBox { Name = "themePresets", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            themePresets.Items.AddRange(new object[] { "Black and white (default)", "Lavender", "Ocean blue", "Forest green", "Rose", "Amber", "Custom" });
            themeHex = new TS_SE_Tool.MaterialTextBox { Name = "themeHex", AccessibleName = "Theme color hex code", Dock = DockStyle.Fill, MaxLength = 7 };
            var picker = new MaterialButton { Name = "themePicker", Text = "Pick color…", Dock = DockStyle.Fill };
            themePreview = new MaterialColorPreview { Name = "themePreview", Text = "Accent preview", Dock = DockStyle.Fill, Margin = new Padding(3, 4, 3, 4), TextAlign = ContentAlignment.MiddleCenter };
            themeError = new Label { Name = "themeError", Dock = DockStyle.Fill, ForeColor = Color.Firebrick, TextAlign = ContentAlignment.MiddleLeft };
            layout.Controls.Add(new Label { Text = "Preset", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            layout.Controls.Add(themePresets, 1, 0);
            layout.SetColumnSpan(themePresets, 2);
            layout.Controls.Add(new Label { Text = "Hex code", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            layout.Controls.Add(themeHex, 1, 1);
            layout.Controls.Add(picker, 2, 1);
            layout.Controls.Add(themePreview, 0, 2);
            layout.SetColumnSpan(themePreview, 3);
            layout.Controls.Add(themeError, 0, 3);
            layout.SetColumnSpan(themeError, 3);
            var hint = new Label { Text = "Save applies the theme to all open windows.", Dock = DockStyle.Fill };
            layout.Controls.Add(hint, 0, 4);
            layout.SetColumnSpan(hint, 3);
            group.Controls.Add(layout);
            tableLayoutPanel1.Controls.Add(group, 0, 4);
            tableLayoutPanel1.SetColumnSpan(group, 2);

            themePresets.SelectedIndexChanged += (sender, e) =>
            {
                string value = themePresets.SelectedIndex < 0 ? null : themePresetColors[themePresets.SelectedIndex];
                if (value != null) themeHex.Text = value;
            };
            themeHex.TextChanged += (sender, e) => UpdateThemePreview();
            picker.Click += (sender, e) =>
            {
                MaterialYou.TryParseHex(themeHex.Text, out var color);
                using (var dialog = new MaterialColorDialog(color))
                    if (dialog.ShowDialog(this) == DialogResult.OK) themeHex.Text = MaterialYou.Hex(dialog.PrimaryColor);
            };
            themeHex.Text = MainForm.ProgSettingsV.ThemeAccent;
        }

        private void UpdateThemePreview()
        {
            if (!MaterialYou.TryParseHex(themeHex.Text, out var color))
            {
                themeError.Text = "Enter six hex digits, for example #0061A4.";
                return;
            }
            themeError.Text = "";
            string hex = MaterialYou.Hex(color);
            int index = Array.IndexOf(themePresetColors, hex);
            themePresets.SelectedIndex = index < 0 ? themePresetColors.Length - 1 : index;
            themePreview.BackColor = color;
            themePreview.ForeColor = MaterialYou.TextOn(color);
        }

        private bool SaveThemeSetting()
        {
            if (!MaterialYou.TryParseHex(themeHex.Text, out var color))
            {
                UpdateThemePreview();
                themeHex.Focus();
                themeHex.SelectAll();
                return false;
            }
            MainForm.ProgSettingsV.ThemeAccent = MaterialYou.Hex(color);
            return true;
        }
    }
}
