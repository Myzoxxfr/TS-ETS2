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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Timers;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Net.Mime;
using System.Security.Cryptography;
using TS_SE_Tool.Utilities;
using System.Security.Policy;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace TS_SE_Tool.Utilities
{
    public class Web_Utilities
    {
        internal static Web_Utilities External = new Web_Utilities();

        private const string RepositoryUrl = "https://github.com/Myzoxxfr/TS-ETS2";
        internal string linkCheckVersion = RepositoryUrl + "/releases/latest/download/fileData.txt";
        internal string linkYoutubeTutorial = "https://rebrand.ly/TS-SET-Tutorial";

        internal string linkHelpDeveloper = "https://www.paypal.me/LIPtoHCode";

        internal string linkSCSforum = "https://forum.scssoft.com/viewtopic.php?f=34&t=266092";
        internal string linkTMPforum = "https://forum.truckersmp.com/index.php?/topic/79561-ts-saveeditor-tool";
        internal string linkGithub = RepositoryUrl;

        internal string linkGithubReleases = RepositoryUrl + "/releases";
        internal string linkGithubReleasesLatest = RepositoryUrl + "/releases/latest";

        //Check for new version
        internal bool CheckNewVersionTimeElapsed(DateTime _lastTime)
        {
            TimeSpan timeDifference = DateTime.Now.Subtract(_lastTime);

            if (timeDifference.TotalHours > 24)
                return true;
            else
                return false;
        }

        internal string[] CheckNewVersionAvailability()
        {
            return CheckLatestRelease(DownloadText);
        }

        internal string[] CheckLatestRelease(Func<string, string> download)
        {
            var metadata = ParseUpdateData(download(linkCheckVersion));
            if (metadata != null) return metadata;
            return ParseReleaseVersion(download("https://api.github.com/repos/Myzoxxfr/TS-ETS2/releases/latest"));
        }

        [DataContract]
        private class ReleaseVersion
        {
            [DataMember(Name = "tag_name")] public string Tag { get; set; }
            [DataMember(Name = "draft")] public bool Draft { get; set; }
            [DataMember(Name = "prerelease")] public bool Prerelease { get; set; }
        }

        internal static string[] ParseReleaseVersion(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            try
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                {
                    var release = (ReleaseVersion)new DataContractJsonSerializer(typeof(ReleaseVersion)).ReadObject(stream);
                    string tag = release?.Tag;
                    if (string.IsNullOrEmpty(tag) || release.Draft || release.Prerelease) return null;
                    if (tag.StartsWith("v", StringComparison.OrdinalIgnoreCase)) tag = tag.Substring(1).TrimStart('.');
                    if (!Version.TryParse(tag, out var version) || version.Revision < 0 || version.ToString() != tag) return null;
                    // Without SHA-512 metadata, offer the release page instead of installing an unverified ZIP.
                    return new[] { tag, "" };
                }
            }
            catch (SerializationException) { return null; }
        }

        private string DownloadText(string url)
        {
            using (WebClientWithTO client = new WebClientWithTO())
            {
                client.Timeout_S = 10;
                try
                {
                    return client.DownloadString(url);
                }
                catch (Exception ex)
                {
                    IO_Utilities.LogWriter("Could not check fork releases: " + ex.Message);
                    return null;
                }
            }
        }

        internal bool? CheckNewVersionStatus(string[] _NewVersion)
        {
            if (_NewVersion == null || _NewVersion.Length != 2 || !Version.TryParse(_NewVersion[0], out var version))
                return null;
            int comparison = version.CompareTo(Version.Parse(AssemblyData.AssemblyVersion));
            if (comparison > 0) return true;
            if (comparison == 0) return false;
            return null;
        }

        internal static string[] ParseUpdateData(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var fields = text.Trim().TrimStart('\uFEFF').Split('\t');
            if (fields.Length != 2) return null;
            string filename = fields[0].Trim();
            const string prefix = "TS-ETS2.";
            if (filename.Length <= prefix.Length + 4 || !filename.StartsWith(prefix, StringComparison.Ordinal) || !filename.EndsWith(".zip", StringComparison.Ordinal))
                return null;
            string versionText = filename.Substring(prefix.Length, filename.Length - prefix.Length - 4);
            if (!Version.TryParse(versionText, out var version) || version.Revision < 0 || version.ToString() != versionText)
                return null;
            string hash = fields[1].Trim();
            if (hash.Length != 128 || !hash.All(Uri.IsHexDigit)) return null;
            return new[] { versionText, hash.ToUpperInvariant() };
        }

        internal string GetDownloadUrl(string versionText)
        {
            if (!Version.TryParse(versionText, out var version) || version.Revision < 0 || version.ToString() != versionText)
                throw new FormatException("Invalid update version.");
            return linkGithubReleasesLatest + "/download/TS-ETS2." + versionText + ".zip";
        }
        //
        internal class WebClientWithTO : WebClient
        {
            public UInt16 Timeout_S { get; set; } = 60;

            public WebClientWithTO()
            {
                Headers[HttpRequestHeader.UserAgent] = "TS-ETS2";
                Headers[HttpRequestHeader.CacheControl] = "no-cache";
            }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest lWebRequest = base.GetWebRequest(uri);
                lWebRequest.Timeout = Timeout_S * 1000;
                ((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout_S * 1000;

                return lWebRequest;
            }
        }

        internal bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                request.Timeout = 5000;
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }
    }
}
