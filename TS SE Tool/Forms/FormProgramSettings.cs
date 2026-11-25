/*
   Copyright 2016-2022 LIPtoH <liptoh.codebase@gmail.com>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace TS_SE_Tool
{
    public partial class FormProgramSettings : MaterialForm
    {
        FormMain MainForm = Application.OpenForms.OfType<FormMain>().Single();
        private readonly CheckBox askBeforeClosing = new CheckBox { Name = "checkBoxAskBeforeClosing", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Right, CheckAlign = ContentAlignment.MiddleCenter };

        public FormProgramSettings()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            Font = AppTypography.Create(9.5f);
            ClientSize = new Size(560, 500);
            MinimumSize = new Size(520, 470);
            tableLayoutPanel1.Padding = new Padding(16, 8, 16, 8);
            tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 85);
            tableLayoutPanel1.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 15);
            tableLayoutPanel1.RowStyles[1].Height = tableLayoutPanel1.RowStyles[2].Height = 40;
            tableLayoutPanel1.RowStyles[3].Height = 40;
            tableLayoutPanel1.RowStyles[0].Height = 12;
            tableLayoutPanel1.RowStyles[4] = new RowStyle(SizeType.Percent, 100);
            tableLayoutPanel1.RowStyles[5] = new RowStyle(SizeType.Absolute, 8);
            tableLayoutPanel1.RowStyles[6].Height = 46;
            tableLayoutPanel1.Controls.Add(new Label { Name = "labelAskBeforeClosing", Text = "Ask before closing", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
            tableLayoutPanel1.Controls.Add(askBeforeClosing, 1, 3);

            this.Icon = Utilities.Graphics_TSSET.IconFromImage(MainForm.ProgUIImgsDict["ProgramSettings"]);

            this.SuspendLayout();

            MainForm.HelpTranslateControl(this);

            MainForm.HelpTranslateFormMethod(this);

            this.ResumeLayout();

            LoadSettings();
            InitializeAppearanceSettings();
        }

        private void LoadSettings()
        {
            //Loading settings Setting checkboxes
            askBeforeClosing.Checked = MainForm.ProgSettingsV.AskBeforeClosing;
            checkBoxShowSplashOnStartup.Checked = !MainForm.ProgSettingsV.HideStartupPopup && Properties.Settings.Default.ShowSplashOnStartup;
            checkBoxCheckUpdatesOnStartup.Checked = Properties.Settings.Default.CheckUpdatesOnStartup;
        }

        internal bool ApplyProgramSettings()
        {
            if (!SaveThemeSetting()) return false;
            MainForm.ProgSettingsV.HideStartupPopup = !checkBoxShowSplashOnStartup.Checked;
            MainForm.ProgSettingsV.AskBeforeClosing = askBeforeClosing.Checked;
            MainForm.ProgSettingsV.WriteConfigToFile();
            MainForm.RefreshTheme();
            return true;
        }

        private void SaveSettings()
        {
            if (!ApplyProgramSettings()) return;
            Properties.Settings.Default.ShowSplashOnStartup = checkBoxShowSplashOnStartup.Checked;
            Properties.Settings.Default.CheckUpdatesOnStartup = checkBoxCheckUpdatesOnStartup.Checked;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

    }
}
