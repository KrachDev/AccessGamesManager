using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Loads API keys and admin credentials from a local secrets.json file
    /// that is NOT committed to source control.
    /// Copy secrets.json.template → secrets.json and fill in your keys.
    /// </summary>
    public static class AppSecrets
    {
        private static readonly string SecretsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "secrets.json");

        private static Dictionary<string, string>? _secrets;

        private static Dictionary<string, string> Load()
        {
            if (_secrets != null) return _secrets;
            if (File.Exists(SecretsPath))
            {
                try
                {
                    _secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        File.ReadAllText(SecretsPath)) ?? new();
                }
                catch { _secrets = new(); }
            }
            else
            {
                _secrets = new();
            }
            return _secrets;
        }

        private static void Save()
        {
            try { File.WriteAllText(SecretsPath, JsonConvert.SerializeObject(_secrets, Formatting.Indented)); }
            catch { }
        }

        public static bool Has(string key) => Load().TryGetValue(key, out var v) && !string.IsNullOrEmpty(v);

        // ── Keys ─────────────────────────────────────────────────────────────

        public static string SteamApiKey => Load().TryGetValue("SteamApiKey",        out var v) ? v : string.Empty;
        public static string RawgApiKey  => Load().TryGetValue("RawgApiKey",          out var v) ? v : string.Empty;
        public static string GithubPat   => Load().TryGetValue("GithubPat",           out var v) ? v : string.Empty;

        // ── Admin password (SHA-256 hash) ─────────────────────────────────────

        public static string AdminPasswordHash
            => Load().TryGetValue("AdminPasswordHash", out var v) ? v : string.Empty;

        public static bool HasAdminPassword => !string.IsNullOrEmpty(AdminPasswordHash);

        /// <summary>Checks an entered plaintext password against the stored hash.</summary>
        public static bool VerifyAdminPassword(string plaintext)
        {
            string stored = AdminPasswordHash;
            if (string.IsNullOrEmpty(stored)) return false;
            return HashPassword(plaintext) == stored;
        }

        /// <summary>
        /// Hashes <paramref name="plaintext"/> with SHA-256 and saves it to secrets.json.
        /// Call this once to initialise the admin password.
        /// </summary>
        public static void SetAdminPassword(string plaintext)
        {
            var d = Load();
            d["AdminPasswordHash"] = HashPassword(plaintext);
            _secrets = d;
            Save();
        }

        /// <summary>Returns the SHA-256 hex string of a plaintext password.</summary>
        public static string HashPassword(string plaintext)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plaintext));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
