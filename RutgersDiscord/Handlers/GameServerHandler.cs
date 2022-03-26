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


        public GameServerHandler(DatabaseHandler database, DatHostAPIHandler datHostAPIHandler)
        {
            _database = database;
            _datHostAPIHandler = datHostAPIHandler;
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

        public class Root
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
            public object cancel_reason { get; set; }
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

        public async Task UpdateDatabase(string json)
        {
            Root generaljson = JsonConvert.DeserializeObject<Root>(json);
       
            Team1Stats team1 = generaljson.team1_stats;
            Team2Stats team2 = generaljson.team2_stats;


            //update match in database
            string matchid = generaljson.id;
            string serverid = generaljson.game_server_id;

            var cmatch = (await _database.GetMatchByAttribute(datMatchID: matchid)).First();
            cmatch.DatMatchID = matchid;
            cmatch.ServerID = serverid;
            await _database.UpdateMatchAsync(cmatch);


            var cteam1 = (await _database.GetTeamByAttribute(cmatch.TeamHomeID)).First();
            var cteam2 = (await _database.GetTeamByAttribute(cmatch.TeamAwayID)).First();

            //update team in database
            if (team1.score > team2.score)
            {
                cteam1.Wins += 1;
                cteam2.Losses += 1;
            }
            else
            {
                cteam1.Losses += 1;
                cteam2.Wins += 1;
            }
            cteam1.RoundWins += team1.score;
            cteam1.RoundLosses += team2.score;

            cteam2.RoundWins += team2.score;
            cteam2.RoundLosses += team1.score;

            await _database.UpdateTeamAsync(cteam1);
            await _database.UpdateTeamAsync(cteam2);


            //Get Channel
            SocketGuild guild = _client.GetGuild(Constants.guild);
            SocketTextChannel channel = guild.GetTextChannel((ulong)cmatch.DiscordChannel);

            List<PlayerStat> players = generaljson.player_stats;

            //update player in database and channel permissions
            foreach (PlayerStat player in players)
            {

                long playersteamid = long.Parse(player.steam_id);
                var cplayer = (await _database.GetPlayerByAttribute(playersteamid)).First();

                cplayer.Kills += player.kills;
                cplayer.Deaths += player.deaths;

                await _database.UpdatePlayerAsync(cplayer);

                //update channel permissions
                await channel.AddPermissionOverwriteAsync(guild.GetUser((ulong)cplayer.DiscordID), new OverwritePermissions(sendMessages:PermValue.Deny));
               
            }

            await _datHostAPIHandler.DeleteServer(serverid);

        }


    }
}
