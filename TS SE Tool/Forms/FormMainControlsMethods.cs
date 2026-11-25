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
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.Win32;

using TS_SE_Tool.Utilities;

namespace TS_SE_Tool
{
    public partial class FormMain
    {
        //Menu controls
        private void programSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormProgramSettings FormWindow = new FormProgramSettings();
            FormWindow.ShowDialog();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings FormWindow = new FormSettings();
            FormWindow.ShowDialog();

            ApplySettings();
            ApplySettingsUI();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Language
        //tool strip click
        private void toolstripChangeLanguage(object sender, EventArgs e)
        {
            ToolStripItem obj = sender as ToolStripItem;
            string _objname = obj.Name;

            string[] cult = _objname.ToString().Split('_');
            string ButtonCultureInfo = cult[0] + "-" + cult[1];

            ProgSettingsV.Language = ButtonCultureInfo;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(ButtonCultureInfo);

            ChangeLanguage();
        }

        private void ChangeLanguage()
        {
            try
            {
                if (ProgSettingsV.Language != "Default")
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(ProgSettingsV.Language);//CultureInfo.GetCultureInfo("en-US");
            }
            catch
            {
                IO_Utilities.LogWriter("Wrong language setting format");
            }

            CultureInfo ci = Thread.CurrentThread.CurrentUICulture;

            try
            {
                this.SuspendLayout();

                HelpTranslateFormMethod(this, toolTipMain);
                HelpTranslateMenuStripMethod(menuStripMain);

                HelpTranslatContextMenuStripMethod(contextMenuStripMain);

                this.ResumeLayout();

                LngFileLoader("countries_translate.txt", CountriesLngDict, ProgSettingsV.Language);
                LngFileLoader("cities_translate.txt", CitiesLngDict, ProgSettingsV.Language);
                LngFileLoader("companies_translate.txt", CompaniesLngDict, ProgSettingsV.Language);
                LngFileLoader("cargo_translate.txt", CargoLngDict, ProgSettingsV.Language);
                LngFileLoader("urgency_translate.txt", UrgencyLngDict, ProgSettingsV.Language);
                //LngFileLoader("custom_strings.txt", CustomStringsDict, ProgSettingsV.Language);

                LoadTruckBrandsLng();
                LoadDriverNamesLng();

                AddTranslationToData();
                TranslateComboBoxes();
                CorrectControlsPositions();
            }
            catch
            { }

            //rm.ReleaseAllResources();
        }

        //About
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAboutBox aboutWindow = new FormAboutBox();
            aboutWindow.ShowDialog();
        }

        //How to
        private void localPDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string pdf_path = Directory.GetCurrentDirectory() + @"\HowTo.pdf";

