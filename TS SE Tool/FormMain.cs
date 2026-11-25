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
using System.Data.SqlServerCe;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Deployment.Application;
using System.Threading;

using TS_SE_Tool.Utilities;
using JR.Utils.GUI.Forms;

namespace TS_SE_Tool
{
    public partial class FormMain : MaterialForm
    {
        #region  Accesslevels

        internal int[] SupportedSavefileVersionETS2; //Program
        internal string SupportedGameVersionETS2;//Program
        internal int[] SupportedSavefileVersionATS;
        internal string SupportedGameVersionATS;//Program

        private int JobsAmountAdded;//process result

        private int[] UrgencyArray;//Program

        public bool FileDecoded;  //+

        internal string GameType;//Program

        private string LoopStartCity;//Program
        private string LoopStartCompany;//Program
        private string unCertainRouteLength;//Program

        private bool InfoDepContinue;//Program

        private string ProfileETS2;//Program
        private string ProfileATS;//Program

        //Raw data in memory
        private string[] tempProfileFileInMemory; //Program
        private string[] tempInfoFileInMemory;//Program
        private string[] tempSavefileInMemory; //+
        //
        private Dictionary<string, List<JobAdded>> AddedJobsDictionary;//Program
        private List<JobAdded> AddedJobsList;
        private JobAdded FreightMarketJob;//Program

        public List<LevelNames> PlayerLevelNames;//Program

        private List<City> CitiesList;//+
        private List<string> CitiesListDB;//Program

        private List<Cargo> CargoesList; //+
        private List<Cargo> CargoesListDB;//Program

        private Dictionary<string, List<string>> TrailerDefinitionVariants;//Program
        private List<string> TrailerVariants;//Program

        private Dictionary<string, List<string>> TrailerDefinitionVariantsDB;
        private List<string> TrailerDefinitionListDB;
        private List<string> TrailerVariantsListDB;

        private List<string> HeavyCargoList;

        private List<string> CompaniesList; //+
        private List<string> CompaniesListDB;//Program

        private List<string> CountriesList;//Program

        private List<string> DBDependencies;//Program DB

        public List<Garages> GaragesList; //+
        public List<string> extraVehicles;//process result
        public List<string> extraDrivers;//process result


        private List<CompanyTruck> CompanyTruckList;//Program
        private List<CompanyTruck> CompanyTruckListDB;//Program

        private List<ExtCompany> ExternalCompanies;//Program cache
        private List<ExtCargo> ExtCargoList;//Program cache
        private SqlCeConnection DBconnection;//Program

        private DateTime LastModifiedTimestamp; //+

        internal Save.Items.SiiNunit SiiNunitData;

        internal SaveFileProfileData MainSaveFileProfileData;
        internal SaveFileInfoData MainSaveFileInfoData;

        //
        internal ProgSettings ProgSettingsV;//Program

        private Random RandomValue;//Program

        private CountryDictionary CountryDictionary;//Program
        private Dictionary<string,Country> CountriesDataList;

        private Routes RouteList;//Program DB

        public PlainTXTResourceManager ResourceManagerMain;

        internal Dictionary<string, string> dictionaryProfiles;
        public Dictionary<string, string> CompaniesLngDict, CargoLngDict, TruckBrandsLngDict, CountriesLngDict, UrgencyLngDict, DriverNames;

        public static Dictionary<string, string> CitiesLngDict;//, CustomStringsDict;
        public Dictionary<string, UserCompanyTruckData> UserTruckDictionary; //+

        public Dictionary<string, UserCompanyDriverData> UserDriverDictionary; //+
        private Dictionary<string, UserCompanyTrailerData> UserTrailerDictionary; //+
        private Dictionary<string, Save.Items.Trailer_Def> UserTrailerDefDictionary; //+

        //private List<string> namelessList;//Program
        private string namelessLast;//Program

        private Dictionary<string, List<string>> GPSbehind, GPSahead, GPSAvoid, GPSbehindOnline, GPSaheadOnline; //+

