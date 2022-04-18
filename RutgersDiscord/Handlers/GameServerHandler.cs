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
            bool series = json.Contains("match_series_id");
            MatchEndWebhook result = JsonConvert.DeserializeObject<MatchEndWebhook>(json, new JsonSerializerSettings{NullValueHandling = NullValueHandling.Ignore});

            MatchInfo thisMatch = (await _database.GetMatchByAttribute(datMatchID: result.id)).First();
            SocketGuild guild = _client.GetGuild(_config.settings.DiscordSettings.Guild);
            SocketTextChannel channel = guild.GetTextChannel((ulong)thisMatch.DiscordChannel);


            if (result.cancel_reason != null)
            {
                string canceled = result.cancel_reason;

                await channel.SendMessageAsync($"Match cancelled with reason: `{canceled}`");
                await _datHostAPIHandler.DeleteServer(result.game_server_id);

                thisMatch.DatMatchID = null;
                thisMatch.ServerID = null;
                thisMatch.TeamAwayReady = false;
                thisMatch.TeamHomeReady = false;
                await _database.UpdateMatchAsync(thisMatch);

                await channel.SendMessageAsync("Both teams need to ready again to start the match.");
                return;
            }

            MatchTeam1Stats homeTeam = result.team1_stats;
            MatchTeam2Stats awayTeam = result.team2_stats;

            var currentHomeTeam = (await _database.GetTeamByAttribute(thisMatch.TeamHomeID)).First();
            if (currentHomeTeam.Wins == null) currentHomeTeam.Wins = 0;
            if (currentHomeTeam.Losses == null) currentHomeTeam.Losses = 0;
            if (currentHomeTeam.RoundWins == null) currentHomeTeam.RoundWins = 0;
            if (currentHomeTeam.RoundLosses == null) currentHomeTeam.RoundLosses = 0;

            var currentAwayTeam = (await _database.GetTeamByAttribute(thisMatch.TeamAwayID)).First();
            if (currentAwayTeam.Wins == null) currentAwayTeam.Wins = 0;
            if (currentAwayTeam.Losses == null) currentAwayTeam.Losses = 0;
            if (currentAwayTeam.RoundWins == null) currentAwayTeam.RoundWins = 0;
            if (currentAwayTeam.RoundLosses == null) currentAwayTeam.RoundLosses = 0;

            if (series)
            {
                if (homeTeam.score > awayTeam.score)
                {
                    thisMatch.HomeTeamWon = true;
                }
                else
                {
                    thisMatch.HomeTeamWon = false;
                }
            }
            else
            {
                //update team in database
                if (homeTeam.score > awayTeam.score)
                {
                    currentHomeTeam.Wins += 1;
                    currentAwayTeam.Losses += 1;
                    thisMatch.HomeTeamWon = true;
                }
                else
                {
                    currentHomeTeam.Losses += 1;
                    currentAwayTeam.Wins += 1;
                    thisMatch.HomeTeamWon = false;
                }
            }

            currentHomeTeam.RoundWins += homeTeam.score;
            currentHomeTeam.RoundLosses += awayTeam.score;

            currentAwayTeam.RoundWins += awayTeam.score;
            currentAwayTeam.RoundLosses += homeTeam.score;

            bool seriesFinished = false;
            if (series)
            {
                IEnumerable<MatchInfo> matches = await _database.GetMatchByAttribute(discordChannel: thisMatch.DiscordChannel);
                int homeMapWins = 0;
                int awayMapWins = 0;
                foreach (MatchInfo match in matches)
                {
                    if (match.MatchFinished == true)
                    {
                        if (match.HomeTeamWon == true)
                        {
                            homeMapWins++;
                        }
                        else
                        {
                            awayMapWins++;
                        }
                    }
                }

                ulong scmatches = _config.settings.DiscordSettings.Channels.SCMatches;
                var scmatchchannel = _client.GetChannel(scmatches) as IMessageChannel;
                string homeTeamName = result.team1_name;
                string awayTeamName = result.team2_name;

                if (homeMapWins == 2)
                {
                    currentHomeTeam.Wins += 1;
                    currentAwayTeam.Losses += 1;
                    await scmatchchannel.SendMessageAsync($"{homeTeamName} has beat {awayTeamName} {homeMapWins}:{awayMapWins}");
                    seriesFinished = true;
                }
                else if (awayMapWins == 2)
                {
                    currentAwayTeam.Wins += 1;
                    currentHomeTeam.Losses += 1;
                    await scmatchchannel.SendMessageAsync($"{awayTeamName} has beat {homeTeamName} {awayMapWins}:{homeMapWins}");
                    seriesFinished = true;
                }
            }

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

            thisMatch.MatchFinished = true;
            thisMatch.ScoreHome = homeTeam.score;
            thisMatch.ScoreAway = awayTeam.score;
            await _database.UpdateMatchAsync(thisMatch);

            //Fetch Demo
            Console.WriteLine("Waiting For Demo");
            //wait 3 min for match to finish processing
            System.Threading.Thread.Sleep(180000);
            Console.WriteLine("Downloading Demo");
            await _datHostAPIHandler.GetDemo(result.game_server_id, result.id);

            if (!series || seriesFinished)
            {
                await _datHostAPIHandler.DeleteServer(result.game_server_id);
                try
                {
                    var token = await _database.GetTokenByServerID(result.game_server_id);
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
}
