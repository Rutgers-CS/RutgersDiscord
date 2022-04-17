using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using Newtonsoft.Json;
using RutgersDiscord.Types.GameServer.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RutgersDiscord.Handlers
{
    public class StartMatchHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;
        private readonly DatHostAPIHandler _datHostAPIHandler;
        private readonly GameServerHandler _gameServerHandler;
        private readonly ConfigHandler _config;

        public StartMatchHandler(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity, GameServerHandler gameServerHandler, DatHostAPIHandler datHostAPIHandler, ConfigHandler config)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
            _datHostAPIHandler = datHostAPIHandler;
            _gameServerHandler = gameServerHandler;
            _config = config;
        }

        public async Task<ServerInfo> CreateMatch(IEnumerable<MatchInfo> matches, string matchName)
        {
            if (matches.Count() == 1)
            {
                MatchInfo match = matches.First();
                if (match.ServerID == null)
                {
                    ServerInfo newServer = await _datHostAPIHandler.CreateNewServer();
                    ServerTokens newToken = await _database.GetUnusedToken();
                    if (newToken == null)
                    {
                        return null;
                    }

                    newToken.ServerID = newServer.ServerID;
                    await _database.UpdateToken(newToken);
                    await _datHostAPIHandler.UpdateServerToken(newServer.ServerID, matchName, newToken.Token);

                    TeamInfo homeTeam = await _database.GetTeamAsync((int)match.TeamHomeID);
                    PlayerInfo hP1 = await _database.GetPlayerAsync((long)homeTeam.Player1);
                    PlayerInfo hP2 = await _database.GetPlayerAsync((long)homeTeam.Player2);
                    TeamInfo awayTeam = await _database.GetTeamAsync((int)match.TeamAwayID);
                    PlayerInfo aP1 = await _database.GetPlayerAsync((long)awayTeam.Player1);
                    PlayerInfo aP2 = await _database.GetPlayerAsync((long)awayTeam.Player2);
                    MapInfo map = await _database.GetMapAsync((int)match.MapID);

                    //Ping for grassetto
                    if (map.MapID == 2)
                    {
                        ulong adminChannel = _config.settings.DiscordSettings.Channels.SCAdmin;
                        var chnl = _client.GetChannel(adminChannel) as IMessageChannel;
                        await chnl.SendMessageAsync($"Grassetto is being played in <#{_context.Channel.Id}>\nuse /grass-restart if players get stuck.");
                    }

#if DEBUG
                    string webHook = "";
#else
                        string webHook = $"http://{_config.settings.ApplicationSettings.PublicIP}:{_config.settings.ApplicationSettings.Port}/api";
#endif
                    MatchSettings ms = new MatchSettings(map, homeTeam, hP1, hP2, awayTeam, aP1, aP2, newServer.ServerID, webHook);

                    var st = await _datHostAPIHandler.CreateMatch(ms);
                    CreateServerResponse preGameJson = JsonConvert.DeserializeObject<CreateServerResponse>(st);

                    /*long galID = (long)_config.settings.DiscordSettings.Users.Galifi;
                        long opID = (long)_config.settings.DiscordSettings.Users.Op7day;
                        long kenID = (long)_config.settings.DiscordSettings.Users.Guihori;
                        //Add admins as spectator if not in the game
                        try
                        {
                            if (hP1.DiscordID != galID && hP2.DiscordID != galID && aP1.DiscordID != galID && aP2.DiscordID != galID)
                            {
                                ms.spectator_steam_ids += (await _database.GetPlayerAsync(galID)).SteamID + ",";
                            }

                            if (hP1.DiscordID != opID && hP2.DiscordID != opID && aP1.DiscordID != opID && aP2.DiscordID != opID)
                            {
                                ms.spectator_steam_ids += (await _database.GetPlayerAsync(opID)).SteamID + ",";
                            }

                            if (hP1.DiscordID != kenID && hP2.DiscordID != kenID && aP1.DiscordID != kenID && aP2.DiscordID != kenID)
                            {
                                ms.spectator_steam_ids += (await _database.GetPlayerAsync(kenID)).SteamID + ",";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Adding Spectators Failed");
                        }*/

                    //Send gotv in scgeneral
                    ulong scmatches = _config.settings.DiscordSettings.Channels.SCMatches;
                    var scgenChan = _client.GetChannel(scmatches) as IMessageChannel;
                    string msg = $"{homeTeam.TeamName} vs {awayTeam.TeamName} is live now!\nConnect to GOTV: `connect {newServer.IP}:{newServer.Port + 1}`";
                    _interactivity.DelayedSendMessageAndDeleteAsync(scgenChan, deleteDelay: TimeSpan.FromMinutes(30), text: msg);

                    match.ServerID = newServer.ServerID;
                    match.DatMatchID = preGameJson.id;
                    await _database.UpdateMatchAsync(match);
                    return newServer;
                }
            }
            else
            {
                if (matches.First().ServerID == null)
                {
                    ServerInfo newServer = await _datHostAPIHandler.CreateNewServer();
                    ServerTokens newToken = await _database.GetUnusedToken();
                    if (newToken == null)
                    {
                        return null;
                    }

                    newToken.ServerID = newServer.ServerID;
                    await _database.UpdateToken(newToken);
                    await _datHostAPIHandler.UpdateServerToken(newServer.ServerID, matchName, newToken.Token);

                    
                    TeamInfo homeTeam = await _database.GetTeamAsync((int)(matches.First().TeamHomeID));
                    PlayerInfo hP1 = await _database.GetPlayerAsync((long)homeTeam.Player1);
                    PlayerInfo hP2 = await _database.GetPlayerAsync((long)homeTeam.Player2);
                    TeamInfo awayTeam = await _database.GetTeamAsync((int)(matches.First().TeamAwayID));
                    PlayerInfo aP1 = await _database.GetPlayerAsync((long)awayTeam.Player1);
                    PlayerInfo aP2 = await _database.GetPlayerAsync((long)awayTeam.Player2);
                    List<MapInfo> maps = new List<MapInfo>();
                    foreach (MatchInfo match in matches)
                    {
                        maps.Add(await _database.GetMapAsync((int)match.MapID));
                        if (match.MapID == 2)
                        {
                            ulong adminChannel = _config.settings.DiscordSettings.Channels.SCAdmin;
                            var chnl = _client.GetChannel(adminChannel) as IMessageChannel;
                            await chnl.SendMessageAsync($"Grassetto is being played in <#{_context.Channel.Id}>\nuse /grass-restart if players get stuck.");
                        }
                    }

#if DEBUG
                    string webHook = "";
#else
                    string webHook = $"http://{_config.settings.ApplicationSettings.PublicIP}:{_config.settings.ApplicationSettings.Port}/api";
#endif
                    MatchSeriesSettings mss = new MatchSeriesSettings(maps[0], maps[1], maps[2], homeTeam, hP1, hP2, awayTeam, aP1, aP2, newServer.ServerID, webHook);

                    var st = await _datHostAPIHandler.CreateMatchSeries(mss);
                    CreateServerResponse preGameJson = JsonConvert.DeserializeObject<CreateServerResponse>(st);

                    ulong scmatches = _config.settings.DiscordSettings.Channels.SCMatches;
                    var scgenChan = _client.GetChannel(scmatches) as IMessageChannel;
                    string msg = $"{homeTeam.TeamName} vs {awayTeam.TeamName} is live now!\nConnect to GOTV: `connect {newServer.IP}:{newServer.Port + 1}`";
                    _interactivity.DelayedSendMessageAndDeleteAsync(scgenChan, deleteDelay: TimeSpan.FromMinutes(60), text: msg);

                    foreach (MatchInfo match in matches)
                    {
                        match.ServerID = newServer.ServerID;
                        match.DatMatchID = preGameJson.id;
                        await _database.UpdateMatchAsync(match);
                    }
                    return newServer;
                }
            }
            return null;
        }
    }
}
