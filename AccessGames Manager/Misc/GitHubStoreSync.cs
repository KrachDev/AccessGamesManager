using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Handles fetching and pushing store.json to/from GitHub.
    /// Public repos: fetch requires no auth.
    /// Push (write) always requires a PAT in secrets.json under "GithubPat".
    /// </summary>
    public static class GitHubStoreSync
    {
        // ── Configure these ──────────────────────────────────────────────────
        public const string Repo     = "KrachDev/AccessGamesManager";
        public const string Branch   = "main";
        public const string FilePath = "data/store.json";
        // ─────────────────────────────────────────────────────────────────────

        private static readonly string RawUrl =
            $"https://raw.githubusercontent.com/{Repo}/{Branch}/{FilePath}";

        private static readonly string ApiUrl =
            $"https://api.github.com/repos/{Repo}/contents/{FilePath}";

        private static readonly HttpClient _http = new()
        {
            Timeout = TimeSpan.FromSeconds(20),
            DefaultRequestHeaders =
            {
                { "User-Agent", "AccessGamesManager" },
                { "Accept",     "application/vnd.github+json" }
            }
        };

        /// <summary>
        /// Fetches the raw store.json from GitHub.
        /// Works for public repos without any authentication.
        /// Returns null if the fetch fails or the file doesn't exist yet.
        /// </summary>
        public static async Task<string?> FetchRawAsync()
        {
            try
            {
                // Cache-bust with timestamp so GitHub's CDN doesn't serve stale data
                string url = $"{RawUrl}?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                return await _http.GetStringAsync(url);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Pushes the given JSON content as a commit to GitHub.
        /// Requires a PAT with repo scope in secrets.json["GithubPat"].
        /// </summary>
        /// <param name="jsonContent">The full store.json content.</param>
        /// <param name="commitMessage">Git commit message.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static async Task<bool> PushAsync(string jsonContent, string commitMessage)
        {
            string pat = AppSecrets.GithubPat;
            if (string.IsNullOrEmpty(pat))
                return false;

            try
            {
                // We need the file SHA to update — fetch via the API
                string? fileSha = await GetFileShaAsync(pat);

                var payload = new JObject
                {
                    ["message"] = commitMessage,
                    ["content"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonContent)),
                    ["branch"]  = Branch
                };
                if (fileSha != null)
                    payload["sha"] = fileSha;

                var request = new HttpRequestMessage(HttpMethod.Put, ApiUrl)
                {
                    Content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pat);

                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the current SHA of the file in the repo (needed for updates).
        /// Returns null if the file doesn't exist yet (first push).
        /// </summary>
        private static async Task<string?> GetFileShaAsync(string pat)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}?ref={Branch}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pat);
                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;

                string json  = await response.Content.ReadAsStringAsync();
                var    obj   = JObject.Parse(json);
                return obj["sha"]?.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static bool HasPat => !string.IsNullOrEmpty(AppSecrets.GithubPat);
    }
}
