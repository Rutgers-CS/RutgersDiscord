using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RutgersDiscord.Handlers
{
    public class RegistrationHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;

        public RegistrationHandler(DiscordSocketClient client, DatabaseHandler database, InteractivityService interactivity)
        {
            _client = client;
            _database = database;
            _interactivity = interactivity;
        }

        public void ListenDMButtons()
        {
            _client.ButtonExecuted += TeamButtonHandler;
            _client.ModalSubmitted += RegisterFormHandler;
        }

        public async Task SendDMButtons(SocketUser user)
        {
            var builder = new ComponentBuilder()
                .WithButton("Create a new team", "register_team_create")
                .WithButton("Join existing team", "register_team_join")
                .WithButton("Looking for team", "register_team_look");

            await (await user.CreateDMChannelAsync()).SendMessageAsync("test", components: builder.Build());
        }

        public async Task TeamButtonHandler(SocketMessageComponent component)
        {
            if(!component.Data.CustomId.StartsWith("register_team_"))
            {
                return;
            }

            IEnumerable<TeamInfo> current_teams = await _database.GetAllTeamsAsync();

            var newTeamModal = new ModalBuilder()
                .WithTitle("New Team")
                .WithCustomId("new_team_modal")
                .AddTextInput("Team Name", "new_team_name");

            var existingTeamSelect = new SelectMenuBuilder()
                .WithPlaceholder("Select a team")
                .WithCustomId("existing_team_selection")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (TeamInfo team in current_teams)
            {
                existingTeamSelect.AddOption(team.TeamName, $"register_modal_{team.TeamID}");
            }

            var selectBuilder = new ComponentBuilder()
                .WithSelectMenu(existingTeamSelect);

            switch (component.Data.CustomId)
            {
                case "register_create":
                    //Create new team
                    await component.RespondWithModalAsync(newTeamModal.Build());
                    break;
                case "register_join":
                    //Pick existing team
                    await component.RespondAsync("Select a team", components: selectBuilder.Build());
                    break;
                case "register_look":
                    //Flag as looking for team
                    await component.RespondAsync("No team");
                    break;
            }
        }

        public async Task RegisterFormHandler(SocketModal modal)
        {
            if(!modal.Data.CustomId.StartsWith("register_form_"))
            {
                return;
            }
            
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();

            string playerName = components.First(x => x.CustomId == "player_name").Value;
            string steamURL = components.First(x => x.CustomId == "steam_url").Value;

            long steamID64 = await GetSteamID64(steamURL);
            string steamID = "";
            if (steamID64 == 0)
            {
                await modal.RespondAsync("Registration Failed. Please verify the link to your steam profile and resubmit.");
            }
            else
            {
                steamID = SteamID64ToSteamID(steamID64);
            }

            PlayerInfo player = new((long)modal.User.Id, steamID64, steamID, playerName);
            await _database.AddPlayerAsync(player);

            await SendDMButtons(modal.User);

            await modal.RespondAsync("Registration Succeeded. Please check your DMs to pick a team.");
        }

        private static async Task<long> GetSteamID64(string url)
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
                    return 0;
                }
            }
            else
            {
                steamID64 = long.Parse(trimmedURL);
            }

            return steamID64;
        }

        private string SteamID64ToSteamID(long steamID64)
        {
            var accountIDLowBit = steamID64 & 1;
            var accountIDHighBit = (steamID64 >> 1) & 0x7FFFFFF;

            string steamID = "STEAM_1" + ":" + accountIDLowBit + ":" + accountIDHighBit;
            return steamID;
        }
    }
}
