using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Interactions;
using Discord;
using System.Text;
using System.Threading.Tasks;
using Interactivity;
using RutgersDiscord.Handlers;
using Discord;
using System.Net.Http;
using Newtonsoft.Json;

namespace RutgersDiscord.Commands.User
{
    public class RegisterCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;

        public RegisterCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
        }

        public async Task RegistrationForm()
        {
            var modalBuilder = new ModalBuilder()
                .WithTitle("Event Registration")
                .WithCustomId("registration_form")
                .AddTextInput("Your Name:", "player_name", required: true)
                .AddTextInput("Link to Steam profile:", "steam_url", required: true)
                .AddTextInput("Your team name:", "team_name", required: true)
                ;

            await _context.Interaction.RespondWithModalAsync(modalBuilder.Build());

            _client.ModalSubmitted += async modal =>
            {
                List<SocketMessageComponentData> components = modal.Data.Components.ToList();

                //TODO: Replace with database connection
                string playerName = components.First(x => x.CustomId == "player_name").Value;
                string steamURL = components.First(x => x.CustomId == "steam_url").Value;
                string teamName = components.First(x => x.CustomId == "team_name").Value;

                string steamID = ResolveSteamID(steamURL).Result;

                string msg = $"{playerName} with {steamID} on {teamName}";

                await modal.RespondAsync(msg);
            };
        }

        public async Task<string> ResolveSteamID(string url)
        {
            string trimmedURL = url.Replace("steamcommunity.com/id/", "").Replace("steamcommunity.com/profiles/", "").Replace("https://", "").Replace("/", "");
            long steamID64;

            bool vanityURL = true;
            if (trimmedURL.All(char.IsDigit)) vanityURL = false;

            if (vanityURL)
            {
                string requestUrl = $"http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={Environment.GetEnvironmentVariable("steamWebAPIToken")}&vanityurl=" + trimmedURL;
                HttpClient steamAPIClient = new HttpClient();
                HttpResponseMessage response = await steamAPIClient.GetAsync(requestUrl);
                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic deserializedResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);

                if (deserializedResponse.response.success == "1")
                {
                    string steamid = deserializedResponse.response.steamid;
                    steamID64 = long.Parse(steamid);
                }
                else
                {
                    return "error";
                }
            }
            else
            {
                steamID64 = long.Parse(trimmedURL);
            }

            var accountIDLowBit = steamID64 & 1;
            var accountIDHighBit = (steamID64 >> 1) & 0x7FFFFFF;

            string steamID = "STEAM_1" + ":" + accountIDLowBit + ":" + accountIDHighBit;
            return steamID;
        }
    }

}
