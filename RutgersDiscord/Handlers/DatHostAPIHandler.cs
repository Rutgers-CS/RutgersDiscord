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
    public class DatHostAPIHandler
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigHandler _config;
        private string templateServerID;

        public DatHostAPIHandler(HttpClient httpClient, ConfigHandler config)
        {
            _httpClient = httpClient;
            _config = config;
            templateServerID = _config.settings.DatHostSettings.TemplateServerID;
            string datHostEmail = _config.settings.DatHostSettings.DatHostEmail;
            string datHostPassword = _config.settings.DatHostSettings.DatHostPassword;

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
                Console.WriteLine(jsonString);
                JObject json = JObject.Parse(jsonString);
                return new ServerInfo(
                    (string)json.SelectToken("id"),
                    (string)json.SelectToken("raw_ip"),
                    int.Parse((string) json.SelectToken("ports.game")));
                
            }
        }

        public async Task<string> SyncFiles(string serverID)
        {
            var response = await _httpClient.PostAsync($"game-servers/{templateServerID}/sync-files", null);
            using (HttpContent content = response.Content)
            {
                return await response.Content.ReadAsStringAsync();
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

        public async Task GetDemo(string serverID, string matchID)
        {
            if (serverID == templateServerID) return;
            var response = await _httpClient.GetAsync($"game-server/{serverID}/files/{matchID}.dem");
            using (var fs = new FileStream($"./demo_{matchID}.dem", FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
            }
        }
    }
}
