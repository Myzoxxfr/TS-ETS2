using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace TS_SE_Tool.Utilities
{
    internal static class ProfilePaths
    {
        internal static IEnumerable<string> SteamLocations()
        {
            var paths = new List<string>();
            foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
                foreach (var view in new[] { RegistryView.Registry32, RegistryView.Registry64 })
                    try
                    {
                        using (var registry = RegistryKey.OpenBaseKey(hive, view))
                        using (var steam = registry.OpenSubKey(@"Software\Valve\Steam"))
                        {
                            var path = steam?.GetValue(hive == RegistryHive.CurrentUser ? "SteamPath" : "InstallPath") as string;
                            if (!string.IsNullOrWhiteSpace(path)) paths.Add(path);
                        }
                    }
                    catch (SecurityException ex) { IO_Utilities.LogWriter("Steam registry: " + ex.Message); }
                    catch (UnauthorizedAccessException ex) { IO_Utilities.LogWriter("Steam registry: " + ex.Message); }
            paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"));
            return paths.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static string[] Directories(string path)
        {
            try { return Directory.Exists(path) ? Directory.GetDirectories(path) : new string[0]; }
            catch (IOException ex) { IO_Utilities.LogWriter("Profile folders: " + ex.Message); }
            catch (UnauthorizedAccessException ex) { IO_Utilities.LogWriter("Profile folders: " + ex.Message); }
            catch (SecurityException ex) { IO_Utilities.LogWriter("Profile folders: " + ex.Message); }
            return new string[0];
        }

        internal static List<KeyValuePair<string, string>> FindRoots(string documents, IEnumerable<string> steamLocations, string gameType, bool backups)
        {
            var homes = new List<KeyValuePair<string, string>>();
            homes.Add(new KeyValuePair<string, string>(Path.Combine(documents,
                gameType == "ETS2" ? "Euro Truck Simulator 2" : "American Truck Simulator"), "local"));
            foreach (string steam in steamLocations.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase))
                foreach (string account in Directories(Path.Combine(steam, "userdata")))
                    if (Path.GetFileName(account).All(char.IsDigit))
                        homes.Add(new KeyValuePair<string, string>(Path.Combine(account,
                            gameType == "ETS2" ? "227300" : "270880", "remote"), "steam"));

            var result = new List<KeyValuePair<string, string>>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var home in homes)
                foreach (string path in Directories(home.Key))
                {
                    string name = Path.GetFileName(path);
                    bool standard = name.Equals("profiles", StringComparison.OrdinalIgnoreCase);
                    bool backup = name.StartsWith("profiles(", StringComparison.OrdinalIgnoreCase);
                    if ((standard || (backups && backup)) && Directories(path).Length > 0 && seen.Add(path))
                        result.Add(new KeyValuePair<string, string>(path, home.Value));
                }
            return result;
        }

        internal static string FindOfflineCopy(IEnumerable<KeyValuePair<string, string>> roots, string cloudProfile)
        {
            if (string.IsNullOrWhiteSpace(cloudProfile)) return null;
            string folder = Path.GetFileName(cloudProfile.TrimEnd(Path.DirectorySeparatorChar));
            foreach (var root in roots)
            {
                if (root.Value == "steam" || !Path.GetFileName(root.Key).Equals("profiles", StringComparison.OrdinalIgnoreCase)) continue;
                string candidate = Path.Combine(root.Key, folder);
                try
                {
                    string profileFile = Path.Combine(candidate, "profile.sii");
                    string cloudFile = Path.Combine(cloudProfile, "profile.sii");
                    if (File.Exists(profileFile) && Directory.Exists(Path.Combine(candidate, "save")) &&
                        (!File.Exists(cloudFile) || File.GetLastWriteTimeUtc(profileFile) >= File.GetLastWriteTimeUtc(cloudFile)))
                        return candidate;
                }
                catch (IOException ex) { IO_Utilities.LogWriter("Offline profile: " + ex.Message); }
                catch (UnauthorizedAccessException ex) { IO_Utilities.LogWriter("Offline profile: " + ex.Message); }
            }
            return null;
        }

        internal static Dictionary<string, string> ParseSteamNames(string text)
        {
            var names = new Dictionary<string, string>();
            var tokens = Regex.Matches(text, @"//[^\r\n]*|""(?:\\.|[^""\\])*""|[{}]")
                .Cast<Match>().Select(m => m.Value).Where(t => !t.StartsWith("//")).ToArray();
            int depth = 0;
            string account = null;
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (token == "{") { depth++; continue; }
                if (token == "}") { if (depth == 2) account = null; depth--; continue; }
                if (depth == 1 && i + 1 < tokens.Length && tokens[i + 1] == "{" &&
                    ulong.TryParse(token.Trim('"'), NumberStyles.None, CultureInfo.InvariantCulture, out var steamId))
                    account = (steamId & uint.MaxValue).ToString(CultureInfo.InvariantCulture);
                if (depth == 2 && account != null && token.Equals("\"PersonaName\"", StringComparison.OrdinalIgnoreCase) &&
                    i + 1 < tokens.Length && tokens[i + 1].StartsWith("\""))
                {
                    string value = tokens[++i];
                    string name = Regex.Replace(value.Substring(1, value.Length - 2), @"\\([\\""])", "$1").Replace('\r', ' ').Replace('\n', ' ').Trim();
                    if (name.Length > 0) names[account] = name;
                }
            }
            return names;
        }

        internal static string SteamAccountName(string profileRoot)
        {
            string accountPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(profileRoot)));
            string account = Path.GetFileName(accountPath);
            string steam = Path.GetDirectoryName(Path.GetDirectoryName(accountPath));
            string users = Path.Combine(steam, "config", "loginusers.vdf");
            try
            {
                if (File.Exists(users) && new FileInfo(users).Length <= 1024 * 1024 &&
                    ParseSteamNames(File.ReadAllText(users)).TryGetValue(account, out var name)) return name;
            }
            catch (IOException ex) { IO_Utilities.LogWriter("Steam display names: " + ex.Message); }
            catch (UnauthorizedAccessException ex) { IO_Utilities.LogWriter("Steam display names: " + ex.Message); }
            catch (SecurityException ex) { IO_Utilities.LogWriter("Steam display names: " + ex.Message); }
            return account;
        }
    }
}
