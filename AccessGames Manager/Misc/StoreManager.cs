using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AccessGamesManager.Misc
{
    public enum StorePlatform
    {
        PC,
        PlayStation,
        Xbox
    }

    public enum OfferStatus
    {
        Available,
        SoldOut,
        ComingSoon
    }

    public class StoreOffer
    {
        public string        Id            { get; set; } = Guid.NewGuid().ToString();
        public string        Title         { get; set; } = "";
        public StorePlatform Platform      { get; set; } = StorePlatform.PC;
        public OfferStatus   Availability  { get; set; } = OfferStatus.Available;
        public decimal       Price         { get; set; } = 0;
        public string        Currency      { get; set; } = "MAD";
        public string?       Genre         { get; set; }
        public string?       CoverUrl      { get; set; }
        public string?       Description   { get; set; }
        public string?       StoreUrl      { get; set; }  // clickable link
        public List<string>  Tags          { get; set; } = new();
        public bool          IsHighlighted { get; set; } = false; // shown in featured banner
        public DateTime      DateAdded     { get; set; } = DateTime.UtcNow;
    }

    public static class StoreManager
    {
        // Local cache (offline fallback + write-through)
        private static readonly string _localPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccessGamesManager", "store_cache.json");

        private static List<StoreOffer> _offers = new();

        public static IReadOnlyList<StoreOffer> Offers => _offers;

        /// <summary>
        /// Timestamp of the last successful remote fetch (null = never fetched).
        /// </summary>
        public static DateTime? LastFetched { get; private set; }

        // ── Load ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Tries GitHub first, falls back to local cache, falls back to empty.
        /// Call this once on startup (fire-and-forget or awaited).
        /// </summary>
        public static async Task LoadAsync()
        {
            string? remoteJson = await GitHubStoreSync.FetchRawAsync();

            if (remoteJson != null)
            {
                try
                {
                    var parsed = JsonConvert.DeserializeObject<List<StoreOffer>>(remoteJson) ?? new();
                    _offers     = parsed;
                    LastFetched = DateTime.UtcNow;
                    // Write-through to local cache
                    WriteLocalCache();
                    return;
                }
                catch { /* fall through to local cache */ }
            }

            // Offline / fetch failed — use local cache
            LoadLocalCache();
        }

        /// <summary>
        /// Synchronous local-only load (used when async isn't available).
        /// </summary>
        public static void Load() => LoadLocalCache();

        private static void LoadLocalCache()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_localPath)!);
                if (File.Exists(_localPath))
                    _offers = JsonConvert.DeserializeObject<List<StoreOffer>>(
                        File.ReadAllText(_localPath)) ?? new();
            }
            catch { _offers = new(); }
        }

        private static void WriteLocalCache()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_localPath)!);
                File.WriteAllText(_localPath, Serialize());
            }
            catch { }
        }

        // ── CRUD ─────────────────────────────────────────────────────────────

        public static void Add(StoreOffer offer)
        {
            _offers.Add(offer);
            WriteLocalCache();
        }

        public static void Update(StoreOffer offer)
        {
            int idx = _offers.FindIndex(o => o.Id == offer.Id);
            if (idx >= 0) _offers[idx] = offer;
            else          _offers.Add(offer);
            WriteLocalCache();
        }

        public static void Remove(string id)
        {
            _offers.RemoveAll(o => o.Id == id);
            WriteLocalCache();
        }

        // ── GitHub push ──────────────────────────────────────────────────────

        /// <summary>
        /// Serializes current offers and pushes them to GitHub.
        /// Returns true on success.
        /// </summary>
        public static async Task<bool> PushToGitHubAsync()
        {
            int count   = _offers.Count;
            string msg  = $"chore: update store — {count} offer{(count == 1 ? "" : "s")}";
            return await GitHubStoreSync.PushAsync(Serialize(), msg);
        }

        public static string Serialize()
            => JsonConvert.SerializeObject(_offers, Formatting.Indented);
    }
}
