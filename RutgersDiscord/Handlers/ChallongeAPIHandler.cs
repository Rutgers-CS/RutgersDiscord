using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RutgersDiscord.Handlers
{
    public class ChallongeAPIHandler
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigHandler _config;

        public ChallongeAPIHandler(HttpClient httpClient, ConfigHandler config)
        {
            _httpClient = httpClient;
            _config = config;

            string challongeUser = _config.settings.ChallongeSettings.ChallongeUsername;
            string challongeKey = _config.settings.ChallongeSettings.ChallongeAPIKey;
            string tournamentID = _config.settings.ChallongeSettings.ChallongeTournamentID;

            _httpClient.BaseAddress = new Uri($"https://api.challonge.com/v1/tournaments/{tournamentID}/matches/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{challongeUser}:{challongeKey}")));
        }

        public async Task<string> MarkLive(string challongeMatchID)
        {
            var response = await _httpClient.PostAsync($"{challongeMatchID}/mark_as_underway.json", null);
            using (HttpContent content = response.Content)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        //TODO Check
        public async Task<string> UpdateScore(string challongeMatchID, string score)
        {
            var req = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("match[scores_csv]", score),
            });

            var response = await _httpClient.PutAsync($"{challongeMatchID}.json", req);
            using (HttpContent content = response.Content)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
