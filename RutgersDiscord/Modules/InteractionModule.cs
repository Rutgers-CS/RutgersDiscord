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
        private readonly DatabaseHandler _database;
        private readonly RegistrationHandler _registrationHandler;
        private readonly DatHostAPIHandler _datHostAPIHandler;

        public InteractionModule(DiscordSocketClient client, InteractivityService interactivity, DatabaseHandler database, RegistrationHandler registrationHandler, DatHostAPIHandler datHostAPIHandler)
        {
            _client = client;
            _interactivity = interactivity;
            _database = database;
            _registrationHandler = registrationHandler;
            _datHostAPIHandler = datHostAPIHandler;
        }

        [SlashCommand("echo", "Echo an input", runMode: RunMode.Async)]
        public async Task Echo(string input)
        {
            await RespondAsync(input);
        }

        [SlashCommand("announcement", "Posts announcement")]
        public async Task PostAnnouncement()
        {
            PostAnnouncement pa = new PostAnnouncement(_client, Context, _database, _interactivity);
            await pa.GetAnnouncement();
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
            RegisterCommand rc = new RegisterCommand(_client, Context, _database, _interactivity, _registrationHandler);
            await rc.RegistrationForm();
        }

        [SlashCommand("ready", "Set your team as ready for the match")]
        public async Task TeamReady()
        {
            ReadyCommand rc = new ReadyCommand(_client, Context, _database, _interactivity, _datHostAPIHandler);
            await rc.Ready();
        }

        [SlashCommand("unready", "Set your team as not ready for the match")]
        public async Task TeamUnReady()
        {
            UnReadyCommand urc = new UnReadyCommand(_client, Context, _database, _interactivity);
            await urc.UnReady();
        }

        [SlashCommand("admin", "Notify an admin")]
        public async Task NotifyAdmin()
        {
            NotifyAdminCommand nac = new NotifyAdminCommand(_client, Context, _database, _interactivity);
            await nac.CallAdmin();
        }

        [SlashCommand("stats", "Display the tournament's statistical leaders")]
        public async Task FetchStats()
        {
            StatsCommand sc = new StatsCommand(_client, Context, _database, _interactivity);
            await sc.GetStats();
        }

        [SlashCommand("leaderboard", "Display the tournament leaderboard")]
        public async Task DisplayLeaderboard()
        {
            LeaderboardCommand lc = new LeaderboardCommand(_client, Context, _database, _interactivity);
            await lc.PullLeaderboard();
        }

        [SlashCommand("help", "Display all user commands")]
        public async Task Help()
        {
            HelpCommand lc = new HelpCommand(_client, Context, _database, _interactivity);
            await lc.GetHelp();
        }

        [SlashCommand("teamselection", "Select or create a team")]
        public async Task TeamSelection()
        {

        }

        
    }
}