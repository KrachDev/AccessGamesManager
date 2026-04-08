using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace AccessGamesManager.Misc
{
    public enum PlayStatus
    {
        Backlog,
        Playing,
        Completed,
        Dropped,
        Wishlist
    }

    public enum GamePlatform
    {
        Steam,
        EpicGames,
        GOG,
        Ubisoft,
        EAApp,
        Xbox,
        PlayStation,
        Nintendo,
        Other
    }

    public class CatalogueEntry
    {
        public string  Id          { get; set; } = Guid.NewGuid().ToString();
        public string  Title       { get; set; } = "";
        public GamePlatform Platform   { get; set; } = GamePlatform.Steam;
        public PlayStatus   Status     { get; set; } = PlayStatus.Backlog;
        public int     Rating      { get; set; } = 0;   // 0 = unrated, 1-5
        public string? Notes       { get; set; }
        public string? CoverUrl    { get; set; }        // local file path or URL
        public string? Genre       { get; set; }
        public DateTime DateAdded  { get; set; } = DateTime.UtcNow;
        public DateTime? DateCompleted { get; set; }
        public int     HoursPlayed { get; set; } = 0;
    }

    public static class CatalogueManager
    {
        private static readonly string _path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccessGamesManager", "catalogue.json");

        private static List<CatalogueEntry> _entries = new();

        public static IReadOnlyList<CatalogueEntry> Entries => _entries;

        public static void Load()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                if (File.Exists(_path))
                    _entries = JsonConvert.DeserializeObject<List<CatalogueEntry>>(File.ReadAllText(_path)) ?? new();
            }
            catch { _entries = new(); }
        }

        private static void Save()
        {
            try { File.WriteAllText(_path, JsonConvert.SerializeObject(_entries, Formatting.Indented)); }
            catch { }
        }

        public static void Add(CatalogueEntry entry)
        {
            _entries.Add(entry);
            Save();
        }

        public static void Update(CatalogueEntry entry)
        {
            int idx = _entries.FindIndex(e => e.Id == entry.Id);
            if (idx >= 0) _entries[idx] = entry;
            else          _entries.Add(entry);
            Save();
        }

        public static void Remove(string id)
        {
            _entries.RemoveAll(e => e.Id == id);
            Save();
        }
    }
}
