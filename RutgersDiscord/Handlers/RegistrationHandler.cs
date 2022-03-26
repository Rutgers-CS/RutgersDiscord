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

        public void SubscribeHandlers()
        {
            _client.ButtonExecuted += RegistrationButtonHandler;
            _client.ButtonExecuted += TeamOptionsButtonsHandler;
            _client.ModalSubmitted += RegistrationFormHandler;
            _client.SelectMenuExecuted += JoinTeamFormHandler;
            _client.ButtonExecuted += JoinRequestHandler;
            _client.ModalSubmitted += CreateTeamFormHandler;
        }

        public async Task RegistrationButtonHandler(SocketMessageComponent component)
        {
            if (component.Data.CustomId != "spawn_registration_form")
            {
                return;
            }

            var modalBuilder = new ModalBuilder()
                .WithTitle("Event Registration")
                .WithCustomId("register_form")
                .AddTextInput("Your Name:", "player_name", required: true, placeholder: "Gabe Newell", style: TextInputStyle.Short, minLength: 3, maxLength: 40)
                .AddTextInput("Link to Steam profile:", "steam_url", required: true, placeholder: "https://steamcommunity.com/id/gaben", style: TextInputStyle.Short, minLength: 22)
                ;

            await component.RespondWithModalAsync(modalBuilder.Build());
        }

        public async Task RegistrationFormHandler(SocketModal modal)
        {
            if (!modal.Data.CustomId.Equals("register_form"))
            {
                return;
            }

            if (await _database.GetPlayerAsync((long)modal.User.Id) != null)
            {
                await modal.RespondAsync("You are already registered. Check your DMs to pick a team.", ephemeral: true);
                return;
            }

            List<SocketMessageComponentData> components = modal.Data.Components.ToList();

            string playerName = components.First(x => x.CustomId == "player_name").Value;
            string steamURL = components.First(x => x.CustomId == "steam_url").Value;

            long steamID64 = await GetSteamID64(steamURL);
            string steamID = "";
            if (steamID64 == 0)
            {
                await modal.RespondAsync("Registration Failed. Please verify the link to your steam profile and resubmit.", ephemeral: true);
                return;
            }
            else
            {
                steamID = SteamID64ToSteamID(steamID64);
            }

            PlayerInfo player = new((long)modal.User.Id, steamID64, steamID, playerName,null,0,0);
            int status = await _database.AddPlayerAsync(player);

            if (status == 0)
            {
                await modal.RespondAsync("Registration Failed. You are already registered.", ephemeral: true);
            }
            else
            {
                await TeamOptionsButtons(modal.User);
                await (modal.User as IGuildUser).AddRoleAsync(Constants.Role.scarletClassic);
                await modal.RespondAsync("Registration Succeeded. Please check your DMs to pick a team.", ephemeral: true);
            }
        }

        public async Task TeamOptionsButtons(SocketUser user)
        {
            var builder = new ComponentBuilder()
                .WithButton("Create a New Team", "team_options_create", emote: new Emoji("\U0001F4DD"))
                .WithButton("Join Existing Team", "team_options_join", emote: new Emoji("\U0001F4F2"))
                .WithButton("Looking for Team", "team_options_look", emote: new Emoji("\U0001F50E"));

            await (await user.CreateDMChannelAsync()).SendMessageAsync($"Click a Button Below To Finish Your Registration, Reach out to one of us ({_client.GetUser(Constants.Users.galifi).Mention}, {_client.GetUser(Constants.Users.open).Mention}, {_client.GetUser(Constants.Users.kenji).Mention}) if you have any issues.", components: builder.Build());
        }

        public async Task TeamOptionsButtonsHandler(SocketMessageComponent component)
        {
            if (!component.Data.CustomId.StartsWith("team_options"))
            {
                return;
            }

            TeamInfo userTeam = await _database.GetTeamByDiscordIDAsync((long)component.User.Id);
            if (userTeam != null)
            {
                await component.RespondAsync("You are already on a team.", ephemeral: true);
                return;
            }

            switch (component.Data.CustomId)
            {
                case "team_options_create":
                    var newTeamCreateModal = new ModalBuilder()
                        .WithTitle("Create a New Team")
                        .WithCustomId("modal_team_create")
                        .AddTextInput("Team Name", "new_team_name", TextInputStyle.Short, "Kenji Peek", 4, 32, true);
                    await component.RespondWithModalAsync(newTeamCreateModal.Build());
                    break;
                case "team_options_join":
                    IEnumerable<TeamInfo> current_teams = await _database.GetTeamByAttribute(player2: 0);
                    List<SelectMenuOptionBuilder> selection = new();

                    if (current_teams.Count() == 0)
                    {
                        await component.RespondAsync("No teams created yet. Why don't you make one?", ephemeral: true);
                        return;
                    }

                    foreach (TeamInfo team in current_teams)
                    {
                        selection.Add(new SelectMenuOptionBuilder(team.TeamName, $"teams_dropdown_{team.TeamID}", $"{_client.GetUser((ulong)team.Player1).Username}"));
                    }
                    SelectMenuBuilder existingTeamSelect = new SelectMenuBuilder()
                        .WithPlaceholder("Select a Team")
                        .WithCustomId("teams_dropdown_jointeam")
                        .WithOptions(selection);

                    ComponentBuilder selectBuilder = new ComponentBuilder()
                        .WithSelectMenu(existingTeamSelect);

                    await component.RespondAsync("Select a Team", components: selectBuilder.Build());
                    break;
                case "team_options_look":
                    await _client.GetGuild(Constants.guild).GetTextChannel(Constants.Channels.scGeneral).SendMessageAsync($"{component.User.Mention} has registed for Scarlet Classic Wingman 2V2 and is looking for a team.");
                    await component.RespondAsync($"We've marked you as looking for a team. Try asking around in sc-general or dm {_client.GetUser(Constants.Users.galifi).Mention} for help finding a partner.");
                    break;
            }
        }

        //Form to create team
        public async Task CreateTeamFormHandler(SocketModal modal)
        {
            if (modal.Data.CustomId != "modal_team_create")
            {
                return;
            }

            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            string teamName = components.First(x => x.CustomId == "new_team_name").Value;

            teamName = teamName.Trim();

            if (teamName == "" || teamName == null)
            {
                await modal.RespondAsync("Invalid team name. Try again.", ephemeral: true);
                return;
            }

            //Horrible but we should check for duplicates
            IEnumerable<TeamInfo> currentTeams = await _database.GetAllTeamsAsync();
            List<string> teamNames = new List<string>();
            foreach (TeamInfo t in currentTeams)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var c in t.TeamName.ToLower())
                {
                    if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                        sb.Append(c);
                }
                teamNames.Add(sb.ToString());
            }

            StringBuilder sb2 = new StringBuilder();
            foreach (var c in teamName.ToLower())
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                    sb2.Append(c);
            }
            string cmp = sb2.ToString();

            if (teamNames.Contains(cmp))
            {
                await modal.RespondAsync("Team name already exists.", ephemeral: true);
                return;
            }

            Random r = new();
            TeamInfo team = new TeamInfo(r.Next(), teamName, (long)modal.User.Id,0,0,0,0,0);
            await _database.AddTeamAsync(team);
            PlayerInfo player = await _database.GetPlayerAsync((long)modal.User.Id);
            player.TeamID = team.TeamID;
            await _database.UpdatePlayerAsync(player);
            await modal.RespondAsync($"{team.TeamName} created. Now have your partner join.");
        }

        //Selection to join team
        public async Task JoinTeamFormHandler(SocketMessageComponent component)
        {
            if (component.Data.CustomId != "teams_dropdown_jointeam")
            {
                return;
            }

            int teamID = int.Parse(component.Data.Values.First().Replace("teams_dropdown_", ""));
            TeamInfo team = await _database.GetTeamAsync(teamID);

            if (team.Player2 != 0)
            {
                await component.RespondAsync("Team already has two players.", ephemeral: true);
                return;
            }

            var builder = new ComponentBuilder()
            .WithButton("Accept", $"teams_request_accept_{component.User.Id}", ButtonStyle.Success)
            .WithButton("Reject", $"teams_request_reject_{component.User.Id}", ButtonStyle.Danger);

            await (await _client.GetUser((ulong)team.Player1).CreateDMChannelAsync()).SendMessageAsync($"{component.User.Mention} has requested to join your team.", components: builder.Build());
            await component.UpdateAsync(x => { x.Content = $"We've sent a request to join {team.TeamName} to {_client.GetUser((ulong)team.Player1).Mention}"; x.Components = new ComponentBuilder().Build(); });

        }

        public async Task JoinRequestHandler(SocketMessageComponent component)
        {
            if (!component.Data.CustomId.StartsWith("teams_request_"))
            {
                return;
            }

            if (component.Data.CustomId.StartsWith("teams_request_accept_"))
            {
                ulong userID = ulong.Parse(component.Data.CustomId.Replace("teams_request_accept_", ""));
                TeamInfo team = (await _database.GetTeamByAttribute(player1: (long?)component.User.Id)).First();
                if (team.Player2 != 0)
                {
                    await component.RespondAsync("Your team is already full.", ephemeral: true);
                    return;
                }
                team.Player2 = (long)userID;
                PlayerInfo player = await _database.GetPlayerAsync((long)userID);
                player.TeamID = team.TeamID;
                await _database.UpdatePlayerAsync(player);
                await _database.UpdateTeamAsync(team);
                await (await _client.GetUser(userID).CreateDMChannelAsync()).SendMessageAsync($"You have been accepted into {team.TeamName}");
                await component.UpdateAsync(x => { x.Content = $"{_client.GetUser(userID).Mention} has been accepted onto team {team.TeamName}."; x.Components = new ComponentBuilder().Build(); });
            }
            else
            {
                ulong userID = ulong.Parse(component.Data.CustomId.Replace("teams_request_reject_", ""));
                TeamInfo team = (await _database.GetTeamByAttribute(player1: (long?)component.User.Id)).First();
                await component.UpdateAsync(x => { x.Content = $"You have rejected {_client.GetUser(userID).Mention}"; x.Components = new ComponentBuilder().Build(); });
            }
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