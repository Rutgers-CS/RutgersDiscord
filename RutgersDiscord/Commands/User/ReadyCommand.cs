using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using Newtonsoft.Json;
using RutgersDiscord.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RutgersDiscord.Commands
{
    public class ReadyCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;
        private readonly DatHostAPIHandler _datHostAPIHandler;
        private readonly GameServerHandler _gameServerHandler;

        public ReadyCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity, GameServerHandler gameServerHandler, DatHostAPIHandler datHostAPIHandler)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
            _datHostAPIHandler = datHostAPIHandler;
            _gameServerHandler = gameServerHandler;
        }

        public async Task Ready()
        {
            MatchInfo match = (await _database.GetMatchByAttribute(discordChannel: (long?)_context.Channel.Id)).FirstOrDefault();
            if(match == null)
            {
                await _context.Interaction.RespondAsync("Match not found", ephemeral: true);
                return;
            }

            DateTime t = new DateTime().AddTicks((long)match.MatchTime);
            if(t - DateTime.Now > TimeSpan.FromMinutes(15))
            {
                await _context.Interaction.RespondAsync("Cannot ready more than 15 mins before match", ephemeral: true);
                Console.WriteLine(t.AddMinutes(16).Ticks);
                return;
            }

            TeamInfo team = await _database.GetTeamByDiscordIDAsync((long)_context.Interaction.User.Id, true);
            if (team == null)
            {
                await _context.Interaction.RespondAsync("User not captain of a team", ephemeral: true);
                return;
            }

            if(match.TeamHomeID == team.TeamID)
            {
                if(match.TeamHomeReady == true)
                {
                    await _context.Interaction.RespondAsync($"{team.TeamName} is already ready!");
                    return;
                }
                //ready and wait for enemy
                if (match.TeamAwayReady == false)
                {
                    match.TeamHomeReady = true;
                    await _database.UpdateMatchAsync(match);
                    await _context.Interaction.RespondAsync($"{team.TeamName} is now ready!");
                    return;
                }
            }
            else
            {
                if (match.TeamAwayReady == true)
                {
                    await _context.Interaction.RespondAsync($"{team.TeamName} is already ready!");
                    return;
                }
                //ready and wait for enemy
                if (match.TeamHomeReady == false)
                {
                    match.TeamAwayReady = true;
                    await _database.UpdateMatchAsync(match);
                    await _context.Interaction.RespondAsync($"{team.TeamName} is now ready!");
                    return;
                }
            }

            match.TeamAwayReady = true;
            await _database.UpdateMatchAsync(match);


            //Test if veto was done and do it if not
            await _context.Interaction.DeferAsync();
            if(match.MapID == null)
            {
                await _context.Channel.SendMessageAsync("Veto not done. Starting veto");
                VetoCommand v = new(_client,_context,_database,_interactivity);
                await v.StartVeto();
            }
            else
            {
                await _context.Channel.SendMessageAsync("Starting Match");
            }

            match = (await _database.GetMatchByAttribute(discordChannel: (long?)_context.Channel.Id)).FirstOrDefault();

            if (match.ServerID == null)
            {
                Console.WriteLine("Creating server");
                //Start match (generate match)
                try
                {
                    ServerInfo newServer = await _datHostAPIHandler.CreateNewServer();
                    ServerTokens newToken = await _database.GetUnusedToken();
                    if (newToken != null)
                    {
                        Console.WriteLine(newToken.Token);
                        newToken.ServerID = newServer.ServerID;
                        await _database.UpdateToken(newToken);
                        await _datHostAPIHandler.UpdateServerToken(newServer.ServerID, _context.Channel.Name, newToken.Token);

                        TeamInfo homeTeam = await _database.GetTeamAsync((int)match.TeamHomeID);
                        PlayerInfo hP1 = await _database.GetPlayerAsync((long)homeTeam.Player1);
                        PlayerInfo hP2 = await _database.GetPlayerAsync((long)homeTeam.Player2);
                        TeamInfo awayTeam = await _database.GetTeamAsync((int)match.TeamAwayID);
                        PlayerInfo aP1 = await _database.GetPlayerAsync((long)awayTeam.Player1);
                        PlayerInfo aP2 = await _database.GetPlayerAsync((long)awayTeam.Player2);
                        MapInfo map = await _database.GetMapAsync((int)match.MapID);
                        MatchSettings ms = new MatchSettings(map, homeTeam, hP1, hP2, awayTeam, aP1, aP2, newServer.ServerID);

                        var st = await _datHostAPIHandler.CreateMatch(ms);
                        PreGameJson preGameJson = JsonConvert.DeserializeObject<PreGameJson>(st);
                        Console.WriteLine(st);
                        await _context.Channel.SendMessageAsync($"Paste in csgo console: `connect {newServer.IP}:{newServer.Port}`");
                        await _context.Channel.SendMessageAsync($"If you can't connect try again in a few seconds the server might still be booting up");

                        match.ServerID = newServer.ServerID;
                        match.DatMatchID = preGameJson.id;
                        await _database.UpdateMatchAsync(match);
                    }
                    else
                    {
                        await _context.Channel.SendMessageAsync("No free servers availible");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                await _context.Channel.SendMessageAsync("Server already assigned");
            }

            await _context.Interaction.DeleteOriginalResponseAsync();
        }

        public class PlayerStat
        {
            public int assists { get; set; }
            public int deaths { get; set; }
            public int kills { get; set; }
            public string steam_id { get; set; }
        }

        public class PlaywinResult
        {
        }

        public class Team1Stats
        {
            public int score { get; set; }
        }

        public class Team2Stats
        {
            public int score { get; set; }
        }

        public class PreGameJson
        {
            public string cancel_reason { get; set; }
            public int connect_time { get; set; }
            public bool enable_knife_round { get; set; }
            public bool enable_pause { get; set; }
            public bool enable_playwin { get; set; }
            public bool enable_ready { get; set; }
            public bool enable_tech_pause { get; set; }
            public bool finished { get; set; }
            public string game_server_id { get; set; }
            public string id { get; set; }
            public string map { get; set; }
            public string match_end_webhook_url { get; set; }
            public string match_series_id { get; set; }
            public string message_prefix { get; set; }
            public List<PlayerStat> player_stats { get; set; }
            public PlaywinResult playwin_result { get; set; }
            public string playwin_result_webhook_url { get; set; }
            public int ready_min_players { get; set; }
            public string round_end_webhook_url { get; set; }
            public int rounds_played { get; set; }
            public List<string> spectator_steam_ids { get; set; }
            public bool started { get; set; }
            public string team1_coach_steam_id { get; set; }
            public string team1_flag { get; set; }
            public string team1_name { get; set; }
            public bool team1_start_ct { get; set; }
            public Team1Stats team1_stats { get; set; }
            public List<string> team1_steam_ids { get; set; }
            public string team2_coach_steam_id { get; set; }
            public string team2_flag { get; set; }
            public string team2_name { get; set; }
            public Team2Stats team2_stats { get; set; }
            public List<string> team2_steam_ids { get; set; }
            public int team_size { get; set; }
            public bool wait_for_coaches { get; set; }
            public bool wait_for_gotv_before_nextmap { get; set; }
            public bool wait_for_spectators { get; set; }
            public int warmup_time { get; set; }
        }
    }
}
