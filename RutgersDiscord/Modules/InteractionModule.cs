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
            await v.StartVetoAcknowledge();
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

        [SlashCommand("teamselection", "Select or create a team")]
        public async Task TeamSelection()
        {

        }

        
    }
}