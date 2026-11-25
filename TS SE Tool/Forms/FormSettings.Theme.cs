using System;
using System.Drawing;
using System.Windows.Forms;

namespace TS_SE_Tool
{
    public partial class FormSettings
    {
        private void InitializeThemeSettings()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            Font = AppTypography.Create(9);
            MinimumSize = MaximumSize = Size.Empty;
            ClientSize = new Size(520, 340);
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            tableLayoutPanel1.SetRow(buttonSave, 1);
            tableLayoutPanel1.SetRow(buttonCancel, 1);
            BuildSettingRows();
        }

        private void BuildSettingRows()
        {
            panel1.Controls.Clear();
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(12, 8, 12, 8) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int row = 0; row < 6; row++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            var time = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            numericUpDownSettingPickTimeD.Width = numericUpDownSettingPickTimeH.Width = 62;
            labelDayShort.AutoSize = labelHourShort.AutoSize = true;
            time.Controls.AddRange(new Control[] { numericUpDownSettingPickTimeD, labelDayShort, numericUpDownSettingPickTimeH, labelHourShort });
            var loop = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            numericUpDownSettingLoopCitys.Width = 62;
            labelCity.AutoSize = true;
            loop.Controls.AddRange(new Control[] { numericUpDownSettingLoopCitys, labelCity });
            Control[] labels = { labelJobPickupTime, labelLoopEvery, labelDistance, labelWeight, labelCurrencyETS2, labelCurrencyATS };
            Control[] values = { time, loop, comboBoxSettingDistanceMesSelect, comboBoxWeightMesSelect, comboBoxSettingCurrencySelectETS2, comboBoxSettingCurrencySelectATS };
            labelCurrencyETS2.Text = "Currency · ETS2";
            labelCurrencyATS.Text = "Currency · ATS";
            for (int row = 0; row < labels.Length; row++)
            {
                labels[row].Dock = DockStyle.Fill;
                if (labels[row] is Label label) { label.AutoSize = false; label.TextAlign = ContentAlignment.MiddleLeft; label.Margin = new Padding(3); }
                values[row].Dock = DockStyle.Fill;
                if (values[row] is ComboBox choice) choice.DropDownStyle = ComboBoxStyle.DropDownList;
                grid.Controls.Add(labels[row], 0, row);
                grid.Controls.Add(values[row], 1, row);
            }
            panel1.Controls.Add(grid);
        }

    }
}