        internal Dictionary<string, double> DistanceMultipliers; //Program
        internal Dictionary<string, double> WeightMultipliers; //Program

        //internal static Bitmap ProgressBarGradient; //Program

        private Image RepairImg, RefuelImg, CustomizeImg; //Program

        internal Image[] MainIcons, ADRImgS, ADRImgSGrey, SkillImgSBG, SkillImgS, GaragesImg, GaragesHQImg, CitiesImg, UrgencyImg, CargoTypeImg, CargoType2Img, 
            TruckPartsImg, TrailerPartsImg, VehicleIntegrityPBImg, GameIconeImg, AccessoriesImg; //Program

        internal Dictionary<string, Image> ProgUIImgsDict;

        private ImageList TabpagesImages; //Program

        private CheckBox[,] SkillButtonArray; //Program
        private CheckBox[] ADRbuttonArray; //Program

        internal double DistanceMultiplier = 1; //Program
        const double km_to_mile = 0.621371; //Program

        internal double WeightMultiplier = 1; //Program
        const double kg_to_lb = 2.20462262185; //Program

        public Dictionary<string, List<string>> CurrencyDictFormat;
        public Dictionary<string, double> CurrencyDictConversion;

        public Dictionary<string, List<string>> CurrencyDictFormatETS2 = new Dictionary<string, List<string>>();
        public Dictionary<string, double> CurrencyDictConversionETS2 = new Dictionary<string, double>();

        public Dictionary<string, List<string>> CurrencyDictFormatATS = new Dictionary<string, List<string>>();
        public Dictionary<string, double> CurrencyDictConversionATS = new Dictionary<string, double>();

        internal Dictionary<string, Dictionary<UInt16, SCS.SCSFontLetter>> GlobalFontMap;
        internal Dictionary<string, byte> LicensePlateWidth;

        internal bool TssetFoldersExist = false;
        internal bool ForseExit = false;

        #endregion

