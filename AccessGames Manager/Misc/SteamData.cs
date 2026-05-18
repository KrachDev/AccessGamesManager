using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using HandyControl.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MessageBox = HandyControl.Controls.MessageBox;
using System.Windows;

namespace AccessGamesManager.Misc
{
    public class SteamData
    {
        // API key loaded from secrets.json -- never hardcode here
        private static string ApiKey => AppSecrets.SteamApiKey;

        private static readonly HttpClient httpClient = new HttpClient();
        private SteamClient?       steamClient;
        private CallbackManager?   manager;
        private SteamUser?         steamUser;
        private SteamUserStats?    steamUserStats;
        private static SteamFriends? steamFriends;

        public AccessGames_Manager.Views.MainWindow? mainWindow { get; set; }
        public string STEAM_INSTALLTION_PATH = "";
        public string STEAM_REG_PATH  = @"HKEY_CURRENT_USER\Software\Valve\Steam";
        public string STEAM_USERS_PATH = "";
        private const string KEY_AUTOLOGIN = "AutoLoginUser";

        public string? sessionID;
        public string? steamLogin;
        public List<string> gameList = new List<string>();
        public string? steamID64;
        public string? profileUrl;
        private bool isRunning = true;
        public string? username;
        public string? password;
        public string? steamID { get; set; }
        public bool LaunchSteamAccount = true;

        public SteamData()
        {
            STEAM_INSTALLTION_PATH = GetSteamPath() ?? "";
            STEAM_USERS_PATH = Path.Combine(STEAM_INSTALLTION_PATH, "config", "loginusers.vdf");
            AccountConfigManager.Load();
        }

        public string? GetAutoLoginUser()
        {
            try { return Registry.GetValue(STEAM_REG_PATH, KEY_AUTOLOGIN, null) as string; }
            catch (Exception ex) { Console.WriteLine($"Registry error: {ex.Message}"); return null; }
        }

