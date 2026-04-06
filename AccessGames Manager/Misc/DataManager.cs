using HandyControl.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AccessGamesManager.Misc
{
    public class DataManager
    {
        public static string DataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccessGamesManager");
        public static string Datajson   = Path.Combine(DataFolder, "Data.json");
        public static string ImageFolder = Path.Combine(DataFolder, "Images");

        public static void FoldersIntegrityChecker()
        {
            Directory.CreateDirectory(DataFolder);
            Directory.CreateDirectory(ImageFolder);
        }

        public static void SaveData(Account entry)
        {
            string fullPath = Datajson;

            // Ensure the file exists with valid JSON before reading
            if (!File.Exists(fullPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                File.WriteAllText(fullPath, "[]");
            }

            int retries = 0;
            const int maxRetries = 3;
            bool saved = false;

            while (!saved && retries < maxRetries)
            {
                try
                {
                    List<Account> entries;
                    try
                    {
                        string jsonData = File.ReadAllText(fullPath);
                        entries = JsonConvert.DeserializeObject<List<Account>>(jsonData)
                                  ?? new List<Account>();
                    }
                    catch (JsonException ex)
                    {
                        Growl.Error($"Failed to deserialize existing data: {ex.Message}");
                        entries = new List<Account>();
                    }

                    int index = entries.FindIndex(e => e.Username == entry.Username);
                    if (index != -1)
                        entries[index] = entry;
                    else
                        entries.Add(entry);

                    File.WriteAllText(fullPath, JsonConvert.SerializeObject(entries, Formatting.Indented));
                    saved = true;
                }
                catch (IOException)
                {
                    retries++;
                    System.Threading.Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Growl.Error("SaveData error: " + ex.Message);
                    break;
                }
            }

            if (!saved)
                Growl.Error("Failed to save data. File may be locked by another process.");
        }

        public static void ConvertOldEntriesToNewFormat()
        {
            if (!File.Exists(Datajson))
                throw new FileNotFoundException($"The file {Datajson} does not exist.");

            string json = File.ReadAllText(Datajson);
            try
            {
                var oldAccounts = JsonConvert.DeserializeObject<List<OldAccount>>(json);
                if (oldAccounts == null || oldAccounts.Count == 0) { Console.WriteLine("No accounts found."); return; }

                var newAccounts = new List<Account>();
                foreach (var old in oldAccounts)
                {
                    var newAcc = new Account { Username = old.Username, Password = old.Password, IsUnlocked = old.IsUnlocked, GamesList = new List<Game>() };
                    if (old.GamesList != null)
                        foreach (var g in old.GamesList)
                            newAcc.GamesList.Add(new Game { Name = g.Name, Username = g.Username, Password = g.Password, ImageUrl = g.ImageUrl, ImageCach = g.ImageCach });
                    newAccounts.Add(newAcc);
                }

                File.WriteAllText(Datajson, JsonConvert.SerializeObject(newAccounts, Formatting.Indented));
                Console.WriteLine("Conversion completed successfully.");
            }
            catch (JsonException ex) { Console.WriteLine($"JSON parsing error: {ex.Message}"); }
            catch (Exception ex)     { Console.WriteLine($"Unexpected error: {ex.Message}"); }
        }

        public class OldAccount
        {
            public string? Username   { get; set; }
            public string? Password   { get; set; }
            [JsonProperty("isUmlocked")]
            public bool?   IsUnlocked { get; set; }
            public List<OldGame>? GamesList { get; set; }
        }

        public class OldGame
        {
            public string? Name     { get; set; }
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? ImageUrl { get; set; }
            public string? ImageCach { get; set; }
        }

        public static void AssignAccountCredentialsToGames(List<Account> accounts)
        {
            if (accounts == null || accounts.Count == 0) { Growl.Warning("No accounts found."); return; }

            foreach (var acc in accounts)
            {
                if (acc?.GamesList == null) continue;
                foreach (var game in acc.GamesList)
                {
                    if (game == null) continue;
                    game.Username = acc.Username;
                    game.Password = acc.Password;
                }
            }

            File.WriteAllText(Datajson, JsonConvert.SerializeObject(accounts, Formatting.Indented));
            Growl.Success("Successfully assigned account credentials to games.");
        }

        public static List<Account> LoadData()
        {
            if (!File.Exists(Datajson))
            {
                Growl.Warning("Data file not found.");
                return new List<Account>();
            }

            try
            {
                string json = File.ReadAllText(Datajson);
                return JsonConvert.DeserializeObject<List<Account>>(json) ?? new List<Account>();
            }
            catch (JsonException ex) { Growl.Info($"Failed to parse JSON data: {ex.Message}"); }
            catch (Exception ex)     { Growl.Error($"Unexpected error loading data: {ex.Message}"); }
            return new List<Account>();
        }

        public static Account? GetAccountByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) { Growl.Warning("Username cannot be empty."); return null; }

            var accounts = LoadData();
            if (accounts.Count == 0) { Growl.Warning("No accounts found in data file."); return null; }

            var account = accounts.FirstOrDefault(acc =>
                acc.Username != null && acc.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (account == null)
                Growl.Warning($"Account '{username}' not found.");

            return account;
        }

        public static void RemoveData(Account accountToRemove)
        {
            if (!File.Exists(Datajson)) { Growl.Error("Data file does not exist."); return; }

            try
            {
                string jsonData = File.ReadAllText(Datajson);
                var entries = JsonConvert.DeserializeObject<List<Account>>(jsonData) ?? new List<Account>();
                int index = entries.FindIndex(e => e.Username == accountToRemove.Username);
                if (index == -1) { Growl.Error("Account not found: " + accountToRemove.Username); return; }
                entries.RemoveAt(index);
                File.WriteAllText(Datajson, JsonConvert.SerializeObject(entries, Formatting.Indented));
            }
            catch (JsonException ex) { Growl.Error($"Deserialize error: {ex.Message}"); }
            catch (IOException ex)   { Growl.Error($"IO error: {ex.Message}"); }
        }
    }

    // ── Domain models ────────────────────────────────────────────────────────────

    public class Account
    {
        public string?     Username   { get; set; }
        public string?     Password   { get; set; }
        /// <summary>Serialised as "isUmlocked" to preserve existing JSON files.</summary>
        [JsonProperty("isUmlocked")]
        public bool?       IsUnlocked { get; set; }
        public List<Game>? GamesList  { get; set; }
    }

    public class Game
    {
        public string? Name     { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? AppiD    { get; set; }
        public string? GFNlaunch { get; set; }
        public string? ImageUrl  { get; set; }
        public string? ImageCach { get; set; }
    }

    public class GameAccount
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
