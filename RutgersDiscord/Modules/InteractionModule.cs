﻿using Discord;
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
        private readonly RegistrationHandler _registration;
        private readonly DatHostAPIHandler _datHostAPI;
        private readonly GameServerHandler _gameServer;
        private readonly ScheduleHandler _schedule;
        private readonly ConfigHandler _config;
        private readonly StartMatchHandler _startMatchHandler;
        //FIX Assign teamID
        public InteractionModule(DiscordSocketClient client, InteractivityService interactivity, DatabaseHandler database, RegistrationHandler registration, DatHostAPIHandler datHostAPI, GameServerHandler gameServer, ScheduleHandler schedule, ConfigHandler config, StartMatchHandler startMatchHandler)
        {
            _client = client;
            _interactivity = interactivity;
            _database = database;
            _registration = registration;
            _datHostAPI = datHostAPI;
            _gameServer = gameServer;
            _schedule = schedule;
            _config = config;
            _startMatchHandler = startMatchHandler;
        }

/*        [SlashCommand("echo", "Echo an input", runMode: RunMode.Async)]
        public async Task Echo(string input)
        {
            await RespondAsync(input);
        }*/


        [SlashCommand("veto", "Starts veto process", runMode: RunMode.Async)]
        public async Task Veto()
        {
            VetoCommand v = new VetoCommand(_client, Context, _database, _interactivity);
            await v.StartVetoAcknowledge();
        }

        [SlashCommand("ready", "Set your team as ready for the match")]
        public async Task TeamReady()
        {
            ReadyCommand rc = new ReadyCommand(_client, Context, _database, _interactivity, _gameServer, _datHostAPI, _config, _startMatchHandler);
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
            NotifyAdminCommand nac = new NotifyAdminCommand(_client, Context, _database, _interactivity, _config);
            await nac.CallAdmin();
        }

        [SlashCommand("stats", "Display the tournament's statistical leaders")]
        public async Task FetchStats([Choice("KD", "kd"), Choice("Kills", "kills"), Choice("Deaths", "deaths"), Choice("All", "all")] string sortBy = "kd")
        {
            StatsCommand sc = new StatsCommand(_client, Context, _database, _interactivity);
            await sc.GetStats(sortBy);
        }

        [SlashCommand("leaderboard", "Display the tournament leaderboard")]


        public async Task DisplayLeaderboard([Choice("Record", "record"), Choice("KD", "kd"), Choice("RD", "rd")] string sortBy = "record")
        {
            LeaderboardCommand lc = new LeaderboardCommand(_client, Context, _database, _interactivity);
            await lc.PullLeaderboard(sortBy);
        }

        [SlashCommand("bracket", "Display the tournament bracket")]
        public async Task Bracket()
        {
            BracketCommand bc = new BracketCommand(_client, Context, _database, _interactivity, _config);
            await bc.GetBracket();
        }

        [SlashCommand("help", "Display all user commands")]
        public async Task Help()
        {
            HelpCommand lc = new HelpCommand(_client, Context, _database, _interactivity);
            await lc.GetHelp();
        }

        [SlashCommand("reschedule", "Requests a reschedule")]
        public async Task Resched(int month, int day, int hour, int min)
        {
            RescheduleCommand rc = new RescheduleCommand(_client, Context, _database, _interactivity,_schedule,_config);
            DateTime t = new DateTime(DateTime.Now.Year, month, day, hour, min, 0);
            await rc.RescheduleMatch(t);
        }

        [SlashCommand("match-schedule", "Shows the games remaining in this round")]
        public async Task ScheduleCommnd()
        {
            MatchSchedule ms = new(_client, Context, _database, _interactivity);
            await ms.GetMatchSchedule(false);

        }
    }
}