using System;
using System.Drawing;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TS_SE_Tool
{
    public partial class FormMain
    {
        private readonly MaterialButton refreshSteamCloud = new MaterialButton { Name = "refreshSteamCloud", Text = "Refresh", Dock = DockStyle.Fill, MinimumSize = new Size(0, 36) };
        private readonly TableLayoutPanel steamCloudActions = new TableLayoutPanel { Name = "steamCloudActions", Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 2, Visible = false };

        private void ConfigureModernLayouts()
        {
            buttonMainCloseSave.Dock = DockStyle.Fill;
            buttonMainCloseSave.Margin = buttonMainLoadSave.Margin;
            tableLayoutPanel16.RowStyles[3] = new RowStyle(SizeType.Absolute, 44);
            float[] garageWidths = { 32, 18, 16, 14, 20 };
            for (int i = 0; i < garageWidths.Length; i++) tableLayoutPanel6.ColumnStyles[i] = new ColumnStyle(SizeType.Percent, garageWidths[i]);
            toolStripStatusMessages.AutoSize = true;
            toolStripStatusMessages.Spring = false;
            toolStripStatusMessages.Margin = new Padding(0, 0, 12, 0);
            toolStripProgressBarMain.Alignment = ToolStripItemAlignment.Left;
            toolStripProgressBarMain.Margin = new Padding(0, 3, 0, 3);
            ConfigureSteamCloudActions();
            tableLayoutPanel15.ColumnStyles[1].Width = 288;
            foreach (MaterialComboBox list in new[] { comboBoxRootFolders, comboBoxProfiles, comboBoxSaves })
            {
                list.MinimumItemHeight = list.ItemHeight = 26;
                list.Font = AppTypography.Create(10);
                list.IntegralHeight = false;
                list.DropDownHeight = 240;
                list.DropDownWidth = 380;
            }
            foreach (ListBox list in new[] { listBoxGarages, listBoxUserCompanyDrivers, listBoxVisitedCities }) list.Font = AppTypography.Create(10);
            LayoutCargoFields();
            LayoutFreightFields();
            LayoutVehicleParts(tableLayoutPanelTruckDetails, "Truck", TruckPartsImg);
            LayoutVehicleParts(tableLayoutPanelTrailerDetails, "Trailer", TrailerPartsImg);
            tableLayoutPanel9.RowStyles[0] = new RowStyle(SizeType.Absolute, 126);
            tableLayoutPanel12.RowStyles[0] = new RowStyle(SizeType.Absolute, 126);
            tableLayoutPanel8.RowStyles[0] = new RowStyle(SizeType.Absolute, 36);
            tableLayoutPanel13.RowStyles[0] = new RowStyle(SizeType.Absolute, 36);
            tableLayoutPanel8.ColumnStyles[0] = new ColumnStyle(SizeType.AutoSize);
            buttonUserTruckSelectCurrent.AutoSize = true;
            buttonUserTruckSelectCurrent.MinimumSize = new Size(190, 40);
            tableLayoutPanel13.ColumnStyles[0] = new ColumnStyle(SizeType.AutoSize);
            buttonUserTrailerSelectCurrent.AutoSize = true;
            buttonUserTrailerSelectCurrent.MinimumSize = new Size(190, 40);

            tableLayoutPanelCompanyMain.RowStyles[0].Height = 132;
            tableLayoutPanelCompanyDataTopRow.ColumnStyles[0].Width = 132;
            pictureBoxCompanyLogo.MinimumSize = pictureBoxCompanyLogo.MaximumSize = Size.Empty;
            pictureBoxCompanyLogo.Dock = DockStyle.Fill;
            pictureBoxCompanyLogo.Margin = new Padding(10);
            pictureBoxCompanyLogo.BorderStyle = BorderStyle.None;
            pictureBoxCompanyLogo.BackColor = MaterialYou.Container;
            pictureBoxCompanyLogo.Paint += (sender, e) =>
            {
                if (pictureBoxCompanyLogo.Image != null) return;
                int side = Math.Min(56, Math.Min(pictureBoxCompanyLogo.Width, pictureBoxCompanyLogo.Height) - 16);
                if (side > 0) MaterialIcons.Draw(e.Graphics,
                    new Rectangle((pictureBoxCompanyLogo.Width - side) / 2, (pictureBoxCompanyLogo.Height - side) / 2, side, side),
                    "Company", MaterialYou.OnContainer);
            };
        }

        private void ConfigureSteamCloudActions()
        {
            tableLayoutPanel16.Controls.Remove(labelHelpText);
            steamCloudActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            steamCloudActions.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            steamCloudActions.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            steamCloudActions.Controls.Add(refreshSteamCloud, 0, 0);
            steamCloudActions.Controls.Add(labelHelpText, 0, 1);
            tableLayoutPanel16.Controls.Add(steamCloudActions, 0, 4);
            labelHelpText.AutoSize = false;
            labelHelpText.Dock = DockStyle.Fill;
            labelHelpText.Font = AppTypography.Create(11, FontStyle.Bold);
            labelHelpText.Cursor = Cursors.Hand;
            labelHelpText.Click += (sender, e) =>
            {
                using (var help = CreateSteamCloudHelp()) help.ShowDialog(this);
            };
            refreshSteamCloud.Click += (sender, e) => RefreshSteamCloudProfile();
        }

        internal MaterialMessageDialog CreateSteamCloudHelp()
        {
            return new MaterialMessageDialog(
                "1. Launch the game.\r\n2. Press Career Profile in the top-right corner.\r\n3. Press Edit Career.\r\n4. Disable Use Steam Cloud and confirm.\r\n5. Return here and press Refresh.\r\n\r\nIf your offline profile is not listed yet, save the game first, then refresh again.",
                "Disable Steam Cloud", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        internal void SetSteamCloudWarning(bool enabled)
        {
            steamCloudActions.Visible = enabled;
            labelHelpText.Visible = enabled;
            if (!enabled) { SteamSelectedTimer.Stop(); return; }
            labelHelpText.ForeColor = MaterialYou.OnContainer;
        }

        private void RefreshSteamCloudProfile()
        {
            string root = Convert.ToString(comboBoxRootFolders.SelectedValue);
            string profile = Convert.ToString(comboBoxProfiles.SelectedValue);
            string save = Path.GetFileName(Convert.ToString(comboBoxSaves.SelectedValue));
            FillRootFoldersPaths();
            var table = comboBoxRootFolders.DataSource as DataTable;
            if (table == null) return;
            var roots = table.Rows.Cast<DataRow>()
                .Select(row => new KeyValuePair<string, string>((string)row["ProfileID"], (string)row["ProfileType"]));
            string offline = Utilities.ProfilePaths.FindOfflineCopy(roots, profile);
            string nextRoot = offline == null ? root : Path.GetDirectoryName(offline);
            if (FindByValue(comboBoxRootFolders, nextRoot) >= 0) comboBoxRootFolders.SelectedValue = nextRoot;
            string nextProfile = offline ?? profile;
            if (FindByValue(comboBoxProfiles, nextProfile) >= 0) comboBoxProfiles.SelectedValue = nextProfile;
            if (!string.IsNullOrEmpty(save))
            {
                string nextSave = Path.Combine(nextProfile, "save", save);
                if (FindByValue(comboBoxSaves, nextSave) >= 0) comboBoxSaves.SelectedValue = nextSave;
            }
            CheckSaveControls();
            if (offline == null)
                using (var help = new MaterialMessageDialog("The matching offline profile is not available yet. Confirm the change in the game, save, then press Refresh again.",
                    "Steam Cloud profile", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)) help.ShowDialog(this);
        }

        internal static Image LoadCompanyLogo(string logo, string game)
        {
            string name = Path.GetFileNameWithoutExtension((logo ?? "").Trim().Trim('"').Replace('\\', '/'));
            if (string.IsNullOrEmpty(name)) return null;
            var result = Utilities.Graphics_TSSET.ddsImgLoader(Path.Combine("img", game, "player_logo", name + ".dds"));
            if (result.validity[0]) return result.images[0];
            result.images[0].Dispose();
            return null;
        }

        private void LayoutCargoFields()
        {
            var grid = new TableLayoutPanel { Name = "cargoFields", Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3, RowCount = 6, Padding = new Padding(8) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 6; i++) grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Action<Control, int, int> add = (control, column, row) =>
            {
                control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                control.Margin = new Padding(6, 6, 6, 6);
                if (control is Label label) label.AutoSize = true;
                if (control is Button button) { button.AutoSize = true; button.MinimumSize = new Size(0, 34); }
                grid.Controls.Add(control, column, row);
            };
            add(labelCargoMarketCity, 1, 0); add(labelCargoMarketCompany, 2, 0);
            add(labelCargoMarketSource, 0, 1);
            add(comboBoxCargoMarketSourceCity, 1, 1); add(comboBoxCargoMarketSourceCompany, 2, 1);
            add(buttonCargoMarketResetCargoCity, 1, 2); add(buttonCargoMarketResetCargoCompany, 2, 2);
            add(buttonCargoMarketRandomizeCargoCity, 1, 3); add(buttonCargoMarketRandomizeCargoCompany, 2, 3);
            add(listBoxCargoMarketSourceCargoSeeds, 1, 4); add(listBoxCargoMarketCargoListForCompany, 2, 4);
            add(labelCMTrailerType, 0, 5); add(comboBoxCMTrailerTypes, 1, 5);
            tabPageCargoMarket.Controls.Add(grid);
            grid.BringToFront();
        }

        private void LayoutVehicleParts(TableLayoutPanel details, string vehicle, Image[] images)
        {
            for (int i = 0; i < images.Length; i++)
            {
                var icon = Controls.Find(vehicle + "PartImg" + i, true).Single();
                var row = (TableLayoutPanel)icon.Parent;
                row.ColumnStyles[0].Width = 72;
                row.ColumnStyles[2].Width = 48;
                row.RowStyles[0] = new RowStyle(SizeType.Absolute, 28);
                var fittedIcon = CropPartIcon(images[i]);
                icon.BackgroundImage = fittedIcon;
                icon.Disposed += (sender, e) => fittedIcon.Dispose();
                icon.BackgroundImageLayout = ImageLayout.Zoom;
                icon.Dock = DockStyle.Fill;
                icon.Margin = new Padding(6);
                details.RowStyles[i] = new RowStyle(SizeType.Percent, 100f / images.Length);
            }
            for (int i = images.Length; i < details.RowCount; i++)
                details.RowStyles[i] = new RowStyle(SizeType.Absolute, i == details.RowCount - 1 ? 36 : 0);
            details.AutoScroll = false;
        }

        private static Bitmap CropPartIcon(Image image)
        {
            using (var bitmap = new Bitmap(image))
            {
                int left = bitmap.Width, top = bitmap.Height, right = -1, bottom = -1;
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                        if (bitmap.GetPixel(x, y).A > 8)
                        {
                            left = Math.Min(left, x); top = Math.Min(top, y);
                            right = Math.Max(right, x); bottom = Math.Max(bottom, y);
                        }
                if (right < left) return new Bitmap(bitmap);
                left = Math.Max(0, left - 2); top = Math.Max(0, top - 2);
                right = Math.Min(bitmap.Width - 1, right + 2); bottom = Math.Min(bitmap.Height - 1, bottom + 2);
                return bitmap.Clone(Rectangle.FromLTRB(left, top, right + 1, bottom + 1), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
        }

        private void LayoutFreightFields()
        {
            var fields = new TableLayoutPanel
            {
                Name = "freightFields", Dock = DockStyle.Fill, AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 5, RowCount = 10,
                Padding = new Padding(8), Margin = Padding.Empty
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            for (int i = 0; i < fields.RowCount; i++) fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Action<Control, int, int> add = (control, column, row) =>
            {
                control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                control.Dock = DockStyle.None;
                control.Margin = new Padding(6, 5, 6, 5);
                if (control is Label label) { label.AutoSize = true; label.MaximumSize = Size.Empty; }
                if (control is Button button) { button.MinimumSize = new Size(0, 36); button.AutoSize = true; }
                fields.Controls.Add(control, column, row);
            };
            add(labelFreightMarketCountryF, 1, 0);
            add(labelFreightMarketCompanyF, 3, 0);
            add(labelFreightMarketFilterMain, 0, 1);
            add(comboBoxFreightMarketCountries, 1, 1);
            add(comboBoxFreightMarketCompanies, 3, 1);
            add(checkBoxFreightMarketRandomDest, 4, 1);
            add(labelFreightMarketCity, 1, 2);
            add(labelFreightMarketCompany, 3, 2);
            add(labelFreightMarketSource, 0, 3);
            add(comboBoxFreightMarketSourceCity, 1, 3);
            add(comboBoxFreightMarketSourceCompany, 3, 3);
            add(checkBoxFreightMarketFilterSource, 4, 3);
            add(labelFreightMarketDestination, 0, 4);
            add(comboBoxFreightMarketDestinationCity, 1, 4);
            add(comboBoxFreightMarketDestinationCompany, 3, 4);
            add(checkBoxFreightMarketFilterDestination, 4, 4);
            add(labelFreightMarketCargo, 0, 5);
            add(comboBoxFreightMarketCargoList, 1, 5);
            add(labelFreightMarketUrgency, 2, 5);
            add(comboBoxFreightMarketUrgency, 3, 5);
            add(labelFreightMarketTrailer, 0, 6);
            add(comboBoxFreightMarketTrailerDef, 1, 6);
            add(comboBoxFreightMarketTrailerVariant, 3, 6);
            add(buttonFreightMarketAddJob, 0, 7);
            fields.SetColumnSpan(buttonFreightMarketAddJob, 4);
            add(buttonFreightMarketRandomizeCargo, 4, 7);
            add(labelFreightMarketDistance, 0, 8);
            add(labelFreightMarketDistanceNumbers, 1, 8);
            add(buttonFreightMarketClearJobList, 4, 8);
            add(buttonFreightMarketCancelJobEdit, 0, 9);
            fields.SetColumnSpan(buttonFreightMarketCancelJobEdit, 4);

            var layout = new TableLayoutPanel
            {
                Name = "freightLayout", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
                Margin = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(fields, 0, 0);
            layout.Controls.Add(listBoxFreightMarketAddedJobs, 0, 1);
            listBoxFreightMarketAddedJobs.Dock = DockStyle.Fill;
            tabPageFreightMarket.Controls.Add(layout);
            layout.BringToFront();
        }
    }
}
