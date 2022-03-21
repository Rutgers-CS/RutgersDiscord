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
            _client.SelectMenuExecuted += DropdownTeamJoin;
            _client.ButtonExecuted += ButtonTeamRequest;
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

            TeamInfo userTeam = await _database.GetTeamByDiscordIDAsync((long)component.User.Id);
            if (userTeam != null)
            {
                await component.RespondAsync("player already in team", ephemeral: true);
                return;
            }

            switch (component.Data.CustomId)
            {
                case "register_team_create":
                    //Create new team
                    var newTeamModal = new ModalBuilder()
                        .WithTitle("New Team")
                        .WithCustomId("register_modal_teamcreate")
                        .AddTextInput("Team Name", "new_team_name");
                    await component.RespondWithModalAsync(newTeamModal.Build());
                    break;

                case "register_team_join":
                    //Pick existing team

                    IEnumerable<TeamInfo> current_teams = await _database.GetTeamByAttribute(player2: 0);
                    List<SelectMenuOptionBuilder> selection = new();

                    if(current_teams.Count() == 0)
                    {
                        await component.RespondAsync("No teams found", ephemeral: true);
                        return;
                    }

                    foreach (TeamInfo team in current_teams)
                    {
                        selection.Add(new SelectMenuOptionBuilder(label: team.TeamName, value: $"register_dropdown_{team.TeamID}"));
                    }
                    SelectMenuBuilder existingTeamSelect = new SelectMenuBuilder()
                        .WithPlaceholder("Select a team")
                        .WithCustomId("register_dropdown_jointeam")
                        .WithOptions(selection);

                    ComponentBuilder selectBuilder = new ComponentBuilder()
                        .WithSelectMenu(existingTeamSelect);
                    await component.RespondAsync("Select a team", components: selectBuilder.Build());
                    break;

                case "register_team_look":
                    //Flag as looking for team
                    await component.RespondAsync("No team");
                    break;
            }
        }

        public async Task RegisterFormHandler(SocketModal modal)
        {
            if(!modal.Data.CustomId.StartsWith("register_form"))
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

        public async Task RegisterTeamForm(SocketModal modal)
        {
            if (modal.Data.CustomId == "register_modal_teamcreate")
            {
                return;
            }

            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            string teamName = components.First(x => x.CustomId == "new_team_name").Value;

            if(teamName == "" || teamName == null)
            {
                await modal.RespondAsync("invalid team name", ephemeral: true);
                return;
            }

            Random r = new();
            TeamInfo team = new TeamInfo(r.Next(), teamName, (long)modal.User.Id, 0,null,null);
            await _database.AddTeamAsync(team);
            await modal.RespondAsync("team registered!");
        }

        public async Task DropdownTeamJoin(SocketMessageComponent component)
        {
            if(component.Data.CustomId != "register_dropdown_jointeam")
            {
                return;
            }

            await (await component.GetOriginalResponseAsync()).DeleteAsync();

            int teamID = int.Parse(component.Data.Values.First().Replace("register_dropdown_", ""));
            TeamInfo team = await _database.GetTeamAsync(teamID);

            if(team.Player2 != 0)
            {
                await component.RespondAsync("team is already full", ephemeral: true);
            }

            var builder = new ComponentBuilder()
                .WithButton("Accept", $"register_request_accept_{component.User.Id}")
                .WithButton("Reject", $"register_request_reject_{component.User.Id}");

            await (await _client.GetUser((ulong)team.Player1).CreateDMChannelAsync()).SendMessageAsync($"{component.User.Mention} requested to join your team");
        }

        public async Task ButtonTeamRequest(SocketMessageComponent component)
        {
            
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