        public FormMain()
        {
            IO_Utilities.LogWriter("Initializing form...");
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            IO_Utilities.LogWriter("Form initialized.");

            //Program
            SteamSelectedTimer.Tick += SteamSelectedTimer_Tick;
            SteamSelectedTimer.Interval = 800;

            UpdateStatusBarMessage.OnNewStatusMessage += UpdateStatusBarMessage_OnNewStatusMessage;
            UpdateStatusBarMessage.OnNewMessageBox += ShowMessageBox_OnNewMessageBox;
            this.Icon = Properties.Resources.MainIco;
            this.Text += " · v." + AssemblyData.AssemblyVersion;

            SetDefaultValues(true);
            IO_Utilities.LogWriter("Loading config...");
            ProgSettingsV.LoadConfigFromFile();
            CheckTssetFoldersExist();
            ApplySettings();
            IO_Utilities.LogWriter("Config loaded.");

            IO_Utilities.LogWriter("Loading resources...");
            LoadExtCountries();
            LoadExtImages();
            IO_Utilities.LogWriter("Resources loaded.");
            AddImagesToControls();

            //Create page controls
            IO_Utilities.LogWriter("Creating form elements...");
            CreateProfilePanelControls();
            CreateCompanyPanelControls();
            Graphics_TSSET.CreateProgressBarBitmap();
            CreateTruckPanelControls();
            CreateTrailerPanelControls();
            groupBoxProfileUserColorsShort.Visible = false;
            tabPageProfile.AutoScroll = true;
            tabPageProfile.Resize += (sender, e) => CenterProfileCards();
            groupBoxProfileSkill.Layout += (sender, e) => CenterProfileCards();
            foreach (Control icon in groupBoxProfileSkill.Controls)
                if (icon is Panel) icon.BackgroundImageLayout = ImageLayout.Zoom;
            CenterProfileCards();
            comboBoxRootFolders.SetBounds(8, 108, 186, 30);
            comboBoxProfiles.SetBounds(8, 144, 186, 30);
            comboBoxSaves.SetBounds(8, 180, 186, 30);
            buttonProfilesAndSavesRefreshAll.SetBounds(200, 108, 30, 30);
            buttonProfilesAndSavesEditProfile.SetBounds(200, 144, 30, 30);
            buttonProfilesAndSavesRestoreBackup.SetBounds(200, 180, 30, 30);
            Action layoutProfiles = () =>
            {
                int right = comboBoxRootFolders.Parent.ClientSize.Width - 8;
                foreach (Control field in new Control[] { comboBoxRootFolders, comboBoxProfiles, comboBoxSaves }) field.Width = Math.Max(60, right - field.Left - 36);
                foreach (Control action in new Control[] { buttonProfilesAndSavesRefreshAll, buttonProfilesAndSavesEditProfile, buttonProfilesAndSavesRestoreBackup }) action.Left = right - action.Width;
            };
            comboBoxRootFolders.Parent.Layout += (sender, e) => layoutProfiles();
            foreach (Control field in new Control[] { comboBoxRootFolders, comboBoxProfiles, comboBoxSaves })
                field.SizeChanged += (sender, e) => layoutProfiles();
            foreach (Control action in new Control[] { buttonProfilesAndSavesRefreshAll, buttonProfilesAndSavesEditProfile, buttonProfilesAndSavesRestoreBackup })
            {
                action.LocationChanged += (sender, e) => layoutProfiles();
                action.SizeChanged += (sender, e) => layoutProfiles();
            }
            layoutProfiles();
            foreach (Control field in new Control[] { comboBoxRootFolders, comboBoxProfiles, comboBoxSaves }) field.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            foreach (Control action in new Control[] { buttonProfilesAndSavesRefreshAll, buttonProfilesAndSavesEditProfile, buttonProfilesAndSavesRestoreBackup }) action.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelFreightMarketDestination.Font = AppTypography.Create(8);
            checkBoxProfilesAndSavesProfileBackups.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkBoxProfilesAndSavesProfileBackups.Top = 84;
            LayoutPlayerLevelCard();
            LayoutCompanyCounter(labelUserCompanyGarages, labelUserCompanyGaragesCurrent, labelUserCompanyGaragesDelimetry, labelUserCompanyGaragesTotal);
            LayoutCompanyCounter(labelUserCompanyDrivers, labelUserCompanyDriversCurrent, labelUserCompanyDriversDivider, labelUserCompanyDriversTotal);
            LayoutCompanyCounter(labelUserCompanyVisitedCities, labelUserCompanyVisitedCitiesCurrent, labelUserCompanyVisitedCitiesDelimetry, labelUserCompanyVisitedCitiesTotal);
            ConfigureModernLayouts();
            IO_Utilities.LogWriter("Done.");

            //Clear elements
            IO_Utilities.LogWriter("Prepare form...");
            ClearFormControls(true);

            ToggleControlsAccess(false);
            IO_Utilities.LogWriter("Done.");

            //Language
            IO_Utilities.LogWriter("Loading translation...");
            GetTranslationFiles();
            ChangeLanguage();
            IO_Utilities.LogWriter("Done.");

            //Non program task
            IO_Utilities.LogWriter("Caching game data...");
            CacheGameData();
            IO_Utilities.LogWriter("Caching finished.");
        }

