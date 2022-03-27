using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using RutgersDiscord.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;

namespace RutgersDiscord.Handlers
{
    public class GameServerHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;
        private readonly DatHostAPIHandler _datHostAPIHandler;
        private readonly ConfigHandler _config;


        public GameServerHandler(DiscordSocketClient client, DatabaseHandler database, DatHostAPIHandler datHostAPIHandler, ConfigHandler config)
        {
            _client = client;
            _database = database;
            _datHostAPIHandler = datHostAPIHandler;
            _config = config;
        }

        public async Task UpdateDatabase(string json)
        {
            Console.WriteLine(json);
            ServerReply result = JsonConvert.DeserializeObject<ServerReply>(json, new JsonSerializerSettings{NullValueHandling = NullValueHandling.Ignore});

            //update match in database
            string matchid = result.id;
            string serverid = result.game_server_id;
            
            MatchInfo cmatch = (await _database.GetMatchByAttribute(datMatchID: matchid)).First();
            /*            cmatch.DatMatchID = matchid;
                        cmatch.ServerID = serverid;
                        await _database.UpdateMatchAsync(cmatch);*/

            //Get Channel
            SocketGuild guild = _client.GetGuild(_config.settings.DiscordSettings.Guild);
            SocketTextChannel channel = guild.GetTextChannel((ulong)cmatch.DiscordChannel);

            //Match canceled
            if (result.cancel_reason != null)
            {
                string canceled = result.cancel_reason;
                if (canceled.StartsWith("MISSING_PLAYERS:"))
                {
                    string[] canceledPlayers = canceled.Replace("MISSING_PLAYERS:", "").Split(',');
                    foreach (var s in canceledPlayers)
                    {
                        var player = (await _database.GetPlayerByAttribute(steamID: s)).First().DiscordID;
                        canceled += $"<@{player}> ";
                    }
                    canceled += "failed to connect.";
                }
                
                await channel.SendMessageAsync($"Match cancelled with reason: `{canceled}`");
                await _datHostAPIHandler.DeleteServer(serverid);

                cmatch.DatMatchID = null;
                cmatch.ServerID = null;
                cmatch.TeamAwayReady = false;
                cmatch.TeamHomeReady = false;
                await _database.UpdateMatchAsync(cmatch);

                await channel.SendMessageAsync("Both teams need to ready again to start the match.");
                return;
            }

            Team1Stats homeTeam = result.team1_stats;
            Team2Stats awayTeam = result.team2_stats;

            var currentHomeTeam = (await _database.GetTeamByAttribute(cmatch.TeamHomeID)).First();
            //TODO Change To Kenji Init Method
            if (currentHomeTeam.Wins == null) currentHomeTeam.Wins = 0;
            if (currentHomeTeam.Losses == null) currentHomeTeam.Losses = 0;
            if (currentHomeTeam.RoundWins == null) currentHomeTeam.RoundWins = 0;
            if (currentHomeTeam.RoundLosses == null) currentHomeTeam.RoundLosses = 0;
            
            var currentAwayTeam = (await _database.GetTeamByAttribute(cmatch.TeamAwayID)).First();
            //TODO Change To Kenji Init Method
            if (currentAwayTeam.Wins == null) currentAwayTeam.Wins = 0;
            if (currentAwayTeam.Losses == null) currentAwayTeam.Losses = 0;
            if (currentAwayTeam.RoundWins == null) currentAwayTeam.RoundWins = 0;
            if (currentAwayTeam.RoundLosses == null) currentAwayTeam.RoundLosses = 0;

            //update team in database
            if (homeTeam.score > awayTeam.score)
            {
                currentHomeTeam.Wins += 1;
                currentAwayTeam.Losses += 1;
                cmatch.HomeTeamWon = true;
            }
            else
            {
                currentHomeTeam.Losses += 1;
                currentAwayTeam.Wins += 1;
                cmatch.HomeTeamWon = false;
            }

            currentHomeTeam.RoundWins += homeTeam.score;
            currentHomeTeam.RoundLosses += awayTeam.score;

            currentAwayTeam.RoundWins += awayTeam.score;
            currentAwayTeam.RoundLosses += homeTeam.score;

            Console.WriteLine(currentHomeTeam);

            await _database.UpdateTeamAsync(currentHomeTeam);
            await _database.UpdateTeamAsync(currentAwayTeam);


            List<PlayerStat> players = result.player_stats;
            Console.WriteLine(players.Count);
            //update player in database and channel permissions
            foreach (PlayerStat player in players)
            {
                if (!player.steam_id.StartsWith("BOT"))
                {
                    Console.WriteLine(player.steam_id);
                    var cplayer = (await _database.GetPlayerByAttribute(steamID: player.steam_id)).FirstOrDefault();
                    //TODO Change to kenji init method
                    if (cplayer.Kills == null) cplayer.Kills = 0;
                    if (cplayer.Deaths == null) cplayer.Deaths = 0;

                    cplayer.Kills += player.kills;
                    cplayer.Deaths += player.deaths;
                    await _database.UpdatePlayerAsync(cplayer);
                    //update channel permissions
                    await channel.AddPermissionOverwriteAsync(guild.GetUser((ulong)cplayer.DiscordID), new OverwritePermissions(sendMessages: PermValue.Deny));
                }
            }

            cmatch.MatchFinished = true;
            cmatch.ScoreHome = homeTeam.score;
            cmatch.ScoreAway = awayTeam.score;
            await _database.UpdateMatchAsync(cmatch);

            //Fetch Demo
            new Task(async () =>
            {
                Console.WriteLine("Waiting For Demo");
                //wait 30 sec for match to finish processing
                System.Threading.Thread.Sleep(60000);
                Console.WriteLine("Downloading Demo");
                await _datHostAPIHandler.GetDemo(serverid, matchid);
            }).Start();

            //Delete the server in 5 mins
            new Task(async () =>
            {
                //5 min
                System.Threading.Thread.Sleep(300000);
                /*var token = await _database.GetTokenByServerID(serverid);
                token.ServerID = null;
                await _database.UpdateToken(token);*/
                await _datHostAPIHandler.DeleteServer(serverid);
            }).Start();
            
        }

