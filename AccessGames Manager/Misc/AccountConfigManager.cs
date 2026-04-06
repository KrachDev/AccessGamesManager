using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace AccessGamesManager.Misc
{
    public enum AccountRole
    {
        Access,     // Default — shared/access account, gets offline+firewall treatment
        Personal,   // Your own account — logs in normally online, hidden from shared library
    }

    public enum ForceLaunchMode
    {
        Auto,          // Use role logic to decide
        ForceOnline,   // Always launch online regardless of role
        ForceOffline   // Always launch offline regardless of role
    }

    public class AccountConfig
    {
        public Dictionary<string, AccountRole> Roles { get; set; } = new();
        public ForceLaunchMode LaunchMode { get; set; } = ForceLaunchMode.Auto;
        public bool UseRegisteredOwner { get; set; } = true;
        public AppLanguage Language { get; set; } = AppLanguage.English;

        /// <summary>
        /// Per-game owner override: key = AppID, value = AccountID the user chose.
        /// </summary>
        public Dictionary<string, string> GameOwnerOverrides { get; set; } = new();

        /// <summary>
        /// Steam JWT refresh tokens keyed by AccountName (lowercase).
        /// Stored so we can re-authenticate without asking for a password again.
        /// </summary>
        public Dictionary<string, string> RefreshTokens { get; set; } = new();
    }

    public static class AccountConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccessGamesManager", "account_config.json");

        private static AccountConfig _config = new();

        public static void Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                    _config = JsonConvert.DeserializeObject<AccountConfig>(File.ReadAllText(ConfigPath)) ?? new();
            }
            catch { _config = new(); }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
            catch { }
        }

        public static AccountRole GetRole(string accountID)
            => _config.Roles.TryGetValue(accountID, out var r) ? r : AccountRole.Access;

        public static void SetRole(string accountID, AccountRole role)
        {
            _config.Roles[accountID] = role;
            Save();
        }

        public static bool IsPersonal(string accountID)
            => GetRole(accountID) == AccountRole.Personal;

        // Personal => always online (no firewall block). Access => offline by default.
        public static bool IsNormalLogin(string accountID)
            => GetRole(accountID) == AccountRole.Personal;

        public static bool ShouldLaunchOffline(string accountID)
        {
            return _config.LaunchMode switch
            {
                ForceLaunchMode.ForceOnline  => false,
                ForceLaunchMode.ForceOffline => true,
                _                            => !IsNormalLogin(accountID)
            };
        }

        public static ForceLaunchMode GetLaunchMode() => _config.LaunchMode;
        public static void SetLaunchMode(ForceLaunchMode mode) { _config.LaunchMode = mode; Save(); }

        public static bool GetUseRegisteredOwner() => _config.UseRegisteredOwner;
        public static void SetUseRegisteredOwner(bool value) { _config.UseRegisteredOwner = value; Save(); }

        public static AppLanguage GetLanguage() => _config.Language;
        public static void SetLanguage(AppLanguage lang) { _config.Language = lang; Save(); }

        /// <summary>Returns the stored JWT refresh token for an account, or null.</summary>
        public static string? GetRefreshToken(string accountName)
            => _config.RefreshTokens.TryGetValue(accountName.ToLower(), out var t) ? t : null;

        /// <summary>Persists a new JWT refresh token for an account.</summary>
        public static void SaveRefreshToken(string accountName, string token)
        {
            _config.RefreshTokens[accountName.ToLower()] = token;
            Save();
        }

        /// <summary>Removes a stored token (e.g. after it has been revoked or expired).</summary>
        public static void ClearRefreshToken(string accountName)
        {
            _config.RefreshTokens.Remove(accountName.ToLower());
            Save();
        }

        // ── Per-game owner override ────────────────────────────────────────────

        /// <summary>Returns the user-chosen owner AccountID for a game, or null if not overridden.</summary>
        public static string? GetGameOwnerOverride(string appID)
            => _config.GameOwnerOverrides.TryGetValue(appID, out var id) ? id : null;

        /// <summary>Stores which account the user wants to use for a given game.</summary>
        public static void SetGameOwnerOverride(string appID, string accountID)
        {
            _config.GameOwnerOverrides[appID] = accountID;
            Save();
        }
    }
}
