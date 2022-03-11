using CoreRCON;
using Discord;
using Interactivity;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RutgersDiscord.Commands.User;
using RutgersDiscord.Handlers;
using RutgersDiscord.Commands;

namespace RutgersDiscord.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractivityService _interactivity;
        private readonly IServiceProvider _services;
        private readonly DatabaseHandler _database;

        public InteractionModule(DiscordSocketClient client, InteractivityService interactivity, DatabaseHandler database, IServiceProvider services)
        {
            _client = client;
            _interactivity = interactivity;
            _database = database;
            _services = services;
        }

        [SlashCommand("echo", "Echo an input", runMode: RunMode.Async)]
        public async Task Echo(string input)
        {
            await RespondAsync(input);
        }

        [SlashCommand("veto", "Starts veto process", runMode: RunMode.Async)]
        public async Task Veto()
        {
            VetoCommand v = new VetoCommand(_client, Context, _database, _interactivity);
            await v.StartVeto();

        [SlashCommand("register", "Provide required information to register for the event")]
        public async Task Register()
        {
            RegisterCommand rc = new RegisterCommand(_client, Context, _database, _interactivity);
            rc.RegistrationForm();
        }

        [SlashCommand("ready", "Set your team as ready for the match")]
        public async Task TeamReady()
        {
            ReadyCommand rc = new ReadyCommand(_client, Context, _database, _interactivity);
            rc.Ready();
        }

        [SlashCommand("unready", "Set your team as not ready for the match")]
        public async Task TeamUnReady()
        {
            UnReadyCommand urc = new UnReadyCommand(_client, Context, _database, _interactivity);
            urc.UnReady();
        }

        [SlashCommand("admin", "Notify an admin")]
        public async Task NotifyAdmin()
        {
            //Ping @guihori, @0p7day, @galifi in eboard or smth
        }

        [SlashCommand("leaderboard", "Display the tournament leaderboard")]
        public async Task DisplayLeaderboard()
        {
            //TODO
        }

        [SlashCommand("teamselction", "Select or create a team")]
        public async Task TeamSelection()
        {
            var builder = new ComponentBuilder()
                .WithButton("Create a new team", "new_team")
                .WithButton("Join existing team", "join_team")
                .WithButton("Looking for team", "no_team")
                ;

            await ReplyAsync("test", components: builder.Build());
            _client.ButtonExecuted += TeamButtonHandler;
        }

        public async Task TeamButtonHandler(SocketMessageComponent component)
        {
            List<string> current_teams = new List<string>();
            current_teams.Add("test1");
            current_teams.Add("test2");

            var newTeamModal = new ModalBuilder()
                .WithTitle("New Team")
                .WithCustomId("new_team_modal")
                .AddTextInput("Team Name", "new_team_name");

            var existingTeamSelect = new SelectMenuBuilder()
                .WithPlaceholder("Select a team")
                .WithCustomId("existing_team_selection")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (string team in current_teams)
            {
                existingTeamSelect.AddOption(team.ToString(), team.ToString());
            }

            var selectBuilder = new ComponentBuilder()
                .WithSelectMenu(existingTeamSelect);

            switch(component.Data.CustomId)
            {
                case "new_team":
                    //Create new team
                    await component.RespondWithModalAsync(newTeamModal.Build());
                    break;
                case "join_team":
                    //Pick existing team
                    await component.RespondAsync("Select a team", components: selectBuilder.Build());
                    break;
                case "no_team":
                    //Flag as looking for team
                    await component.RespondAsync("No team");
                    break;
            }
        }

        [SlashCommand("register", "Provide required information to register for the event")]
        public async Task Register()
        {
            RegisterCommand rc = new RegisterCommand(_client, Context, _database, _interactivity);
            rc.RegistrationForm();
        }

        [SlashCommand("ready", "Set your team as ready for the match")]
        public async Task TeamReady()
        {
            ReadyCommand rc = new ReadyCommand(_client, Context, _database, _interactivity);
            rc.Ready();
        }

        [SlashCommand("unready", "Set your team as not ready for the match")]
        public async Task TeamUnReady()
        {
            UnReadyCommand urc = new UnReadyCommand(_client, Context, _database, _interactivity);
            urc.UnReady();
        }

        [SlashCommand("admin", "Notify an admin")]
        public async Task NotifyAdmin()
        {
            //Ping @guihori, @0p7day, @galifi in eboard or smth
        }

        [SlashCommand("leaderboard", "Display the tournament leaderboard")]
        public async Task DisplayLeaderboard()
        {
            //TODO
        }

        [SlashCommand("teamselction", "Select or create a team")]
        public async Task TeamSelection()
        {
            var builder = new ComponentBuilder()
                .WithButton("Create a new team", "new_team")
                .WithButton("Join existing team", "join_team")
                .WithButton("Looking for team", "no_team")
                ;

            await ReplyAsync("test", components: builder.Build());
            _client.ButtonExecuted += TeamButtonHandler;
        }

        public async Task TeamButtonHandler(SocketMessageComponent component)
        {
            List<string> current_teams = new List<string>();
            current_teams.Add("test1");
            current_teams.Add("test2");

            var newTeamModal = new ModalBuilder()
                .WithTitle("New Team")
                .WithCustomId("new_team_modal")
                .AddTextInput("Team Name", "new_team_name");

            var existingTeamSelect = new SelectMenuBuilder()
                .WithPlaceholder("Select a team")
                .WithCustomId("existing_team_selection")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (string team in current_teams)
            {
                existingTeamSelect.AddOption(team.ToString(), team.ToString());
            }

            var selectBuilder = new ComponentBuilder()
                .WithSelectMenu(existingTeamSelect);

            switch(component.Data.CustomId)
            {
                case "new_team":
                    //Create new team
                    await component.RespondWithModalAsync(newTeamModal.Build());
                    break;
                case "join_team":
                    //Pick existing team
                    await component.RespondAsync("Select a team", components: selectBuilder.Build());
                    break;
                case "no_team":
                    //Flag as looking for team
                    await component.RespondAsync("No team");
                    break;
            }
        }
    }
}