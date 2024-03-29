﻿using Discord;
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
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;
        private readonly DatHostAPIHandler _datHostAPIHandler;
        private readonly GameServerHandler _gameServerHandler;
        private readonly ConfigHandler _config;

        public StartMatchHandler(DiscordSocketClient client, DatabaseHandler database, InteractivityService interactivity, GameServerHandler gameServerHandler, DatHostAPIHandler datHostAPIHandler, ConfigHandler config)
        {
            _client = client;
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
                    //TODO Broken
                    /*if (map.MapID == 2)
                    {
                        ulong adminChannel = _config.settings.DiscordSettings.Channels.SCAdmin;
                        var chnl = _client.GetChannel(adminChannel) as IMessageChannel;
                        await chnl.SendMessageAsync($"Grassetto is being played in <#{_context.Channel.Id}>\nuse /grass-restart if players get stuck.");
                    }*/

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
                    string msg = $"{homeTeam.TeamName} vs {awayTeam.TeamName} is live now!\nCheck Stream or Discord to watch."; //\nConnect to GOTV: `connect {newServer.IP}:{newServer.Port}`
                    _interactivity.DelayedSendMessageAndDeleteAsync(scgenChan, deleteDelay: TimeSpan.FromMinutes(30), text: msg);

                    match.ServerID = newServer.ServerID;
                    match.DatMatchID = preGameJson.id;
                    await _database.UpdateMatchAsync(match);
                    return newServer;
                }
            }
            else
            {
                Console.WriteLine("test2");
                if (matches.First().ServerID == null)
                {
                    Console.WriteLine("test3");
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
                        //TODO Broken
                        /*if (match.MapID == 2)
                        {
                            ulong adminChannel = _config.settings.DiscordSettings.Channels.SCAdmin;
                            var chnl = _client.GetChannel(adminChannel) as IMessageChannel;
                            await chnl.SendMessageAsync($"Grassetto is being played in <#{_context.Channel.Id}>\nuse /grass-restart if players get stuck.");
                        }*/
                    }

#if DEBUG
                    string webHook = "";
#else
                    string webHook = $"http://{_config.settings.ApplicationSettings.PublicIP}:{_config.settings.ApplicationSettings.Port}/api";
#endif
                    MatchSeriesSettings mss = new MatchSeriesSettings(maps[0], maps[1], maps[2], homeTeam, hP1, hP2, awayTeam, aP1, aP2, newServer.ServerID, webHook);

                    try
                    {
                        var st = await _datHostAPIHandler.CreateMatchSeries(mss);
                        Console.WriteLine(st);
                        CreateSeriesResponse preGameJson = JsonConvert.DeserializeObject<CreateSeriesResponse>(st);
                        Console.WriteLine("fuck you");
                        ulong scmatches = _config.settings.DiscordSettings.Channels.SCMatches;
                        var scgenChan = _client.GetChannel(scmatches) as IMessageChannel;
                        string msg = $"{homeTeam.TeamName} vs {awayTeam.TeamName} is live now!\nCheck Stream or Discord to watch.`";
                        _interactivity.DelayedSendMessageAndDeleteAsync(scgenChan, deleteDelay: TimeSpan.FromMinutes(60), text: msg);
                        Console.WriteLine(2);
                        List<MatchInfo> mts = matches.ToList();
                        for (int i = 0; i < matches.Count(); i++)
                        {
                            mts[i].ServerID = newServer.ServerID;
                            mts[i].DatMatchID = preGameJson.matches[i].id;
                            await _database.UpdateMatchAsync(mts[i]);
                        }
                        Console.WriteLine(3);
                        return newServer;
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex);
                    }
                }
            }
            return null;
        }
    }
}
