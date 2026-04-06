using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AccessGamesManager.Misc
{
    public static class RawgApi
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<string?> GetGameImageByName(string gameName)
        {
            string apiKey = AppSecrets.RawgApiKey;
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("RAWG API key is missing. Add RawgApiKey to secrets.json.");
                return null;
            }

            try
            {
                string requestUrl = $"https://api.rawg.io/api/games?key={apiKey}&page_size=1&search={Uri.EscapeDataString(gameName)}";
                var response = await httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonObject   = JObject.Parse(jsonResponse);
                var results      = jsonObject["results"];

                if (results != null && results.HasValues)
                    return results[0]?["background_image"]?.ToString();

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching game image: {ex.Message}");
                return null;
            }
        }
    }
}
