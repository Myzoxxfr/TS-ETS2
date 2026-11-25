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
using System.IO;
using System.Globalization;
using System.Threading;

namespace TS_SE_Tool
{
    public partial class FormSettings : MaterialForm
    {
        FormMain MainForm = Application.OpenForms.OfType<FormMain>().Single();

        public FormSettings()
        {
            InitializeComponent();
            InitializeThemeSettings();

            this.Icon = Utilities.Graphics_TSSET.IconFromImage(MainForm.ProgUIImgsDict["Settings"]);            

            this.SuspendLayout();

            try
            {
                string translatedString = MainForm.ResourceManagerMain.GetString(this.Name, Thread.CurrentThread.CurrentUICulture);
                if (translatedString != null)
                    this.Text = translatedString;
            }
            catch
            { }

            MainForm.HelpTranslateFormMethod(this, toolTip);

            CorrectControlsPositions();
            this.ResumeLayout();

            PopulateControls();
        }

        private void CorrectControlsPositions()
        {
            panel1.PerformLayout();
        }

        private void PopulateControls()
        {
            bool _fixSettings = false;
            DataTable combDT;

            //Distances choice
            combDT = new DataTable();
            combDT.Columns.Add("ID");
            combDT.Columns.Add("DistDisplayName");

            Dictionary<string, string> DistanceMesNames = new Dictionary<string, string> { { "km", "Kilometers" }, { "mi", "Miles" } };

            foreach (KeyValuePair<string, string> tempitem in DistanceMesNames)
            {
                string value = MainForm.ResourceManagerMain.GetString(tempitem.Value, Thread.CurrentThread.CurrentUICulture);

                if (value != null && value != "")
                {
                    combDT.Rows.Add(tempitem.Key, value);
                }
                else
                {
                    combDT.Rows.Add(tempitem.Key, tempitem.Value);
                }
            }

            comboBoxSettingDistanceMesSelect.ValueMember = "ID";
            comboBoxSettingDistanceMesSelect.DisplayMember = "DistDisplayName";
            comboBoxSettingDistanceMesSelect.DataSource = combDT;
            comboBoxSettingDistanceMesSelect.SelectedValue = MainForm.ProgSettingsV.DistanceMes;

            if (comboBoxSettingDistanceMesSelect.SelectedValue == null)
            {
                _fixSettings = true;
                comboBoxSettingDistanceMesSelect.SelectedIndex = 0;
            }


            //Weight choice
            combDT = new DataTable();
            combDT.Columns.Add("ID");
            combDT.Columns.Add("DistDisplayName");

            Dictionary<string, string> WeightMesNames = new Dictionary<string, string> { { "kg", "Kilograms" }, { "lb", "Pounds" } };

            foreach (KeyValuePair<string, string> tempitem in WeightMesNames)
            {
                string value = MainForm.ResourceManagerMain.GetString(tempitem.Value, Thread.CurrentThread.CurrentUICulture);

                if (value != null && value != "")
                {
                    combDT.Rows.Add(tempitem.Key, value);
                }
                else
                {
                    combDT.Rows.Add(tempitem.Key, tempitem.Value);
                }
            }

            comboBoxWeightMesSelect.ValueMember = "ID";
            comboBoxWeightMesSelect.DisplayMember = "DistDisplayName";
            comboBoxWeightMesSelect.DataSource = combDT;
            comboBoxWeightMesSelect.SelectedValue = MainForm.ProgSettingsV.WeightMes;

            if (comboBoxWeightMesSelect.SelectedValue == null)
            {
                _fixSettings = true;
                comboBoxWeightMesSelect.SelectedIndex = 0;
            }


            //Currency choise
            //ETS2
            combDT = new DataTable();
            combDT.Columns.Add("ID");
            combDT.Columns.Add("CurrencyDisplayName");

            foreach (KeyValuePair<string, double> tempitem in MainForm.CurrencyDictConversionETS2)
            {
                string value = MainForm.ResourceManagerMain.GetString(tempitem.Key, Thread.CurrentThread.CurrentUICulture);

                if (value != null && value != "")
                {
                    combDT.Rows.Add(tempitem.Key, value);
                }
                else
                {
                    combDT.Rows.Add(tempitem.Key, tempitem.Key);
                }
            }

            comboBoxSettingCurrencySelectETS2.ValueMember = "ID";
            comboBoxSettingCurrencySelectETS2.DisplayMember = "CurrencyDisplayName";
            comboBoxSettingCurrencySelectETS2.DataSource = combDT;
            comboBoxSettingCurrencySelectETS2.SelectedValue = MainForm.ProgSettingsV.CurrencyMesETS2;

            if (comboBoxSettingCurrencySelectETS2.SelectedValue == null)
            {
                _fixSettings = true;
                comboBoxSettingCurrencySelectETS2.SelectedIndex = 0;
            }
            //ATS
            combDT = new DataTable();
            combDT.Columns.Add("ID");
            combDT.Columns.Add("CurrencyDisplayName");

            foreach (KeyValuePair<string, double> tempitem in MainForm.CurrencyDictConversionATS)
            {
                string value = MainForm.ResourceManagerMain.GetString(tempitem.Key, Thread.CurrentThread.CurrentUICulture);

                if (value != null && value != "")
                {
                    combDT.Rows.Add(tempitem.Key, value);
                }
                else
                {
                    combDT.Rows.Add(tempitem.Key, tempitem.Key);
                }
            }

            comboBoxSettingCurrencySelectATS.ValueMember = "ID";
            comboBoxSettingCurrencySelectATS.DisplayMember = "CurrencyDisplayName";
            comboBoxSettingCurrencySelectATS.DataSource = combDT;
            comboBoxSettingCurrencySelectATS.SelectedValue = MainForm.ProgSettingsV.CurrencyMesATS;

            if (comboBoxSettingCurrencySelectATS.SelectedValue == null)
            {
                _fixSettings = true;
                comboBoxSettingCurrencySelectATS.SelectedIndex = 0;
            }


            //Pickup time intervals
            numericUpDownSettingPickTimeD.Value = Math.Floor((decimal)(MainForm.ProgSettingsV.JobPickupTime / 24));
            numericUpDownSettingPickTimeH.Value = MainForm.ProgSettingsV.JobPickupTime - numericUpDownSettingPickTimeD.Value * 24;

            //Loop width
            numericUpDownSettingLoopCitys.Value = MainForm.ProgSettingsV.LoopEvery;

            if (_fixSettings)
                PrepareSettingsAndSave();
        }
        