            if (File.Exists(pdf_path))
                Process.Start(pdf_path);
            else
                JR.Utils.GUI.Forms.FlexibleMessageBox.Show("Missing manual. Try to repair via update", "HowTo.pdf not found");
        }

        private void youTubeVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Utilities.Web_Utilities.External.linkYoutubeTutorial);
        }

        //Downloads
        private void checkGitHubRelesesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Utilities.Web_Utilities.External.linkGithubReleases);
        }

        private void checkTMPForumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Utilities.Web_Utilities.External.linkTMPforum);
        }

        private void checkSCSForumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Utilities.Web_Utilities.External.linkSCSforum);
        }

        private void latestStableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCheckUpdates FormWindow = new FormCheckUpdates("check");
            FormWindow.ShowDialog();
        }

        //Menu controls End

        //Form methods
        private void ToggleControlsAccess(bool _state)
        {
            //Main Save controls
            buttonMainWriteSave.Enabled = _state;
            buttonMainWriteSave.Visible = _state;

            buttonMainCloseSave.Visible = _state;

            //Main tabs
            foreach (TabPage tp in tabControlMain.TabPages)            
                tp.Enabled = _state;            

            //Profile
            for (int i = 0; i < 6; i++)
            {
                Control[] tmp = this.Controls.Find("profileSkillsPanel" + i.ToString(), true);

                if (tmp[0] != null)
                {
                    Bitmap bgimg = new Bitmap(SkillImgS[i], 64, 64);

                    if (_state)
                        tmp[0].BackgroundImage = bgimg;
                    else
                        tmp[0].BackgroundImage = Graphics_TSSET.ConvertBitmapToGrayscale(bgimg);
                }
            }
        }

        private void ToggleMainControlsAccess(bool _state)
        {
            radioButtonMainGameSwitchETS.Enabled = _state;
            radioButtonMainGameSwitchATS.Enabled = _state;

            checkBoxProfilesAndSavesProfileBackups.Enabled = _state;
            buttonProfilesAndSavesRefreshAll.Enabled = _state;
            buttonProfilesAndSavesEditProfile.Enabled = _state;
            buttonProfilesAndSavesRestoreBackup.Enabled = _state;

            comboBoxRootFolders.Enabled = _state;
            comboBoxProfiles.Enabled = _state;
            comboBoxSaves.Enabled = _state;

            buttonMainDecryptSave.Enabled = _state;
            buttonMainLoadSave.Enabled = _state;

            buttonMainWriteSave.Enabled = _state;

            if (_state)
                CheckSaveControls();
        }

        private void CheckSaveControls()
        {
            // Root
            DataRowView drv = (DataRowView)comboBoxRootFolders.SelectedItem;

            if (drv == null) { SetSteamCloudWarning(false); buttonMainLoadSave.Enabled = false; return; }
            bool cloud = drv["ProfileType"].ToString() == "steam";
            SetSteamCloudWarning(cloud);
            Font loadButtonFont = buttonMainLoadSave.Font;

            // Change Load button properties based on Profile type
            if (cloud)
            {
                buttonMainLoadSave.Enabled = false;
                buttonMainLoadSave.Text = ResourceManagerMain.GetString(buttonMainLoadSave.Name + "SteamCloud"); // Disable Steam Cloud

                buttonMainLoadSave.Font = new Font(loadButtonFont.FontFamily, 12f, FontStyle.Bold);

                labelHelpText.Visible = true;
                SteamSelectedTimer.Stop();
            }
            else
            {
                buttonMainLoadSave.Enabled = true;
                buttonMainLoadSave.Text = ResourceManagerMain.GetString(buttonMainLoadSave.Name); // Load

                buttonMainLoadSave.Font = new Font(loadButtonFont.FontFamily, 18F, FontStyle.Bold);

                labelHelpText.Visible = false;
                SteamSelectedTimer.Stop();
                SteamSelectedTimer.Enabled = false;
            }

            //===
            // Save
            drv = (DataRowView)comboBoxSaves.SelectedItem;
            if (drv == null) { buttonMainLoadSave.Enabled = false; return; }

            string savePath = drv["savePath"].ToString() + @"\game.sii",
                   backupPath = drv["savePath"].ToString() + @"\game_backup.sii";

            //===
            // Backup button

            if (File.Exists(backupPath))
                buttonProfilesAndSavesRestoreBackup.Enabled = true;
            else
                buttonProfilesAndSavesRestoreBackup.Enabled = false;

            buttonMainLoadSave.Enabled = CanLoadSelectedSave();

            //===
            // Decode buton
            sbyte saveFileFormat = GetSaveFileFormat(savePath).saveFileFormat;

            if (saveFileFormat == 2 || saveFileFormat == 4)
                buttonMainDecryptSave.Enabled = true;
            else
                buttonMainDecryptSave.Enabled = false;
        }

        //Main part controls
        
        //Game select
        private bool switchingGame;
        public void ToggleGame_Click(object sender, EventArgs e)
        {
            if (switchingGame || !(sender is RadioButton selected) || !selected.Checked) return;
            switchingGame = true;
            try { ToggleGame(selected == radioButtonMainGameSwitchETS ? "ETS2" : "ATS"); }
            finally
            {
                radioButtonMainGameSwitchETS.Checked = GameType == "ETS2";
                radioButtonMainGameSwitchATS.Checked = GameType == "ATS";
                switchingGame = false;
            }
        }

        public void ToggleGame(string game)
        {
            TrySwitchGame(game, () => JR.Utils.GUI.Forms.FlexibleMessageBox.Show(
                "Savefile not saved.\r\nDo you want to discard changes and switch game type?", "Switching game",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes, FillRootFoldersPaths);
        }

        internal bool TrySwitchGame(string game, Func<bool> confirmDiscard, Action rescan)
        {
            if (game != "ETS2" && game != "ATS") throw new ArgumentException("Unknown game", nameof(game));
            if (game == GameType)
            {
                if (comboBoxRootFolders.DataSource == null) rescan();
                return true;
            }
            if (tempSavefileInMemory != null && !confirmDiscard()) return false;
            ToggleControlsAccess(false);
            SetSteamCloudWarning(false);
            GameType = game;
            SetDefaultValues(false);
            ClearFormControls(true);
            comboBoxRootFolders.DataSource = comboBoxProfiles.DataSource = comboBoxSaves.DataSource = null;
            Globals.ProfilesPaths = new string[0];
            Globals.ProfilesHex.Clear();
            Globals.SavesHex = new string[0];
            Globals.SelectedProfile = Globals.SelectedProfilePath = Globals.SelectedProfileName = "";
            Globals.SelectedSave = Globals.SelectedSavePath = Globals.SelectedSaveName = "";
            buttonMainLoadSave.Enabled = buttonMainDecryptSave.Enabled = false;
            comboBoxProfiles.Enabled = comboBoxSaves.Enabled = false;
            buttonMainLoadSave.Text = ResourceManagerMain.GetString(buttonMainLoadSave.Name) ?? "Load";
            rescan();
            CheckSaveControls();
            return true;
        }

        internal bool CanLoadSelectedSave()
        {
            var roots = comboBoxRootFolders.DataSource as DataTable;
            var root = comboBoxRootFolders.SelectedItem as DataRowView;
            if (roots == null || !Equals(roots.ExtendedProperties["GameType"], GameType) || root == null ||
                Convert.ToString(root["ProfileType"]) == "steam") return false;
            string profilePath = Convert.ToString(comboBoxProfiles.SelectedValue), savePath = Convert.ToString(comboBoxSaves.SelectedValue);
            if (string.IsNullOrEmpty(profilePath) || string.IsNullOrEmpty(savePath) || !Path.IsPathRooted(profilePath) || !Path.IsPathRooted(savePath)) return false;
            string saveParent = Path.GetDirectoryName(savePath);
            if (string.IsNullOrEmpty(saveParent)) return false;
            return string.Equals(Path.GetDirectoryName(profilePath), Convert.ToString(comboBoxRootFolders.SelectedValue), StringComparison.OrdinalIgnoreCase)
                && string.Equals(Path.GetDirectoryName(saveParent), profilePath, StringComparison.OrdinalIgnoreCase)
                && File.Exists(Path.Combine(savePath, "game.sii"));
        }


        private void buttonMainAddCustomFolder_Click(object sender, EventArgs e)
        {
            FormAddCustomFolder FormWindow = new FormAddCustomFolder();
            FormWindow.ShowDialog();
        }
        
        //Profile list
        private void buttonRefreshAll_Click(object sender, EventArgs e)
        {
            FillRootFoldersPaths(); // RePopulate root folders
        }

        private void buttonProfilesAndSavesEditProfile_Click(object sender, EventArgs e)
        {
            FormProfileEditor FormWindow = new FormProfileEditor();
            FormWindow.ParentForm = this;

            DialogResult t = FormWindow.ShowDialog();

            if (t != DialogResult.Cancel)
            {
                FillRootFoldersPaths(); // RePopulate root folders
            }
        }
        
        private void buttonProfilesAndSavesRestoreBackup_Click(object sender, EventArgs e)
        {
            //Set variables
            string SiiSavePath = Globals.SelectedSavePath + @"\game.sii", 
                   SiiSavePathBackup = Globals.SelectedSavePath + @"\game_backup.sii";

            //If backups exist
            if (File.Exists(SiiSavePathBackup))
            {
                DialogResult dr = JR.Utils.GUI.Forms.FlexibleMessageBox.Show("Restoring from backup file will overwrite existing save file." + Environment.NewLine +
                                                  "Select: Yes - Overwrite | No - Swap files | Cancel - Abort restoring.", 
                                                  "Restoring Save file from Backup", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                //If Cancel - exit Method
                if (dr == DialogResult.Cancel)
                    return;

                //Set variables
                string SiiInfoPath = Globals.SelectedSavePath + @"\info.sii",
                       SiiInfoPathBackup = Globals.SelectedSavePath + @"\info_backup.sii";

                if (dr == DialogResult.No)
                {
                    //Swap
                    SwapFiles(SiiSavePath, SiiSavePathBackup);

                    if (File.Exists(SiiInfoPathBackup))
                        SwapFiles(SiiInfoPath, SiiInfoPathBackup);
                }
                else
                {
                    //Overwrite
                    File.Copy(SiiSavePathBackup, SiiSavePath, true);
                    File.Delete(SiiSavePathBackup);

                    if (File.Exists(SiiInfoPathBackup))
                    {
                        File.Copy(SiiInfoPathBackup, SiiInfoPath, true);
                        File.Delete(SiiInfoPathBackup);
                    }                        
                }

                //Swap Files Function
                void SwapFiles(string _firstFile, string _secondFile)
                {
                    string tmpFile = Directory.GetParent(_firstFile).FullName + "\\tmp";

                    File.Copy(_firstFile, tmpFile, true);
                    File.Copy(_secondFile, _firstFile, true);
                    File.Copy(tmpFile, _secondFile, true);

                    File.Delete(tmpFile);
                }
            }
        }

        //Buttons
        private void buttonDecryptSave_Click(object sender, EventArgs e)
        {
            //Initial State Setup
            SetDefaultValues(false);
            ClearFormControls(true);

            ToggleMainControlsAccess(false);

            //Set variables
            Globals.SelectedSavePath = Convert.ToString(comboBoxSaves.SelectedValue);
            string SiiSavePath = Globals.SelectedSavePath + @"\game.sii";

            //Decrypt
            string[] file = NewDecodeFile(SiiSavePath);

            //Check result
            if (file != null)
            {
                IO_Utilities.LogWriter("Backing up file to: " + Globals.SelectedSavePath + @"\game_backup.sii");

                //Backup
                File.Copy(SiiSavePath, Globals.SelectedSavePath + @"\game_backup.sii", true);

                //Write Decrypted file
                File.WriteAllLines(SiiSavePath, file);

                UpdateStatusBarMessage.ShowStatusMessage(SMStatus.Clear);
            }
            else
                UpdateStatusBarMessage.ShowStatusMessage(SMStatus.Error, "error_could_not_decode_file");

            //Unlock controls
            ToggleMainControlsAccess(true);
            buttonMainDecryptSave.Enabled = false;

            ToggleGame(GameType);

            //GC
            GC.Collect();
            //GC.WaitForPendingFinalizers();
        }

        private void buttonOpenSaveFolder_Click(object sender, EventArgs e)
        {
            //Open Save Folder
            if (Directory.Exists(Globals.SavesHex[comboBoxSaves.SelectedIndex]))
                Process.Start(Globals.SavesHex[comboBoxSaves.SelectedIndex]);
        }

        internal static BackgroundWorker workerLoadSaveFile;
        private void LoadSaveFile_Click(object sender, EventArgs e)
        {
            if (!CanLoadSelectedSave()) { buttonMainLoadSave.Enabled = false; return; }
            //Initial State Setup
            ToggleMainControlsAccess(false);
            ToggleControlsAccess(false);
            ClearFormControls(true);

            SetDefaultValues(false);
            ClearJobData();

            //Load Save file

            //Set variables
            Globals.SelectedSavePath = Convert.ToString(comboBoxSaves.SelectedValue);
            Globals.SelectedSave = Globals.SelectedSavePath.Split(new string[] { "\\" }, StringSplitOptions.None).Last();
            Globals.SelectedSaveName = GetCustomSaveFilename(Globals.SelectedSavePath);

            Globals.SelectedProfilePath = Convert.ToString(comboBoxProfiles.SelectedValue);
            Globals.SelectedProfile = Globals.SelectedProfilePath.Split(new string[] { "\\" }, StringSplitOptions.None).Last();
            Globals.SelectedProfileName = Utilities.TextUtilities.FromHexToString(Globals.SelectedProfile);

            //Setup BG worker
            workerLoadSaveFile = new BackgroundWorker();
            workerLoadSaveFile.WorkerReportsProgress = true;

            workerLoadSaveFile.DoWork += LoadSaveFile;
            workerLoadSaveFile.ProgressChanged += worker_ProgressChanged;
            workerLoadSaveFile.RunWorkerCompleted += worker_RunWorkerCompleted;

            //Start BG worker
            workerLoadSaveFile.RunWorkerAsync();
        }

        private void buttonMainCloseSave_Click(object sender, EventArgs e)
        {
            ToggleControlsAccess(false);

            SetDefaultValues(false);

            ClearFormControls(true);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void buttonWriteSave_Click(object sender, EventArgs e)
        {
            if (extraDrivers.Count() > 0 || extraVehicles.Count() > 0)
            {
                DialogResult res = JR.Utils.GUI.Forms.FlexibleMessageBox.Show("Do you want to save Drivers and Trucks from sold garages?", "Attention! Loosing content", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                {
                    FormGaragesSoldContent testDialog = new FormGaragesSoldContent();
                    testDialog.ShowDialog(this);
                }
            }

            ToggleMainControlsAccess(false);
            ToggleControlsAccess(false);

            //Write

            workerLoadSaveFile = new BackgroundWorker();
            workerLoadSaveFile.WorkerReportsProgress = true;

            workerLoadSaveFile.DoWork += NewWrireSaveFile;
            workerLoadSaveFile.ProgressChanged += worker_ProgressChanged;
            workerLoadSaveFile.RunWorkerCompleted += workerWrite_RunWorkerCompleted;

            workerLoadSaveFile.RunWorkerAsync();
        }

        //Profile and Saves groupbox
        private void checkBoxProfileBackups_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxRootFolders.SelectedIndexChanged -= comboBoxRootFolders_SelectedIndexChanged;

            string sv = Convert.ToString(comboBoxRootFolders.SelectedValue);

            FillRootFoldersPaths(); // refresh list as backup entries was deleted\added

            int index = FindByValue(comboBoxRootFolders, sv); // try find previous value

            if (index > -1)
            {
                comboBoxRootFolders.SelectedValue = sv; // if exists - set as selected
                comboBoxRootFolders.SelectedIndexChanged += comboBoxRootFolders_SelectedIndexChanged;
            }
            else
            {
                comboBoxRootFolders.SelectedIndexChanged += comboBoxRootFolders_SelectedIndexChanged;
                comboBoxRootFolders.SelectedIndex = comboBoxRootFolders.Items.Count > 0 ? 0 : -1; // Select the first remaining root.
            }
        }

        public void FillRootFoldersPaths()
        {
            try
            {
                var roots = ProfilePaths.FindRoots(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    ProfilePaths.SteamLocations(), GameType, checkBoxProfilesAndSavesProfileBackups.Checked);
                DataTable combDT = new DataTable();
                combDT.ExtendedProperties["GameType"] = GameType;
                combDT.Columns.Add("ProfileID", typeof(string));
                combDT.Columns.Add("ProfileName", typeof(string));
                combDT.Columns.Add("ProfileType", typeof(string));
                var paths = new List<string>();
                foreach (var root in roots)
                {
                    string label = (root.Value == "steam" ? "[S] " : "[L] ") + Path.GetFileName(root.Key);
                    if (root.Value == "steam")
                        label += " (" + ProfilePaths.SteamAccountName(root.Key) + ")";
                    combDT.Rows.Add(root.Key, label, root.Value);
                    paths.Add(root.Key);
                }
                int cpIndex = 0;
                if (ProgSettingsV.CustomPaths.TryGetValue(GameType, out var customPaths))
                    foreach (string customPath in customPaths)
                    {
                        cpIndex++;
                        string path = Directory.Exists(Path.Combine(customPath, "profiles"))
                            ? Path.Combine(customPath, "profiles") : customPath;
                        if (Directory.Exists(path) && !paths.Contains(path, StringComparer.OrdinalIgnoreCase))
                        {
                            combDT.Rows.Add(path, "[C] Custom path " + cpIndex, "custom");
                            paths.Add(path);
                        }
                    }
                Globals.ProfilesPaths = paths.ToArray();
                comboBoxRootFolders.ValueMember = "ProfileID";
                comboBoxRootFolders.DisplayMember = "ProfileName";
                comboBoxRootFolders.DataSource = combDT;
                comboBoxRootFolders.Enabled = combDT.Rows.Count > 0;
                if (combDT.Rows.Count == 0)
                {
                    comboBoxProfiles.Enabled = false;
                    comboBoxSaves.Enabled = false;
                    JR.Utils.GUI.Forms.FlexibleMessageBox.Show("No save profiles found for " + GameType + "." + Environment.NewLine +
                        "This tool needs the game's save folder, not its installation folder." + Environment.NewLine +
                        "Save a game first, then refresh. For a custom game home, use Add Custom paths and choose the folder containing profiles.");
                }
            }
            catch (Exception ex)
            {
                IO_Utilities.ErrorLogWriter("Populating Root Profiles failed: " + ex);
            }
        }

        private void comboBoxRootFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxRootFolders.SelectedIndex < 0 || comboBoxRootFolders.SelectedIndex >= Globals.ProfilesPaths.Length ||
                !Directory.Exists(Globals.ProfilesPaths[comboBoxRootFolders.SelectedIndex]))
                return;

            // Disable save\profile control buttons
            buttonProfilesAndSavesEditProfile.Enabled = false;
            buttonMainDecryptSave.Enabled = false;
            buttonProfilesAndSavesOpenSaveFolder.Enabled = false;
            buttonMainLoadSave.Enabled = false;

            FillProfiles(); // Populate Profiles list

        }

        private void comboBoxRootFolders_DropDown(object sender, EventArgs e)
        {
            comboBoxRootFolders.SelectedIndexChanged -= comboBoxRootFolders_SelectedIndexChanged;

            string sv = Convert.ToString(comboBoxRootFolders.SelectedValue); //save selected value

            FillRootFoldersPaths(); // refresh list in case entries was deleted\added

            int index = FindByValue(comboBoxRootFolders, sv); // try find previous value

            if (index > -1)
            {
                comboBoxRootFolders.SelectedValue = sv; // if exists - set as selected
                comboBoxRootFolders.SelectedIndexChanged += comboBoxRootFolders_SelectedIndexChanged;
            }
            else
            {
                comboBoxRootFolders.SelectedIndexChanged += comboBoxRootFolders_SelectedIndexChanged;
                comboBoxRootFolders.SelectedIndex = comboBoxRootFolders.Items.Count > 0 ? 0 : -1; // Select the first remaining root.
            }
        }

        public void FillProfiles()
        {
            try
            {
                if (comboBoxRootFolders.SelectedIndex < 0 || comboBoxRootFolders.SelectedIndex >= Globals.ProfilesPaths.Length ||
                !Directory.Exists(Globals.ProfilesPaths[comboBoxRootFolders.SelectedIndex]))
                {
                    FillRootFoldersPaths();
                    return;
                }

                string ProfileName = "", 
                       SelectedFolder = comboBoxRootFolders.SelectedValue.ToString();                

                //Setup combobox DataTable
                DataTable combDT = new DataTable();
                DataColumn dc = new DataColumn("ProfilePath", typeof(string));
                combDT.Columns.Add(dc);

                dc = new DataColumn("ProfileName", typeof(string));
                combDT.Columns.Add(dc);

                DataColumn dcDisplay = new DataColumn("DisplayMember");
                dcDisplay.Expression = string.Format("IIF(ProfilePath <> 'null', {1}, '-- not found --')", "ProfilePath", "ProfileName");
                combDT.Columns.Add(dcDisplay);

                //Filter Profile folders
                Globals.ProfilesHex = Directory.GetDirectories(SelectedFolder).OrderByDescending(f => new FileInfo(f).LastWriteTime).ToList();

                if (Globals.ProfilesHex.Count > 0)
                {
                    List<string> NewProfileHex = new List<string>();

                    foreach (string profilePath in Globals.ProfilesHex)
                    {
                        string profileFolder = profilePath.Substring(profilePath.LastIndexOf(@"\") + 1);

                        if (!profileFolder.Contains(" ") && Directory.Exists(profilePath + @"\save"))
                        {
                            ProfileName = Utilities.TextUtilities.FromHexToString(Path.GetFileName(profilePath));

                            if (ProfileName != null)
                            {
                                combDT.Rows.Add(profilePath, ProfileName);
                                NewProfileHex.Add(profilePath);
                            }
                        }
                    }

                    Globals.ProfilesHex = NewProfileHex;

                    //
                    if (combDT.Rows.Count > 0)
                    {
                        comboBoxProfiles.Enabled = true;

                        buttonProfilesAndSavesEditProfile.Enabled = true;

                        UpdateStatusBarMessage.ShowStatusMessage(SMStatus.Clear);
                    }
                    else
                    {
                        combDT.Rows.Add("null");

                        comboBoxProfiles.Enabled = false;
                        comboBoxSaves.Enabled = false;

                        buttonProfilesAndSavesEditProfile.Enabled = false;

                        UpdateStatusBarMessage.ShowStatusMessage(SMStatus.Error, "error_No valid Saves was found");
                    }

                    comboBoxProfiles.ValueMember = "ProfilePath";
                    comboBoxProfiles.DisplayMember = "DisplayMember";

                    comboBoxProfiles.DataSource = combDT;
                }
                else
                {
                    comboBoxProfiles.Enabled = false;
                    comboBoxSaves.Enabled = false;

                    buttonProfilesAndSavesOpenSaveFolder.Enabled = false;
                    buttonMainDecryptSave.Enabled = false;

                    JR.Utils.GUI.Forms.FlexibleMessageBox.Show("Please select another folder", "No valid profiles found");
                }
            }
            catch
            {
                IO_Utilities.ErrorLogWriter("Populating Profiles list failed");
            }
        }

        private void comboBoxProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Globals.ProfilesHex.Count != 0)
            {
                try
                {
                    string AvatarPath = Globals.ProfilesHex[comboBoxProfiles.SelectedIndex] + @"\avatar.png";

                    if (File.Exists(AvatarPath))
                    {
                        Bitmap SourceImg = new Bitmap(AvatarPath);
                        Rectangle AvatarArea = new Rectangle(0, 0, 95, 95);
                        Bitmap CroppedImg = SourceImg.Clone(AvatarArea, SourceImg.PixelFormat);

                        pictureBoxProfileAvatar.Image = CroppedImg;
                    }
                    else
                    {
                        pictureBoxProfileAvatar.Image = MainIcons[0]; // placeholder icon
                    }
                }
                catch
                {
                    pictureBoxProfileAvatar.Image = MainIcons[0]; // placeholder icon
                }

                try
                {
                    //Read profile data
                    LoadProfileDataFile(Globals.ProfilesHex[comboBoxProfiles.SelectedIndex] + @"\profile.sii"); // Profile file path

                    //Add tooltip to Avatar
                    toolTipMain.SetToolTip(pictureBoxProfileAvatar, MainSaveFileProfileData.getProfileSummary(PlayerLevelNames)); // Profile stats
                }
                catch
                { }
            }
            else
            {
                pictureBoxProfileAvatar.Image = null; 
                //Add tooltip to Avatar
                toolTipMain.SetToolTip(pictureBoxProfileAvatar, "");
            }

            if (comboBoxProfiles.SelectedIndex > -1)
                FillProfileSaves(); // Populate Save folders list
        }

        private void comboBoxProfiles_DropDown(object sender, EventArgs e)
        {
            comboBoxProfiles.SelectedIndexChanged -= comboBoxProfiles_SelectedIndexChanged; // remove event to prevent unnecessary refreshing

            string sv = comboBoxProfiles.SelectedValue.ToString(); //save selected value

            FillProfiles(); // refresh list in case entries was deleted\added

            int index = FindByValue(comboBoxProfiles, sv); // try find previous value

            if (index > -1)
            {
                comboBoxProfiles.SelectedValue = sv; // if exists - set as selected
                comboBoxProfiles.SelectedIndexChanged += comboBoxProfiles_SelectedIndexChanged; // restore event
            }    
            else
            {
                comboBoxProfiles.SelectedIndexChanged += comboBoxProfiles_SelectedIndexChanged; // restore event
                comboBoxProfiles.SelectedIndex = 0; // if not - select first in the list
            }
        }

        public void FillProfileSaves()
        {
            try
            {
                if (Globals.ProfilesHex.Count != 0 && !Directory.Exists(Globals.ProfilesHex[comboBoxProfiles.SelectedIndex]))
                {
                    FillProfiles();
                    return;
                }

                Globals.SavesHex = new string[0];

                if (Globals.ProfilesHex.Count != 0)
                {
                    string SelectedSaveFolder = Globals.ProfilesHex[comboBoxProfiles.SelectedIndex] + @"\save";

                    if (Directory.Exists(SelectedSaveFolder))                    
                        Globals.SavesHex = Directory.GetDirectories(SelectedSaveFolder).OrderByDescending(f => new FileInfo(f).LastWriteTime).ToArray();                    
                    else                    
                        Globals.SavesHex = new string[0];                    
                }

                //Setup combobox DataTable
                DataTable combDT = new DataTable();
                DataColumn dc = new DataColumn("savePath", typeof(string));
                combDT.Columns.Add(dc);

                dc = new DataColumn("saveName", typeof(string));
                combDT.Columns.Add(dc);

                DataColumn dcDisplay = new DataColumn("DisplayMember");
                dcDisplay.Expression = string.Format("IIF(savePath <> 'null', {1}, '-- not found --')", "savePath", "saveName");
                combDT.Columns.Add(dcDisplay);

                //if save folder contains any folders
                if (Globals.SavesHex.Length > 0)
                {
                    bool NotANumber = false;

                    //Check if any of the initial folders is a valid save folder
                    foreach (string saveFolder in Globals.SavesHex)
                    {
                        if (!File.Exists(saveFolder + @"\game.sii") || !File.Exists(saveFolder + @"\info.sii"))
                            continue; //if folder does not contains essential files - skip it

                        string[] folders = saveFolder.Split(new string[] { "\\" }, StringSplitOptions.None);

                        if (folders.Last().Contains(' '))
                        {
                            string tmpName = GetCustomSaveFilename(saveFolder);

                            if (tmpName != "")
                                combDT.Rows.Add(saveFolder, tmpName);
                            else
                                combDT.Rows.Add(saveFolder, "NoName ( " + folders.Last() + " )");
                        }
                        else
                        {
                            foreach (char c in folders.Last())
                            {
                                if (c < '0' || c > '9')
                                {
                                    NotANumber = true;
                                    break;
                                }
                            }

                            if (NotANumber)
                            {
                                string[] namearr = folders.Last().Split(new char[] { '_' });
                                string ProfileName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namearr[0]);

                                for (int i = 1; i < namearr.Length; i++)
                                {
                                    ProfileName += " " + namearr[i];
                                }

                                combDT.Rows.Add(saveFolder, "- " + ProfileName + " -");
                            }
                            else
                            {
                                string tmpName = GetCustomSaveFilename(saveFolder);

                                if (tmpName != "")
                                    combDT.Rows.Add(saveFolder, tmpName);
                                else
                                    combDT.Rows.Add(saveFolder, "NoName ( " + folders.Last() + " )");
                            }

                            NotANumber = false;
                        }

                    }

                    //Check if save folders was found
                    if (combDT.Rows.Count > 0)
                    {
                        comboBoxSaves.Enabled = true;
                        buttonProfilesAndSavesOpenSaveFolder.Enabled = true;
                        buttonMainDecryptSave.Enabled = true;

                        UpdateStatusBarMessage.ShowStatusMessage(SMStatus.Clear);
                    }
                    else
                    {
                        combDT.Rows.Add("null"); //Add fake item to indicate zero saves found

                        comboBoxSaves.Enabled = false;
                        buttonProfilesAndSavesOpenSaveFolder.Enabled = false;
                        buttonMainDecryptSave.Enabled = false;
                        buttonMainLoadSave.Enabled = false;

                        UpdateStatusBarMessage.ShowStatusMessage(SMStatus.Error, "error_No valid Saves was found");
                    }

                    comboBoxSaves.ValueMember = "savePath";
                    comboBoxSaves.DisplayMember = "DisplayMember";

                    comboBoxSaves.DataSource = combDT;
                }
                else //if zero folders found in "save" folder
                {                    
                    combDT.Rows.Add("null"); //Add fake item to indicate zero saves found

                    comboBoxSaves.ValueMember = "savePath";
                    comboBoxSaves.DisplayMember = "DisplayMember";

                    comboBoxSaves.DataSource = combDT;

                    //Visuals
                    comboBoxSaves.Enabled = false;
                    buttonProfilesAndSavesOpenSaveFolder.Enabled = false;
                    buttonMainDecryptSave.Enabled = false;
                    buttonMainLoadSave.Enabled = false;

                    UpdateStatusBarMessage.ShowStatusMessage(SMStatus.Error, "error_No Save file folders found");
                }
            }
            catch
            {
                IO_Utilities.ErrorLogWriter("Populating Saves list failed");
            }
        }

        private void comboBoxSaves_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update save path
            Globals.SelectedSavePath = Convert.ToString(comboBoxSaves.SelectedValue);
            Globals.SelectedSave = Globals.SelectedSavePath.Split(new string[] { "\\" }, StringSplitOptions.None).Last();
            Globals.SelectedSaveName = GetCustomSaveFilename(Globals.SelectedSavePath);

            // Update Profile path
            Globals.SelectedProfilePath = Convert.ToString(comboBoxProfiles.SelectedValue);
            Globals.SelectedProfile = Globals.SelectedProfilePath.Split(new string[] { "\\" }, StringSplitOptions.None).Last();
            Globals.SelectedProfileName = Utilities.TextUtilities.FromHexToString(Globals.SelectedProfile);

            CheckSaveControls();
        }

        private void comboBoxSaves_DropDown(object sender, EventArgs e)
        {
            comboBoxSaves.SelectedIndexChanged -= new EventHandler(comboBoxSaves_SelectedIndexChanged);

            string sv = comboBoxSaves.SelectedValue.ToString(); //save selected value

            FillProfileSaves(); // refresh list in case entries was deleted\added

            int index = FindByValue(comboBoxSaves, sv); // try find previous value

            if (index > -1)
            {
                comboBoxSaves.SelectedValue = sv; // if exists - set as selected
                comboBoxSaves.SelectedIndexChanged += new EventHandler(comboBoxSaves_SelectedIndexChanged);
            }
            else
            {
                comboBoxSaves.SelectedIndexChanged += new EventHandler(comboBoxSaves_SelectedIndexChanged);
                comboBoxSaves.SelectedIndex = 0; // if not - select first in the list
            }
        }
        //end Profile and Saves groupbox
        //end Main part controls
    }
}