        private async void FormMain_Shown(object sender, EventArgs e)
        {
            IO_Utilities.LogWriter("Opening form...");
            bool showSplash = true;
            try { showSplash = !TssetFoldersExist || (!ProgSettingsV.HideStartupPopup && Properties.Settings.Default.ShowSplashOnStartup); }
            catch { IO_Utilities.LogWriter("Startup settings could not be read; opening recovery screen."); }
            if (showSplash)
                using (var splash = new FormSplash()) splash.ShowDialog(this);
            DetectGame();
            if (!showSplash && Properties.Settings.Default.CheckUpdatesOnStartup &&
                Web_Utilities.External.CheckNewVersionTimeElapsed(ProgSettingsV.LastUpdateCheck))
            {
                var version = await System.Threading.Tasks.Task.Run(() => Web_Utilities.External.CheckNewVersionAvailability());
                if (IsDisposed || version == null) return;
                ProgSettingsV.LastUpdateCheck = DateTime.Now;
                if (Web_Utilities.External.CheckNewVersionStatus(version) == true)
                    toolStripStatusMessages.Text = "Update " + version[0] + " available. Open Help > Check updates.";
            }
        }

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == SingleInstance.ActivateMessage)
            {
                SingleInstance.Activate(Handle);
                return;
            }
            base.WndProc(ref message);
        }

        private void CenterProfileCards()
        {
            int width = 420, height = 435;
            foreach (Control child in groupBoxProfileSkill.Controls)
            {
                width = Math.Max(width, child.Right + 12);
                height = Math.Max(height, child.Bottom + 12);
            }
            groupBoxProfileSkill.Size = new Size(width, height);
            groupBoxProfilePlayerLevel.Width = width;
            groupBoxProfileSkill.Top = groupBoxProfilePlayerLevel.Bottom + 14;
            int left = Math.Max(12, (tabPageProfile.ClientSize.Width - width) / 2);
            groupBoxProfilePlayerLevel.Left = groupBoxProfileSkill.Left = left;
        }

        private void AlignFieldCaption(Label caption, Control field)
        {
            Action align = () => caption.Top = Math.Max(0, field.Top - caption.PreferredSize.Height - 2);
            field.Parent.Layout += (sender, e) => align();
            align();
        }

        private void LayoutCompanyCounter(Label title, Label current, Label separator, Label total)
        {
            Control panel = current.Parent;
            foreach (Label label in new[] { current, separator, total })
            {
                label.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                label.AutoSize = false;
                label.Height = 24;
            }
            Action align = () =>
            {
                int right = panel.ClientSize.Width - 12, top = Math.Max(0, (panel.ClientSize.Height - 24) / 2);
                total.SetBounds(right - 56, top, 56, 24);
                separator.SetBounds(right - 74, top, 18, 24);
                current.SetBounds(right - 130, top, 56, 24);
                title.Top = Math.Max(0, (panel.ClientSize.Height - title.PreferredSize.Height) / 2);
            };
            panel.Resize += (sender, e) => align();
            align();
        }

        private void LayoutPlayerLevelCard()
        {
            groupBoxProfilePlayerLevel.Height = 168;
            groupBoxProfileSkill.Top = 186;
            buttonPlayerLevelMinus10.SetBounds(12, 8, 42, 30);
            buttonPlayerLevelMinus01.SetBounds(60, 8, 42, 30);
            buttonPlayerLevelPlus01.SetBounds(316, 8, 42, 30);
            buttonPlayerLevelPlus10.SetBounds(364, 8, 42, 30);
            var experienceRow = new FlowLayoutPanel { Name = "experienceRow", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false };
            foreach (Label label in new[] { labelPlayerExperience, labelPlayerExperienceSpacer, labelPlayerExperienceNxtLvlThreshhold })
            {
                label.AutoSize = true;
                label.RightToLeft = RightToLeft.No;
                label.Margin = new Padding(4, 0, 4, 0);
                experienceRow.Controls.Add(label);
            }
            panelPlayerLevel.Controls.Add(experienceRow);
            bool fittingLevelLabels = false;
            Action fitLevelLabels = () =>
            {
                if (fittingLevelLabels) return;
                fittingLevelLabels = true;
                try
                {
                    int left = buttonPlayerLevelMinus01.Right + 8;
                    int width = Math.Max(1, buttonPlayerLevelPlus01.Left - left - 8);
                    labelPlayerLevelNumber.Left = labelPlayerLevelName.Left = left;
                    labelPlayerLevelNumber.Width = labelPlayerLevelName.Width = width;
                    labelPlayerLevelNumber.Height = Math.Max(1, labelPlayerLevelName.Top - labelPlayerLevelNumber.Top - 4);
                    experienceRow.Top = labelPlayerLevelName.Bottom + 4;
                    experienceRow.Left = Math.Max(0, left + (width - experienceRow.PreferredSize.Width) / 2);
                }
                finally { fittingLevelLabels = false; }
            };
            panelPlayerLevel.Layout += (sender, e) => fitLevelLabels();
            experienceRow.SizeChanged += (sender, e) => fitLevelLabels();
            foreach (Control control in new Control[] { labelPlayerLevelNumber, labelPlayerLevelName, buttonPlayerLevelPlus01, buttonPlayerLevelMinus01 })
            {
                control.LocationChanged += (sender, e) => fitLevelLabels();
                control.SizeChanged += (sender, e) => fitLevelLabels();
            }
            labelPlayerLevelNumber.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            labelPlayerLevelNumber.SetBounds(112, 2, 198, 36);
            labelPlayerLevelName.SetBounds(112, 38, 198, 24);
            buttonPlayerLevelMinimum.SetBounds(12, 48, 90, 32);
            buttonPlayerLevelSet.SetBounds(316, 48, 94, 32);
            buttonPlayerUnlockSkills.SetBounds(130, 100, 160, 32);
            foreach (Control control in groupBoxProfileSkill.Controls)
                if (control.Name.StartsWith("labelProfileSkill")) control.Top -= 6;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ForseExit) return;
            bool unsaved = AddedJobsDictionary != null && AddedJobsDictionary.Count > 0;
            if (unsaved || ProgSettingsV.AskBeforeClosing)
            {
                using (var dialog = CreateCloseConfirmation(unsaved))
                {
                    if (!AcceptCloseChoice(dialog, dialog.ShowDialog(this)))
                    {
                        e.Cancel = true;
                        Activate();
                        return;
                    }
                }
            }
            ProgSettingsV.WriteConfigToFile();
        }

        internal MaterialMessageDialog CreateCloseConfirmation(bool unsaved)
        {
            var dialog = new MaterialMessageDialog(
                unsaved ? "You have unsaved changes.\r\nDo you really want to close down application?" : "Do you really want to close down application?",
                unsaved ? "Close Application without saving changes" : "Close Application",
                MessageBoxButtons.YesNo, unsaved ? MessageBoxIcon.Exclamation : MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (!unsaved)
            {
                var body = dialog.Controls.Find("messageBody", true)[0];
                var option = new CheckBox { Name = "dontAskAgain", Text = "Don't ask again", AutoSize = true, UseMnemonic = false, Left = 24, Top = dialog.MessageLabel.Bottom + 12 };
                body.Controls.Add(option);
                body.Layout += (sender, args) => option.Top = dialog.MessageLabel.Bottom + 12;
                dialog.ClientSize = new Size(dialog.ClientSize.Width, dialog.ClientSize.Height + 44);
            }
            return dialog;
        }

        internal bool AcceptCloseChoice(MaterialMessageDialog dialog, DialogResult result)
        {
            if (result != DialogResult.Yes) return false;
            var option = dialog.Controls.Find("dontAskAgain", true).FirstOrDefault() as CheckBox;
            if (option != null && option.Checked) ProgSettingsV.AskBeforeClosing = false;
            return true;
        }

        private void makeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Set default culture
            string sysCI = CultureInfo.InstalledUICulture.Name;

            string folderPath = Directory.GetCurrentDirectory() + @"\lang\" + sysCI;

            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            Process.Start(folderPath);

            //Copy default files

        }
    }

    public class Globals
    {
        //-----
        public static string[] ProfilesPaths = new string[0];
        public static List<string> ProfilesHex = new List<string>();
        //
        public static string SelectedProfile = "";
        public static string SelectedProfilePath = "";
        public static string SelectedProfileName = "";
        //----
        public static string[] SavesHex = new string[0];
        //
        public static string SelectedSave = "";
        public static string SelectedSavePath = "";
        public static string SelectedSaveName = "";
        //----
        public static int[] PlayerLevelUps = new int[0];
        public static string CurrencyName = "";
    }

}
