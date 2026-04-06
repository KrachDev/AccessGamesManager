using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Checks the latest GitHub Release and performs a silent self-replace.
    ///
    /// Just set <see cref="GitHubRepo"/> to your "username/repo" and you're done.
    /// Every time you push a new GitHub Release (mark it as Latest), all installed
    /// copies will pick it up automatically on next launch.
    ///
    /// The release must have one asset attached: the built .exe
    /// (any name ending in .exe — first one found is used).
    /// The release body is shown as the changelog in the update dialog.
    /// </summary>
    public static class AutoUpdater
    {
        // ── CONFIGURE THIS ────────────────────────────────────────────────────
        // "username/repository" — nothing else needed
        public const string GitHubRepo = "KrachDev/AccessMA";
        // ─────────────────────────────────────────────────────────────────────

        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15),
            DefaultRequestHeaders =
            {
                // GitHub API requires a User-Agent
                { "User-Agent", "AccessGamesManager-Updater" },
                { "Accept",     "application/vnd.github+json" }
            }
        };

        /// <summary>
        /// Hits the GitHub latest-release endpoint and returns an
        /// <see cref="UpdateInfo"/> when a newer version is available, or null.
        /// </summary>
        public static async Task<UpdateInfo?> CheckAsync()
        {
            try
            {
                string url  = $"https://api.github.com/repos/{GitHubRepo}/releases/latest";
                string json = await _http.GetStringAsync(url);
                var    obj  = JObject.Parse(json);

                // tag_name is typically "v2.1.0" — strip the leading 'v'
                string tag     = (obj["tag_name"]?.ToString() ?? "").TrimStart('v');
                string body    = obj["body"]?.ToString() ?? "";

                // Find the first .exe asset
                string? downloadUrl = null;
                var assets = obj["assets"] as JArray;
                if (assets != null)
                    foreach (var asset in assets)
                    {
                        string? name = asset["name"]?.ToString();
                        if (name != null && name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            downloadUrl = asset["browser_download_url"]?.ToString();
                            break;
                        }
                    }

                if (string.IsNullOrEmpty(downloadUrl) || string.IsNullOrEmpty(tag))
                    return null;

                Version current = Assembly.GetEntryAssembly()?.GetName().Version
                                  ?? new Version(0, 0, 0);
                Version latest  = Version.Parse(tag);

                return latest > current
                    ? new UpdateInfo { Version = tag, Changelog = body, DownloadUrl = downloadUrl }
                    : null;
            }
            catch { return null; } // silent — no internet, private repo, etc.
        }

        /// <summary>
        /// Downloads the new exe to %TEMP%, launches it with the self-replace
        /// argument, then exits the current process.
        /// </summary>
        public static async Task DownloadAndReplaceAsync(UpdateInfo info, IProgress<int>? progress = null)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "AGM_update.exe");

            using var response = await _http.GetAsync(
                info.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long total    = response.Content.Headers.ContentLength ?? -1;
            long received = 0;

            await using var src  = await response.Content.ReadAsStreamAsync();
            await using var dest = File.Create(tempPath);

            var buffer = new byte[81920];
            int read;
            while ((read = await src.ReadAsync(buffer)) > 0)
            {
                await dest.WriteAsync(buffer.AsMemory(0, read));
                received += read;
                if (total > 0) progress?.Report((int)(received * 100 / total));
            }

            progress?.Report(100);
            dest.Close();

            string currentExe = Process.GetCurrentProcess().MainModule?.FileName ?? "";
            Process.Start(new ProcessStartInfo
            {
                FileName        = tempPath,
                Arguments       = $"--update-replace \"{currentExe}\"",
                UseShellExecute = true
            });

            await Task.Delay(800);
            Environment.Exit(0);
        }
    }

    public class UpdateInfo
    {
        public string? Version     { get; set; }
        public string? Changelog   { get; set; }
        public string? DownloadUrl { get; set; }
    }
}
