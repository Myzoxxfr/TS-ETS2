using System;
using System.Linq;
using System.IO;
using System.Net;
using TS_SE_Tool.Utilities;

namespace TS_SE_Tool.Utilities
{
    static class AssemblyData { internal static string AssemblyVersion = "0.3.12.0"; }
    static class IO_Utilities { internal static void LogWriter(string message) { } }
}

class CheckUpdates
{
    class Client : Web_Utilities.WebClientWithTO
    {
        internal HttpWebRequest InspectRequest() => (HttpWebRequest)GetWebRequest(new Uri("https://github.com/Myzoxxfr/TS-ETS2"));
    }

    static int Main(string[] args)
    {
        try
        {
            string hash = new string('a', 128);
            var parsed = Web_Utilities.ParseUpdateData("\uFEFFTS-ETS2.0.3.13.0.zip\t" + hash + "\r\n");
            if (parsed == null || parsed[0] != "0.3.13.0" || parsed[1] != hash.ToUpperInvariant())
                throw new Exception("Valid metadata was not parsed.");
            foreach (string bad in new[] { null, "", " ", "<html>not a release</html>", "TS-ETS2.zip\t" + hash,
                "TS-ETS2.0.3.13.zip\t" + hash, "TS-ETS2.0.3.13.0.zip", "../../TS-ETS2.0.3.13.0.zip\t" + hash,
                "TS-ETS2.0.3.13.0.zip\t1234", "TS-ETS2.0.3.13.0.zip\t" + new string('g', 128),
                "TS-ETS2.0.3.13.0.zip\t" + hash + "\textra", "TS-ETS2.0.3.13.0.exe\t" + hash,
                "TS.SE.Tool.0.3.13.0.zip\t" + hash })
                if (Web_Utilities.ParseUpdateData(bad) != null) throw new Exception("Invalid metadata was accepted: " + bad);

            var updates = new Web_Utilities();
            if (updates.linkCheckVersion != "https://github.com/Myzoxxfr/TS-ETS2/releases/latest/download/fileData.txt"
                || updates.GetDownloadUrl(parsed[0]) != "https://github.com/Myzoxxfr/TS-ETS2/releases/latest/download/TS-ETS2.0.3.13.0.zip")
                throw new Exception("Update URLs do not point to the fork's assets.");
            if (updates.CheckNewVersionStatus(new[] { "0.3.13.0", hash }) != true
                || updates.CheckNewVersionStatus(new[] { "0.3.300.0", hash }) != true
                || updates.CheckNewVersionStatus(new[] { "0.3.12.0", hash }) != false
                || updates.CheckNewVersionStatus(new[] { "0.3.11.999", hash }) != null
                || updates.CheckNewVersionStatus(new[] { "0.2.999.0", hash }) != null)
                throw new Exception("Version ordering is incorrect.");
            foreach (string bad in new[] { null, "../0.3.13.0", "0.3.13.0.zip", "0.3.13" })
            {
                bool rejected = false;
                try { updates.GetDownloadUrl(bad); } catch (FormatException) { rejected = true; }
                if (!rejected) throw new Exception("An unsafe version reached the download URL.");
            }
            using (var client = new Client { Timeout_S = 10 })
            {
                var request = client.InspectRequest();
                if (request.Timeout != 10000 || request.ReadWriteTimeout != 10000
                    || request.UserAgent != "TS-ETS2")
                    throw new Exception("HTTP settings: timeout=" + request.Timeout + ", read/write=" + request.ReadWriteTimeout
                        + ", client UA=" + client.Headers[HttpRequestHeader.UserAgent] + ", request UA=" + request.UserAgent);
            }
            if (args.Length > 0 && Web_Utilities.ParseUpdateData(File.ReadAllText(args[0])) == null)
                throw new Exception("The generated fileData.txt is incompatible with the updater.");
            Console.WriteLine("PASS: fork-only URLs, valid metadata, malformed metadata rejection, version ordering and HTTP timeouts.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
