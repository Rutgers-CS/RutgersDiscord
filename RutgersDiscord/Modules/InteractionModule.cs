using CoreRCON;
using Discord;
using Interactivity;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RutgersDiscord.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractivityService _interactive;
        private readonly IServiceProvider _services;

        public InteractionModule(DiscordSocketClient client, InteractivityService interactive, IServiceProvider services)
        {
            _client = client;
            _interactive = interactive;
            _services = services;
        }

        [SlashCommand("echo", "Echo an input", runMode: RunMode.Async)]
        public async Task Echo(string input)
        {
            await RespondAsync(input);
        }

        //Not sure the correct placement/format for all of this, feel free to move it
        [SlashCommand("register", "Provide required information to register for the event")]
        public async Task Register()
        {
            var modalBuilder = new ModalBuilder()
                .WithTitle("Event Registration")
                .WithCustomId("registration_form")
                .AddTextInput("Your Name:", "player_name", required:true)
                .AddTextInput("Link to Steam profile:", "steam_url", required:true)
                .AddTextInput("Your team name:", "team_name", required:true)
                ;

            await Context.Interaction.RespondWithModalAsync(modalBuilder.Build());

            _client.ModalSubmitted += async modal =>
            {
                List<SocketMessageComponentData> components = modal.Data.Components.ToList();

                //TODO: Replace with database connection
                string playerName = components.First(x => x.CustomId == "player_name").Value;
                string steamURL = components.First(x => x.CustomId == "steam_url").Value;
                string teamName = components.First(x => x.CustomId == "team_name").Value;

                string msg = $"{playerName} with {steamURL} on {teamName}";

                await modal.RespondAsync(msg);
            };
        }

        [SlashCommand("ready", "Set your team as ready for the match")]
        public async Task TeamReady()
        {
            //Replace with database lookup of team name based on discord id
            string teamName = "testing";
            await RespondAsync($"{teamName} is ready for the match");
            //Tell MatchHandler that the team is ready
        }

        [SlashCommand("unready", "Set your team as not ready for the match")]
        public async Task TeamUnReady()
        {
            //Replace with database lookup of team name based on discord id
            string teamName = "testing";
            await RespondAsync($"{teamName} is not ready for the match");
            //Tell MatchHandler that the team is not ready
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
    }
}