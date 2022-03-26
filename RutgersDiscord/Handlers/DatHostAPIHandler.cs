using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RutgersDiscord.Handlers
{
    public class DatHostAPIHandler
    {
        private readonly HttpClient _httpClient;
        private string templateServerID;

        public DatHostAPIHandler(HttpClient httpClient)
        {
            _httpClient = httpClient;

            templateServerID = Environment.GetEnvironmentVariable("templateServerID");
            string datHostEmail = Environment.GetEnvironmentVariable("datHostEmail");
            string datHostPassword = Environment.GetEnvironmentVariable("datHostPassword");

            _httpClient.BaseAddress = new Uri("https://dathost.net/api/0.1/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{datHostEmail}:{datHostPassword}")));
        }

        public async Task<ServerInfo> CreateNewServer()
        {
            var response = await _httpClient.PostAsync($"game-servers/{templateServerID}/duplicate", null);
            using (HttpContent content = response.Content)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(jsonString);
                return new ServerInfo(
                    (string)json.SelectToken("id"),
                    (string)json.SelectToken("raw_ip"),
                    int.Parse((string) json.SelectToken("ports.game")));
                
            }
        }

        public async Task<string> UpdateServerToken(string serverID, string serverName, string token)
        {
            var req = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("name", serverName),
                new KeyValuePair<string, string>("csgo_settings.steam_game_server_login_token", token)
            });

            var response = await _httpClient.PutAsync($"game-servers/{serverID}", req);
            using (HttpContent content = response.Content)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> CreateMatch(MatchSettings ms)
        {
            var req = ms.ToForm();
            var response =  await _httpClient.PostAsync("matches", req);
            using (HttpContent content = response.Content)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> DeleteServer(string serverID)
        {
            if (serverID == templateServerID) return "Cannot delete template";
            var response = await _httpClient.DeleteAsync($"game-servers/{serverID}");
            using (HttpContent content = response.Content)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
