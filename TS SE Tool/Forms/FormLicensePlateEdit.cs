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
    public partial class FormLicensePlateEdit : MaterialForm
    {
        TS_SE_Tool.FormMain MainForm = Application.OpenForms.OfType<TS_SE_Tool.FormMain>().Single();
        public string licenseplatetext = "";

        public FormLicensePlateEdit(string _licenseplatetext)
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            Font = AppTypography.Create(10);
            ClientSize = new Size(840, 460);
            MinimumSize = new Size(780, 420);
            MaximumSize = Size.Empty;
            tableLayoutPanel1.Padding = new Padding(14);
            tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 75);
            tableLayoutPanel1.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 25);
            tableLayoutPanel1.RowStyles[0].Height = 30;
            tableLayoutPanel1.RowStyles[1].Height = 74;
            tableLayoutPanel1.RowStyles[2].Height = 48;
            tableLayoutPanel1.RowStyles[4].Height = 46;
            tableLayoutPanel1.RowStyles[5].Height = 0;
            tableLayoutPanel1.SetColumnSpan(panel2, 2);
            panel2.AutoScroll = true;
            textBoxLicensePlateCountry.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            buttonShowTagHelp.Visible = false;
            buttonOk.Height = buttonCancel.Height = 36;

            this.Icon = Utilities.Graphics_TSSET.IconFromImage(MainForm.ProgUIImgsDict["Settings"]);

            try
            {
                string translatedString = MainForm.ResourceManagerMain.GetString(this.Name, Thread.CurrentThread.CurrentUICulture);
                if (translatedString != null)
                    this.Text = translatedString;
            }
            catch
            { }

            this.SuspendLayout();

            CreateControls();

            MainForm.HelpTranslateFormMethod(this);

            licenseplatetext = _licenseplatetext;

            string[] lpParts = licenseplatetext.Split(new char[] { '|' });

            textBoxLicensePlateNumber.Text = lpParts[0];
            textBoxLicensePlateCountry.Text = lpParts[1];
            
            this.ResumeLayout();
        }
        private void FormTruckLicensePlateEdit_Shown(object sender, EventArgs e)
        {
            buttonCancel.Focus();
        }

        private void CreateControls()
        {
            this.AcceptButton = buttonOk;
            this.CancelButton = buttonCancel;

            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            SetTagHelpText();
        }

        private void SetTagHelpText()
        {
            panel2.Controls.Clear();
            var help = new TableLayoutPanel { Name = "licensePlateHelp", Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
            help.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            help.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            string[] tags = { "<color value=FFBBGGRR>", "<font xscale=0.5 yscale=0.5> </font>", "<offset hshift=5 vshift=-7>",
                "<img src=/material/ui/lp/COUNTRY/NAME.mat width=10 height=15>", "<align left=23> <align right=23> </align>", "<sup> </sup> <sub> </sub>", "<ret>" };
            string[] meanings = { "Colour in HEX format ABGR", "Font scale", "Offset the next symbols and images", "Add an image and set its size",
                "Place symbols in columns", "Upper and lower index", "Start a new column or plate layer" };
            for (int i = 0; i < tags.Length; i++)
            {
                help.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                help.Controls.Add(new Label { Text = tags[i], AutoSize = true, MaximumSize = new Size(480, 0), Margin = new Padding(3, 4, 10, 4), UseMnemonic = false }, 0, i);
                help.Controls.Add(new Label { Text = meanings[i], AutoSize = true, MaximumSize = new Size(300, 0), Margin = new Padding(3, 4, 3, 4) }, 1, i);
            }
            panel2.Controls.Add(help);
        }

        private void textBoxLicensePlateNumber_TextChanged(object sender, EventArgs e)
        {
            licenseplatetext = textBoxLicensePlateNumber.Text + '|' + textBoxLicensePlateCountry.Text;

            SCS.SCSLicensePlate thisLP = new SCS.SCSLicensePlate(licenseplatetext, SCS.SCSLicensePlate.LPtype.Truck);

            panelLicensePlatePreview.BackgroundImage = Utilities.Graphics_TSSET.ResizeImage(thisLP.LicensePlateIMG, MainForm.LicensePlateWidth[MainForm.GameType], 32); //ETS - 128x32 or ATS - 128x64
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
