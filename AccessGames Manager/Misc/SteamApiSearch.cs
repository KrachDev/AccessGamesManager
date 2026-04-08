using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AccessGamesManager.Misc
{
    public class SteamSearchResult
    {
        [JsonProperty("id")]
        public int AppId { get; set; }

        [JsonProperty("name")]
        public string Title { get; set; } = "";

        [JsonProperty("tiny_image")]
        public string TinyImage { get; set; } = "";
    }

    public class SteamSearchResponse
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("items")]
        public List<SteamSearchResult> Items { get; set; } = new();
    }

    public static class SteamApiSearch
    {
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        /// <summary>
        /// Searches the Steam Store for games matching the query.
        /// </summary>
        public static async Task<List<SteamSearchResult>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<SteamSearchResult>();

            try
            {
                string url = $"https://store.steampowered.com/api/storesearch/?term={Uri.EscapeDataString(query)}&l=english&cc=US";
                string json = await _http.GetStringAsync(url);

                var response = JsonConvert.DeserializeObject<SteamSearchResponse>(json);
                return response?.Items ?? new List<SteamSearchResult>();
            }
            catch
            {
                return new List<SteamSearchResult>();
            }
        }

        /// <summary>
        /// Gets the high-res 600x900 library cover URL for a given AppID.
        /// </summary>
        public static string GetLibraryCoverUrl(int appId)
        {
            return $"https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/{appId}/library_600x900.jpg";
        }
    }
}
