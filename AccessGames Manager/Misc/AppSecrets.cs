using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Loads sensitive API keys from a local secrets.json file that is NOT committed to source control.
    /// Copy secrets.json.template to secrets.json and fill in your keys.
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
                        File.ReadAllText(SecretsPath)) ?? new Dictionary<string, string>();
                }
                catch
                {
                    _secrets = new Dictionary<string, string>();
                }
            }
            else
            {
                _secrets = new Dictionary<string, string>();
            }
            return _secrets;
        }

        public static string SteamApiKey => Load().TryGetValue("SteamApiKey", out var v) ? v : string.Empty;
        public static string RawgApiKey  => Load().TryGetValue("RawgApiKey",  out var v) ? v : string.Empty;
    }
}