        public class Team1Stats
        {
            public int score { get; set; }
        }

        public class Team2Stats
        {
            public int score { get; set; }
        }

        public class PlayerStat
        {
            public string steam_id { get; set; }
            public int kills { get; set; }
            public int deaths { get; set; }
            public int assists { get; set; }
        }

        public class PlaywinResult
        {
        }

        public class ServerReply
        {
            public string id { get; set; }
            public string game_server_id { get; set; }
            public string map { get; set; }
            public int connect_time { get; set; }
            public int warmup_time { get; set; }
            public bool team1_start_ct { get; set; }
            public List<string> team1_steam_ids { get; set; }
            public object team1_coach_steam_id { get; set; }
            public string team1_name { get; set; }
            public string team1_flag { get; set; }
            public List<string> team2_steam_ids { get; set; }
            public object team2_coach_steam_id { get; set; }
            public string team2_name { get; set; }
            public string team2_flag { get; set; }
            public List<object> spectator_steam_ids { get; set; }
            public bool wait_for_coaches { get; set; }
            public bool wait_for_spectators { get; set; }
            public string round_end_webhook_url { get; set; }
            public string match_end_webhook_url { get; set; }
            public bool started { get; set; }
            public bool finished { get; set; }
            public string cancel_reason { get; set; }
            public int rounds_played { get; set; }
            public Team1Stats team1_stats { get; set; }
            public Team2Stats team2_stats { get; set; }
            public List<PlayerStat> player_stats { get; set; }
            public bool enable_knife_round { get; set; }
            public bool enable_pause { get; set; }
            public bool enable_playwin { get; set; }
            public object playwin_result_webhook_url { get; set; }
            public PlaywinResult playwin_result { get; set; }
            public bool enable_ready { get; set; }
            public int ready_min_players { get; set; }
            public bool enable_tech_pause { get; set; }
            public string message_prefix { get; set; }
        }
    }
}