        public List<SteamUserEntry> GetSteamUsers()
        {
            var list = new List<SteamUserEntry>();
            if (!File.Exists(STEAM_USERS_PATH))
            { MessageBox.Show("loginusers.vdf not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return list; }

            try
            {
                var vdf  = VdfConvert.Deserialize(File.ReadAllText(STEAM_USERS_PATH));
                var json = JObject.Parse($"{{{vdf.ToJson()}}}");
                if (json["users"] == null) throw new Exception("'users' key missing in VDF.");

                var dict = JsonConvert.DeserializeObject<Dictionary<string, SteamUserEntry>>(json["users"]!.ToString())!;
                foreach (var (id, entry) in dict)
                {
                    entry.AccountID   = id;
                    entry.AvatarImage = Path.Combine(STEAM_INSTALLTION_PATH, "config", "avatarcache", $"{id}.png");
                    list.Add(entry);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading Steam users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                try { copy(ex.Message); } catch { }
            }
            return list;
        }

        public string? GetSteamPath()
        {
            string? path = (string?)Registry.GetValue(STEAM_REG_PATH, "SteamPath", null);
            if (string.IsNullOrEmpty(path)) MessageBox.Error("Steam installation path not found in registry.");
            return path;
        }

        public async Task<string?> ResolveVanityURL(string vanityUrl)
        {
            try
            {
                if (vanityUrl.Contains("/profiles/"))
                    return vanityUrl.Split("/profiles/")[1].TrimEnd('/');

                if (vanityUrl.Contains("/id/"))
                {
                    string uname = vanityUrl.Split("/id/")[1].TrimEnd('/');
                    string url = $"https://api.steampowered.com/ISteamUser/ResolveVanityURL/v1/?key={ApiKey}&vanityurl={uname}";
                    var response = await httpClient.GetStringAsync(url);
                    JObject data = JObject.Parse(response);
                    if ((int)data["response"]!["success"]! == 1)
                        return data["response"]!["steamid"]?.ToString();
                    Growl.ErrorGlobal("Failed to resolve vanity URL.");
                    return null;
                }
                Growl.ErrorGlobal("Invalid URL format.");
                return null;
            }
            catch (Exception ex) { Growl.ErrorGlobal($"Error resolving URL: {ex.Message}"); return null; }
        }

        public List<SteamGame> GetInstalledGames()
        {
            var list = new List<SteamGame>();
            const string regPath = @"Software\Valve\Steam\Apps";
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(regPath);
                if (key == null) return list;
                foreach (string subName in key.GetSubKeyNames())
                {
                    using var sub = key.OpenSubKey(subName);
                    if (sub == null) continue;
                    string name = sub.GetValue("Name")?.ToString() ?? "Unknown";
                    if (name == "Unknown") continue;
                    int installed = Convert.ToInt32(sub.GetValue("Installed", 0));
                    list.Add(new SteamGame { AppID = subName, Name = name, isInstalled = installed == 1 });
                }
            }
            catch (Exception ex) { Console.WriteLine($"Registry read error: {ex.Message}"); }
            return list;
        }

        public string GetGameOwner(string appID)
        {
            string ownerId = "";
            string acfFile = Path.Combine(STEAM_INSTALLTION_PATH, "steamapps", $"appmanifest_{appID}.acf");
            if (!File.Exists(acfFile)) return ownerId;
            try
            {
                string content   = File.ReadAllText(acfFile);
                const string key = "\"LastOwner\"";
                int start        = content.IndexOf(key);
                if (start == -1) return ownerId;
                start = content.IndexOf('"', start + key.Length + 1) + 1;
                int end = content.IndexOf('"', start);
                if (start != -1 && end != -1) ownerId = content.Substring(start, end - start);
            }
            catch (Exception ex) { MessageBox.Error($"Error reading ACF: {ex.Message}"); }
            return ownerId;
        }

        public string GetGameImages(string appID, int imageType)
        {
            string baseDir = Path.Combine(STEAM_INSTALLTION_PATH, "appcache", "librarycache", appID);
            if (imageType == 5)
            {
                if (!Directory.Exists(baseDir)) return "";
                foreach (var sub in Directory.GetDirectories(baseDir))
                {
                    string candidate = Path.Combine(sub, "library_capsule.jpg");
                    if (File.Exists(candidate)) return candidate;
                }
                return "";
            }
            string target = imageType switch
            {
                1 => "library_header.jpg", 2 => "library_hero.jpg",
                3 => "library_hero_blur.jpg", 4 => "logo.png", 6 => "icon.jpg", _ => ""
            };
            if (string.IsNullOrEmpty(target) || !Directory.Exists(baseDir)) return "";
            foreach (var sub in Directory.GetDirectories(baseDir))
            {
                string c = Path.Combine(sub, target);
                if (File.Exists(c)) return c;
            }
            string direct = Path.Combine(baseDir, target);
            return File.Exists(direct) ? direct : "";
        }

        public async Task<List<string>> GetOwnedGameNames(string vanityUrl)
        {
            var gameNames = new List<string>();
            try
            {
                string? steamId = await ResolveVanityURL(vanityUrl);
                if (string.IsNullOrEmpty(steamId)) return gameNames;
                string url = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={ApiKey}&steamid={steamId}&include_appinfo=true";
                var response = await httpClient.GetStringAsync(url);
                JObject data = JObject.Parse(response);
                var games = data["response"]?["games"];
                if (games == null) { Growl.ErrorGlobal("No games found."); return gameNames; }
                foreach (var game in games) gameNames.Add(game["name"]?.ToString() ?? "Unknown Game");
            }
            catch (Exception ex) { Growl.ErrorGlobal($"Error fetching games: {ex.Message}"); }
            return gameNames;
        }

        public async Task<int?> GetAppIdByNameAsync(string gameName)
        {
            const string url = "https://api.steampowered.com/ISteamApps/GetAppList/v2/";
            try
            {
                var response = await httpClient.GetStringAsync(url);
                var appList  = JsonConvert.DeserializeObject<AppListResponse>(response);
                return appList?.Apps?.FirstOrDefault(a =>
                    a.Name.Equals(gameName, StringComparison.OrdinalIgnoreCase))?.AppId;
            }
            catch (Exception ex) { Growl.ErrorGlobal("Error fetching App ID: " + ex.Message); return null; }
        }

        public async Task LogINAcc()
        {
            try
            {
                Growl.ClearGlobal();
                await LogOff();

                steamClient   ??= new SteamClient();
                manager       ??= new CallbackManager(steamClient);
                steamUser       = steamClient.GetHandler<SteamUser>();
                steamUserStats  = steamClient.GetHandler<SteamUserStats>();
                steamFriends    = steamClient.GetHandler<SteamFriends>();

                manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
                manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
                manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
                manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
                manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

                isRunning = true;
                steamClient.Connect();

                await Task.Run(() =>
                {
                    while (isRunning) manager!.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                });
            }
            catch (Exception ex) { Growl.ErrorGlobal($"Login error: {ex.Message}"); }
        }

        public async Task LogOff()
        {
            isRunning = false;
            if (steamUser != null) try { steamUser.LogOff(); } catch { }
            if (steamClient != null) try { steamClient.Disconnect(); } catch { }
            await Task.Delay(500);
        }

        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            Growl.InfoGlobal("Connected to Steam. Logging in...");
            steamUser!.LogOn(new SteamUser.LogOnDetails { Username = username, Password = password });
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            isRunning = false;
            Growl.InfoGlobal("Disconnected from Steam.");
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            { Growl.ErrorGlobal($"Login failed: {callback.Result}"); isRunning = false; return; }
            steamID64 = steamClient!.SteamID!.ConvertToUInt64().ToString();
            Growl.SuccessGlobal($"Logged in as {username}. SteamID64: {steamID64}");
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            isRunning = false;
            Growl.InfoGlobal("Logged off from Steam.");
        }

        private void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            steamUser!.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                BytesWritten    = callback.BytesToWrite,
                FileName        = callback.FileName,
                FileSize        = callback.BytesToWrite,
                Offset          = callback.Offset,
                OneTimePassword = callback.OneTimePassword,
                JobID           = callback.JobID
            });
            Growl.SuccessGlobal("Machine auth completed.");
        }

        public string? GetActiveUserSteamId64()
        {
            string? activeName = GetAutoLoginUser();
            if (string.IsNullOrEmpty(activeName)) return null;
            var users = GetSteamUsers();
            return users.FirstOrDefault(u => u.AccountName != null && u.AccountName.Equals(activeName, StringComparison.OrdinalIgnoreCase))?.AccountID;
        }

        public void ResetGameTime(string appID)
        {
            string? activeId = GetActiveUserSteamId64();
            string gameUserId = !string.IsNullOrEmpty(activeId) ? activeId : GetGameOwner(appID);
            int userID32      = SteamId64ToUserID32(gameUserId);
            string timeFile   = Path.Combine(GetSteamPath() ?? "", "userdata", userID32.ToString(), "config", "localconfig.vdf");
            try { Clipboard.SetText(timeFile); } catch { }
            if (!File.Exists(timeFile)) { Console.WriteLine("localconfig.vdf not found."); return; }
            File.WriteAllText(timeFile, UpdateGamePlaytime(File.ReadAllText(timeFile), appID));
            Growl.Success("Playtime reset.");
        }

        private string UpdateGamePlaytime(string content, string appID)
        {
            var match = Regex.Match(content, $@"""{appID}""[\s\S]*?}}");
            if (match.Success) content = content.Replace(match.Value, ModifyGamePlaytime(match.Value));
            return content;
        }

        private string ModifyGamePlaytime(string entry)
        {
            entry = Regex.Replace(entry, "(?i)\"LastPlayed\"[\\s]*\"[0-9]+\"",   "\"LastPlayed\" \"0\"");
            entry = Regex.Replace(entry, "(?i)\"Playtime2wks\"[\\s]*\"[0-9]+\"", "\"Playtime2wks\" \"0\"");
            entry = Regex.Replace(entry, "(?i)\"playtime\"[\\s]*\"[0-9]+\"",     "\"Playtime\" \"0\"");
            return entry;
        }

        public int GetGamePlayTime(string appID)
        {
            string? activeId = GetActiveUserSteamId64();
            string gameUserId = !string.IsNullOrEmpty(activeId) ? activeId : GetGameOwner(appID);
            int userID32      = SteamId64ToUserID32(gameUserId);
            string timeFile   = Path.Combine(GetSteamPath() ?? "", "userdata", userID32.ToString(), "config", "localconfig.vdf");
            MessageBox.Show(timeFile);
            if (!File.Exists(timeFile)) { MessageBox.Warning("localconfig.vdf not found."); return -1; }
            return ExtractPlaytime(File.ReadAllText(timeFile), appID);
        }

        private int ExtractPlaytime(string content, string appID)
        {
            var match = Regex.Match(content, $"(?i)\"{appID}\"[\\s\\S]*?\"playtime\"[\\s]*\"([0-9]+)\"");
            return match.Success && int.TryParse(match.Groups[1].Value, out int pt) ? pt : -1;
        }

        public void ResetAchievementsWithSAM(string appIDStr)
        {
            if (uint.TryParse(appIDStr, out uint appId))
            {
                try
                {
                    // Extract steam_api64.dll from embedded resources to temp so the exe stays single-file
                    string dllPath = Path.Combine(Path.GetTempPath(), "steam_api64.dll");
                    if (!File.Exists(dllPath))
                    {
                        using var stream = typeof(SteamData).Assembly.GetManifestResourceStream("steam_api64.dll");
                        if (stream != null)
                        {
                            using var fs = File.Create(dllPath);
                            stream.CopyTo(fs);
                        }
                    }
                    if (File.Exists(dllPath))
                        System.Runtime.InteropServices.NativeLibrary.Load(dllPath);

                    Steamworks.SteamClient.Init(appId);

                    // Pump callbacks so Steam loads the stats
                    for (int i = 0; i < 10; i++)
                    {
                        Steamworks.SteamClient.RunCallbacks();
                        System.Threading.Thread.Sleep(100);
                    }

                    // ResetAll(true) = reset all stats AND achievements (same as SAM's ResetAllStats)
                    bool resetOk = Steamworks.SteamUserStats.ResetAll(true);
                    if (resetOk)
                    {
                        Steamworks.SteamUserStats.StoreStats();

                        // Pump callbacks again so Steam processes the store
                        for (int i = 0; i < 20; i++)
                        {
                            Steamworks.SteamClient.RunCallbacks();
                            System.Threading.Thread.Sleep(100);
                        }
                    }

                    Steamworks.SteamClient.Shutdown();
                    Growl.Success("Achievements reset via Steamworks API.");
                }
                catch (Exception ex)
                {
                    Growl.ErrorGlobal($"Steamworks reset failed: {ex.Message}");
                    try { Steamworks.SteamClient.Shutdown(); } catch { }
                }
            }
        }

        public string GetAchievementsCount(string appidOfTheGame)
        {
            string gameUserId = GetGameOwner(appidOfTheGame);
            int userID32      = SteamId64ToUserID32(gameUserId);
            string filePath   = Path.Combine(GetSteamPath() ?? "", "userdata", userID32.ToString(), "config", "librarycache", $"{appidOfTheGame}.json");
            try { Clipboard.SetText(filePath); } catch { }

            if (!File.Exists(filePath)) return "File not found";
            try
            {
                JArray jsonArray = JArray.Parse(File.ReadAllText(filePath));
                if (jsonArray[0] is JArray achievementsArray)
                {
                    int total = 0, unlocked = 0;
                    foreach (var item in achievementsArray)
                        if (item is JObject d && d["data"]?["vecHighlight"] is JArray)
                        { total = (int)d["data"]!["nTotal"]!; unlocked = (int)d["data"]!["nAchieved"]!; }
                    return $"{unlocked}/{total}";
                }
                return "Invalid JSON structure.";
            }
            catch (Exception ex) { copy(ex.Message); return $"Error: {ex.Message}"; }
        }

        public const string BlockRuleName = "SteamNetworking";

        private static void RunNetsh(string args)
        {
            var psi = new ProcessStartInfo("netsh", args)
            { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
            Process.Start(psi)?.WaitForExit();
        }

        public bool IsSteamNetworkBlocked()
        {
            try
            {
                var psi = new ProcessStartInfo("netsh", $"advfirewall firewall show rule name=\"{BlockRuleName}\"")
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true };
                var p      = Process.Start(psi)!;
                string out_ = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                bool blocked = out_.Contains(BlockRuleName);
                mainWindow?.CheckSteamNetworkStatus(blocked ? "Offline" : "Online");
                return blocked;
            }
            catch (Exception ex) { Console.WriteLine("Firewall check error: " + ex.Message); return false; }
        }

        public void BlockSteamNetwork()
        {
            try
            {
                RunNetsh($"advfirewall firewall delete rule name=\"{BlockRuleName}\"");
                string exe = Path.Combine(STEAM_INSTALLTION_PATH, "Steam.exe").Replace('/', '\\');
                RunNetsh($"advfirewall firewall add rule name=\"{BlockRuleName}\" dir=out action=block program=\"{exe}\" enable=yes");
            }
            catch (Exception ex) { Console.WriteLine("Block error: " + ex.Message); }
        }

        public void UnblockSteamNetwork()
        {
            try
            {
                RunNetsh($"advfirewall firewall delete rule name=\"{BlockRuleName}\"");
                if (File.Exists(STEAM_USERS_PATH))
                {
                    var attrs = File.GetAttributes(STEAM_USERS_PATH);
                    if (attrs.HasFlag(FileAttributes.ReadOnly))
                        File.SetAttributes(STEAM_USERS_PATH, attrs & ~FileAttributes.ReadOnly);
                }
                Growl.SuccessGlobal("Unblocked Steam network access.");
            }
            catch (Exception ex) { Growl.ErrorGlobal("Unblock error: " + ex.Message); }
        }

        public void SwitchAccount(SteamUserEntry targetUser)
        {
            try
            {
                string id = targetUser.AccountID ?? "";
                bool shouldBlock = AccountConfigManager.ShouldLaunchOffline(id);
                Growl.InfoGlobal($"Switching: {targetUser.AccountName} ({id}) | Block: {shouldBlock}");

                KillAllSteamProcesses();

                Registry.SetValue(STEAM_REG_PATH, KEY_AUTOLOGIN, targetUser.AccountName ?? "");

                string steamExe = Path.Combine(STEAM_INSTALLTION_PATH, "Steam.exe");
                if (!File.Exists(steamExe)) { Growl.ErrorGlobal("Could not find Steam.exe."); return; }

                //PatchLoginUsersVdf(id, shouldBlock);

                if (shouldBlock)
                {
                    // Block BEFORE launching so Steam never gets a chance to go online
                    BlockSteamNetwork();
                    Process.Start(new ProcessStartInfo { FileName = steamExe, Arguments = "-offline", UseShellExecute = true });
                    Growl.SuccessGlobal($"Switched to {targetUser.PersonaName} — Offline mode active.");
                }
                else
                {
                    UnblockSteamNetwork();
                    Process.Start(new ProcessStartInfo { FileName = steamExe, UseShellExecute = true });
                    Growl.SuccessGlobal($"Switched to {targetUser.PersonaName} — Online mode active.");
                }

                _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => mainWindow?.RefreshFirewallStatus());
            }
            catch (Exception ex) { Growl.ErrorGlobal($"Switch failed: {ex.Message}"); }
        }

        // ─── KILL ALL STEAM PROCESSES ────────────────────────────────────────────
        private static readonly string[] _steamProcessNames =
            { "steam", "steamwebhelper", "steamservice", "gameoverlayui", "steamtours", "steamcefdebug" };

        public void KillAllSteamProcesses(int waitMs = 2000)
        {
            foreach (var pname in _steamProcessNames)
                foreach (var p in Process.GetProcessesByName(pname))
                    try { p.Kill(); } catch { }
            System.Threading.Thread.Sleep(waitMs);
        }

        // ─── FIX STEAM INFINITE LOADING LOOP ────────────────────────────────────
        // Steps:
        //  1. Kill every Steam process (including webhelpers)
        //  2. Rename userdata → userdata_backup
        //  3. Launch Steam briefly (it creates a clean userdata and shows login)
        //  4. Kill Steam again
        //  5. Merge userdata_backup into the fresh userdata (old files win only if not present in new)
        //  6. Apply firewall block and relaunch in offline mode
        // waitCallback is awaited while Steam is open online (e.g. a countdown dialog).
        // Pass null to fall back to a plain 20-second delay.
        public async Task FixInfiniteLoadingLoop(Func<Task>? waitCallback = null)
        {
            try
            {
                Growl.InfoGlobal("Fix: killing all Steam processes…");
                KillAllSteamProcesses(2500);

                string userdataPath   = Path.Combine(STEAM_INSTALLTION_PATH, "userdata");
                string userdataBackup = Path.Combine(STEAM_INSTALLTION_PATH, "userdata_backup");

                // Clean up any leftover backup from a previous failed attempt
                if (Directory.Exists(userdataBackup))
                    Directory.Delete(userdataBackup, recursive: true);

                if (Directory.Exists(userdataPath))
                {
                    Growl.InfoGlobal("Fix: renaming userdata folder…");
                    Directory.Move(userdataPath, userdataBackup);
                }

                string steamExe = Path.Combine(STEAM_INSTALLTION_PATH, "Steam.exe");
                if (!File.Exists(steamExe)) { Growl.ErrorGlobal("Steam.exe not found."); return; }

                // Ensure Steam can reach the internet so it syncs/updates while open
                UnblockSteamNetwork();

                // Patch loginusers.vdf before launching so Steam doesn't get stuck
                // in its own infinite loading loop during this temporary online boot
                //EnsureAllowAutoLogin();

                // Launch Steam online — it will boot to the login window and create a fresh userdata
                Growl.InfoGlobal("Fix: launching Steam online to rebuild userdata…");
                Process.Start(new ProcessStartInfo { FileName = steamExe, UseShellExecute = true });

                // Wait (countdown dialog if provided, otherwise plain delay)
                if (waitCallback != null)
                    await waitCallback();
                else
                    await Task.Delay(20_000);

                // Kill it again before the user can do anything
                Growl.InfoGlobal("Fix: killing Steam again…");
                KillAllSteamProcesses(2000);

                // Merge: copy everything from backup into new userdata;
                // skip files that Steam already created fresh in the new folder
                if (Directory.Exists(userdataBackup))
                {
                    if (Directory.Exists(userdataPath))
                    {
                        Growl.InfoGlobal("Fix: merging userdata folders…");
                        MergeDirectories(userdataBackup, userdataPath, overwriteExisting: false);
                    }
                    else
                    {
                        // Steam didn’t create a new userdata — just restore the backup
                        Directory.Move(userdataBackup, userdataPath);
                    }
                    if (Directory.Exists(userdataBackup))
                        Directory.Delete(userdataBackup, recursive: true);
                }

                // Block firewall BEFORE relaunching so Steam goes straight into offline mode
                Growl.InfoGlobal("Fix: applying firewall block and launching offline…");
                BlockSteamNetwork();
                Process.Start(new ProcessStartInfo { FileName = steamExe, Arguments = "-offline", UseShellExecute = true });

                Growl.SuccessGlobal("✔ Infinite-loop fix applied — Steam launched in offline mode.");
                _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => mainWindow?.RefreshFirewallStatus());
            }
            catch (Exception ex) { Growl.ErrorGlobal($"Fix failed: {ex.Message}"); }
        }

        private void EnsureAllowAutoLogin()
        {
            if (!File.Exists(STEAM_USERS_PATH)) return;
            try
            {
                // Make sure the file is writable first
                var attrs = File.GetAttributes(STEAM_USERS_PATH);
                if (attrs.HasFlag(FileAttributes.ReadOnly))
                    File.SetAttributes(STEAM_USERS_PATH, attrs & ~FileAttributes.ReadOnly);

                string vdf = File.ReadAllText(STEAM_USERS_PATH);

                // Replace every occurrence of AllowAutoLogin set to anything other than 1
                vdf = Regex.Replace(
                    vdf,
                    @"""AllowAutoLogin""\ *""\d+""",
                    "\"AllowAutoLogin\"\t\t\"1\"");

                File.WriteAllText(STEAM_USERS_PATH, vdf);
                Growl.InfoGlobal("Fix: AllowAutoLogin restored to 1 for all accounts.");
            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal($"Could not patch loginusers.vdf: {ex.Message}");
            }
        }

        private void PatchLoginUsersVdf(string targetAccountID, bool shouldBlock)
        {
            if (!File.Exists(STEAM_USERS_PATH)) return;
            try
            {
                var attrs = File.GetAttributes(STEAM_USERS_PATH);
                if (attrs.HasFlag(FileAttributes.ReadOnly))
                    File.SetAttributes(STEAM_USERS_PATH, attrs & ~FileAttributes.ReadOnly);

                var lines = File.ReadAllLines(STEAM_USERS_PATH).ToList();
                string currentAccount = "";
                bool insideUser = false;
                
                // Track which keys we've seen in the current user block
                bool seenMostRecent = false;
                bool seenAllowAutoLogin = false;
                bool seenWantsOfflineMode = false;
                bool seenSkipOfflineModeWarning = false;
                
                for (int i = 0; i < lines.Count; i++)
                {
                    string trimmed = lines[i].Trim();
                    
                    if (Regex.IsMatch(trimmed, @"^""7656[0-9]+""$"))
                    {
                        currentAccount = trimmed.Replace("\"", "");
                        insideUser = true;
                        seenMostRecent = false;
                        seenAllowAutoLogin = false;
                        seenWantsOfflineMode = false;
                        seenSkipOfflineModeWarning = false;
                        continue;
                    }
                    
                    if (trimmed == "}" && insideUser)
                    {
                        bool isTarget = (currentAccount == targetAccountID);
                        
                        // Insert missing keys
                        if (!seenMostRecent)
                            lines.Insert(i++, $"\t\t\"MostRecent\"\t\t\"{(isTarget ? "1" : "0")}\"");
                        if (!seenAllowAutoLogin)
                            lines.Insert(i++, "\t\t\"AllowAutoLogin\"\t\t\"1\"");
                            
                        if (isTarget && shouldBlock)
                        {
                            if (!seenWantsOfflineMode)
                                lines.Insert(i++, "\t\t\"WantsOfflineMode\"\t\t\"1\"");
                            if (!seenSkipOfflineModeWarning)
                                lines.Insert(i++, "\t\t\"SkipOfflineModeWarning\"\t\t\"1\"");
                        }
                        else if (isTarget && !shouldBlock)
                        {
                            if (!seenWantsOfflineMode)
                                lines.Insert(i++, "\t\t\"WantsOfflineMode\"\t\t\"0\"");
                            if (!seenSkipOfflineModeWarning)
                                lines.Insert(i++, "\t\t\"SkipOfflineModeWarning\"\t\t\"0\"");
                        }
                        
                        insideUser = false;
                        continue;
                    }
                    
                    if (insideUser)
                    {
                        bool isTarget = (currentAccount == targetAccountID);
                        
                        if (trimmed.StartsWith("\"MostRecent\""))
                        {
                            lines[i] = $"\t\t\"MostRecent\"\t\t\"{(isTarget ? "1" : "0")}\"";
                            seenMostRecent = true;
                        }
                        else if (trimmed.StartsWith("\"AllowAutoLogin\""))
                        {
                            lines[i] = "\t\t\"AllowAutoLogin\"\t\t\"1\"";
                            seenAllowAutoLogin = true;
                        }
                        else if (trimmed.StartsWith("\"WantsOfflineMode\""))
                        {
                            if (isTarget)
                                lines[i] = $"\t\t\"WantsOfflineMode\"\t\t\"{(shouldBlock ? "1" : "0")}\"";
                            seenWantsOfflineMode = true;
                        }
                        else if (trimmed.StartsWith("\"SkipOfflineModeWarning\""))
                        {
                            if (isTarget)
                                lines[i] = $"\t\t\"SkipOfflineModeWarning\"\t\t\"{(shouldBlock ? "1" : "0")}\"";
                            seenSkipOfflineModeWarning = true;
                        }
                    }
                }
                
                File.WriteAllLines(STEAM_USERS_PATH, lines);
            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal($"Could not patch loginusers.vdf: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively copies all files from <paramref name="source"/> into <paramref name="target"/>.
        /// If <paramref name="overwriteExisting"/> is false, existing files in the target are kept untouched.
        /// </summary>
        private static void MergeDirectories(string source, string target, bool overwriteExisting)
        {
            // Ensure all subdirectories exist in target
            foreach (string dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                string rel = Path.GetRelativePath(source, dir);
                Directory.CreateDirectory(Path.Combine(target, rel));
            }
            // Copy files
            foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                string rel        = Path.GetRelativePath(source, file);
                string targetFile = Path.Combine(target, rel);
                if (overwriteExisting || !File.Exists(targetFile))
                    File.Copy(file, targetFile, overwrite: overwriteExisting);
            }
        }

        public void LaunchGame(SteamUserEntry account, string appID, bool offline)
        {
            try
            {
                SwitchAccount(account);
                Task.Run(async () =>
                {
                    // Step 1: wait for steam.exe to appear (max 30s)
                    int waited = 0;
                    while (waited < 30000)
                    {
                        if (Process.GetProcessesByName("steam").Length > 0) break;
                        await Task.Delay(400); waited += 400;
                    }

                    // Step 2: wait until steam.exe has a visible main window — 
                    // this is the key fix. Steam is "ready" only once its window appears,
                    // not just when the process starts. Handles the webhelper respawn race.
                    waited = 0;
                    while (waited < 20000)
                    {
                        var steamProc = Process.GetProcessesByName("steam").FirstOrDefault();
                        if (steamProc != null && steamProc.MainWindowHandle != IntPtr.Zero) break;
                        await Task.Delay(500); waited += 500;
                    }

                    // Step 3: extra buffer to let Steam fully settle its session
                    await Task.Delay(3000);

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"steam://rungameid/{appID}",
                        UseShellExecute = true
                    });

                    Growl.SuccessGlobal($"Launching game {appID} as {account.PersonaName}…");
                    _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => mainWindow?.RefreshFirewallStatus());
                });
            }
            catch (Exception ex) { Growl.ErrorGlobal($"LaunchGame failed: {ex.Message}"); }
        }
        public void ForceLoginPage()
        {
            try
            {
                foreach (var p in Process.GetProcessesByName("steam")) try { p.Kill(); } catch { }
                System.Threading.Thread.Sleep(1000);
                Registry.SetValue(STEAM_REG_PATH, KEY_AUTOLOGIN, "");
                if (File.Exists(STEAM_USERS_PATH))
                {
                    string vdf = File.ReadAllText(STEAM_USERS_PATH);
                    vdf = Regex.Replace(vdf, "\"MostRecent\"\\s*\"\\d+\"", "\"MostRecent\"\t\t\"0\"");
                    File.WriteAllText(STEAM_USERS_PATH, vdf);
                }
                string steamExe = Path.Combine(STEAM_INSTALLTION_PATH, "Steam.exe");
                if (File.Exists(steamExe))
                { Process.Start(new ProcessStartInfo { FileName = steamExe, UseShellExecute = true }); Growl.InfoGlobal("Steam restarting -- login page will appear."); }
                else Growl.ErrorGlobal("Could not find Steam.exe.");
            }
            catch (Exception ex) { Growl.ErrorGlobal($"ForceLoginPage failed: {ex.Message}"); }
        }

        public void copy(string text) { try { Clipboard.SetText(text); } catch { } }

        // Helper: convert SteamID64 string to 32-bit user ID
        private static int SteamId64ToUserID32(string steamId64)
        {
            if (long.TryParse(steamId64, out long id64))
                return (int)(id64 - 76561197960265728L);
            return 0;
        }
    }

    // â"€â"€ Domain models â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    public class AppListResponse { public List<App>? Apps { get; set; } }
    public class App             { public int AppId { get; set; } public string Name { get; set; } = ""; }
    public class SteamGame       { public string AppID { get; set; } = ""; public string Name { get; set; } = ""; public bool isInstalled { get; set; } }
    public class UsersRoot       { public Dictionary<string, SteamUserEntry>? users { get; set; } }

    public class SteamUserEntry
    {
        public string? AccountName           { get; set; }
        public string? PersonaName           { get; set; }
        public string? RememberPassword      { get; set; }
        public string? WantsOfflineMode      { get; set; }
        public string? SkipOfflineModeWarning { get; set; }
        public string? AllowAutoLogin        { get; set; }
        public string? MostRecent            { get; set; }
        public string? Timestamp             { get; set; }
        public string? AccountID             { get; set; }
        public string? AvatarImage           { get; set; }
    }
}
