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
using RutgersDiscord.Types.GameServer.Webhooks;

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
            return;
            MatchEndWebhook result = JsonConvert.DeserializeObject<MatchEndWebhook>(json, new JsonSerializerSettings{NullValueHandling = NullValueHandling.Ignore});

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

            MatchTeam1Stats homeTeam = result.team1_stats;
            MatchTeam2Stats awayTeam = result.team2_stats;

            var currentHomeTeam = (await _database.GetTeamByAttribute(cmatch.TeamHomeID)).First();
            if (currentHomeTeam.Wins == null) currentHomeTeam.Wins = 0;
            if (currentHomeTeam.Losses == null) currentHomeTeam.Losses = 0;
            if (currentHomeTeam.RoundWins == null) currentHomeTeam.RoundWins = 0;
            if (currentHomeTeam.RoundLosses == null) currentHomeTeam.RoundLosses = 0;
            
            var currentAwayTeam = (await _database.GetTeamByAttribute(cmatch.TeamAwayID)).First();
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


            List<MatchPlayerStat> players = result.player_stats;
            Console.WriteLine(players.Count);
            //update player in database and channel permissions
            foreach (MatchPlayerStat player in players)
            {
                if (!player.steam_id.StartsWith("BOT"))
                {
                    Console.WriteLine(player.steam_id);
                    var cplayer = (await _database.GetPlayerByAttribute(steamID: player.steam_id)).FirstOrDefault();
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

            try
            {
                ulong scmatches = _config.settings.DiscordSettings.Channels.SCMatches;
                var scmatchchannel = _client.GetChannel(scmatches) as IMessageChannel;
                string homeTeamName = result.team1_name;
                string awayTeamName = result.team2_name;
                string msg;
                if ((bool)cmatch.HomeTeamWon)
                {
                    msg = $"{homeTeamName} has beat {awayTeamName} {cmatch.ScoreHome}:{cmatch.ScoreAway}";
                }
                else
                {
                    msg = $"{awayTeamName} has beat {homeTeamName} {cmatch.ScoreAway}:{cmatch.ScoreHome}";
                }
                await scmatchchannel.SendMessageAsync(msg);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }


            //Fetch Demo
            Console.WriteLine("Waiting For Demo");
            //wait 3 min for match to finish processing
            System.Threading.Thread.Sleep(180000);
            Console.WriteLine("Downloading Demo");
            await _datHostAPIHandler.GetDemo(serverid, matchid);

            //Delete server
            await _datHostAPIHandler.DeleteServer(serverid);
            try
            {
                var token = await _database.GetTokenByServerID(serverid);
                token.ServerID = null;
                await _database.UpdateToken(token);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Token unallocate failed\n" + ex);
            }
        }

        
    }
}
