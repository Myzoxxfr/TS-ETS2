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
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace TS_SE_Tool
{
    partial class FormAboutBox : MaterialForm
    {
        FormMain MainForm = Application.OpenForms.OfType<FormMain>().Single();

        public FormAboutBox()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            Font = AppTypography.Create(9);
            ClientSize = new Size(500, 560);
            MinimumSize = new Size(460, 500);
            tableLayoutPanel.ColumnStyles[0].Width = 38;
            tableLayoutPanel.ColumnStyles[1].Width = 62;
            tableLayoutPanel.RowStyles[0].Height = 32;
            tableLayoutPanel.RowStyles[2].Height = 28;
            tableLayoutPanel.RowStyles[4].Height = 48;
            tableLayoutPanel.RowStyles[7].Height = tableLayoutPanel.RowStyles[8].Height = 28;
            tableLayoutPanel.RowStyles[10] = new RowStyle(SizeType.Percent, 100);
            tableLayoutPanel.Controls.Remove(buttonSupportDeveloper);
            tableLayoutPanel.SetRow(okButton, 12);
            tableLayoutPanel.RowCount = 13;
            while (tableLayoutPanel.RowStyles.Count > 13) tableLayoutPanel.RowStyles.RemoveAt(13);
            tableLayoutPanel.RowStyles[11] = new RowStyle(SizeType.Absolute, 12);
            tableLayoutPanel.RowStyles[12] = new RowStyle(SizeType.Absolute, 46);
            okButton.Dock = DockStyle.Fill;
            okButton.Margin = new Padding(6);
            foreach (Control child in tableLayoutPanel.Controls)
                if (child is Label label) { label.MaximumSize = Size.Empty; label.MinimumSize = new Size(0, 22); }
            textBoxDescription.WordWrap = true;
            textBoxDescription.ScrollBars = ScrollBars.Vertical;

            SetFormVisual();
            PopulateFormControls();
            TranslateForm();
        }

        private void SetFormVisual()
        {
            this.Icon = Utilities.Graphics_TSSET.IconFromImage(MainForm.ProgUIImgsDict["Info"]);
        }

        private void PopulateFormControls()
        {
            buttonSupportDeveloper.Visible = false;

            labelProductName.Text = Utilities.AssemblyData.AssemblyProduct;
            labelCopyright.Text = Utilities.AssemblyData.AssemblyCopyright;

            labelETS2version.Text = MainForm.SupportedGameVersionETS2;
            labelATSversion.Text = MainForm.SupportedGameVersionATS;

            //
            string[][] referencies = { 
                new string[] {"SII Decrypt", "https://github.com/ncs-sniper/SII_Decrypt"},
                new string[] {"PsColorPicker", "https://github.com/exectails/PsColorPicker"},
                new string[] {"SharpZipLib", "https://github.com/icsharpcode/SharpZipLib"},
                new string[] {"SqlCeBulkCopy", "https://github.com/ErikEJ/SqlCeBulkCopy"},
                new string[] {"DDSImageParser.cs", "https://gist.github.com/soeminnminn/e9c4c99867743a717f5b"},
                new string[] {"TGASharpLib", "https://github.com/ALEXGREENALEX/TGASharpLib"},
                new string[] {"FlexibleMessageBox", "http://www.codeproject.com/Articles/601900/FlexibleMessageBox"},
            };

            string referenciesText = "";

            foreach (string[] tmp in referencies)
            {
                referenciesText += tmp[0] + Environment.NewLine + tmp[1] + Environment.NewLine + Environment.NewLine;
            }
            //
            textBoxDescription.Text = string.Format(MainForm.HelpTranslateString(this.Name + textBoxDescription.Name),
                                                     Utilities.Web_Utilities.External.linkGithub);

            textBoxDescription.Text += referenciesText;
            //
        }

        private void TranslateForm()
        {
            MainForm.HelpTranslateFormMethod(this);

            MainForm.HelpTranslateControlExt(this, Utilities.AssemblyData.AssemblyTitle);
            MainForm.HelpTranslateControlExt(labelVersion, Utilities.AssemblyData.AssemblyVersion);
        }

        private void buttonSupportDeveloper_Click(object sender, EventArgs e)
        {
            string url = Utilities.Web_Utilities.External.linkHelpDeveloper;

            DialogResult result = JR.Utils.GUI.Forms.FlexibleMessageBox.Show("This will open " + url + " web-page." + Environment.NewLine + "Do you want to continue?",
                                                  "Support developer", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
                System.Diagnostics.Process.Start(url);
        }
    }
}
