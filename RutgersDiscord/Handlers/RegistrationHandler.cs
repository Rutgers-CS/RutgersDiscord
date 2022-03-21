using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        public async Task SendDMButtons(SocketInteractionContext context)
        {
            var builder = new ComponentBuilder()
                .WithButton("Create a new team", "register_create")
                .WithButton("Join existing team", "register_join")
                .WithButton("Looking for team", "register_look");

            await (await context.User.CreateDMChannelAsync()).SendMessageAsync("test", components: builder.Build());
        }

        public async Task TeamButtonHandler(SocketMessageComponent component)
        {
            if(!component.Data.CustomId.StartsWith("register_"))
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
    }
}