        //Events
        
        //Pickup window
        private void numericUpDownSettingPickTimeD_ValueChanged(object sender, EventArgs e)
        {
            numericUpDownSettingPickTimeH.Value = 0;
        }

        private void numericUpDownSettingPickTimeH_ValueChanged(object sender, EventArgs e)
        {

            if (numericUpDownSettingPickTimeH.Value == 24)
            {
                numericUpDownSettingPickTimeD.Value++;
                numericUpDownSettingPickTimeH.Value = 0;
            }
            else if (numericUpDownSettingPickTimeH.Value == -1)
            {
                if(numericUpDownSettingPickTimeD.Value > 0)
                {
                    numericUpDownSettingPickTimeD.Value--;
                    numericUpDownSettingPickTimeH.Value = 0;
                }
                else
                {
                    numericUpDownSettingPickTimeH.Value = 0;
                }
            }
        }

        //
        private void numericUpDownSettingLoopCitys_ValueChanged(object sender, EventArgs e)
        {
            MainForm.ProgSettingsV.LoopEvery = Convert.ToByte(numericUpDownSettingLoopCitys.Value);
        }

        //Buttons
        private void buttonSettingSave_Click(object sender, EventArgs e)
        {
            PrepareSettingsAndSave();
        }

        private void PrepareSettingsAndSave()
        {
            MainForm.ProgSettingsV.DistanceMes = comboBoxSettingDistanceMesSelect.SelectedValue.ToString();
            MainForm.ProgSettingsV.WeightMes = comboBoxWeightMesSelect.SelectedValue.ToString();
            MainForm.ProgSettingsV.CurrencyMesETS2 = comboBoxSettingCurrencySelectETS2.SelectedValue.ToString();
            MainForm.ProgSettingsV.CurrencyMesATS = comboBoxSettingCurrencySelectATS.SelectedValue.ToString();
            MainForm.ProgSettingsV.JobPickupTime = (short)(numericUpDownSettingPickTimeH.Value + numericUpDownSettingPickTimeD.Value * 24);
            MainForm.ProgSettingsV.LoopEvery = (byte)(numericUpDownSettingLoopCitys.Value);

            MainForm.ProgSettingsV.WriteConfigToFile();
            MainForm.RefreshTheme();

            if (MainForm.GameType == "ETS2")
            {
                Globals.CurrencyName = MainForm.ProgSettingsV.CurrencyMesETS2;
            }
            else
            {
                Globals.CurrencyName = MainForm.ProgSettingsV.CurrencyMesATS;
            }
        }

        private void buttonSettingCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
