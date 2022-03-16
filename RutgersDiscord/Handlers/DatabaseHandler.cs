﻿using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.Sqlite;

namespace RutgersDiscord.Handlers
{
    public class DatabaseHandler
    {
        //Constants. maybe move them in the future
        const string matchTable = "matches";
        const string playerTable = "players";
        const string teamTable = "teams";
        const string mapTable = "maps";

        const string databaseName = "Data Source=database.db";

        public async Task AddTestData()
        {
            PlayerInfo august = new PlayerInfo(222897049899630592, 76561198049694740, "STEAM_1:0:44714506", "August", 1, null, null);
            PlayerInfo kenji = new PlayerInfo(164803512633524225, 76561198064565542, "STEAM_1:0:52149907", "Kenji", 1, null, null);
            PlayerInfo gal = new PlayerInfo(171431827746193410, 76561198143013155, "STEAM_1:1:91373713", "Galifi", 2, null, null);
            PlayerInfo park = new PlayerInfo(953059699627012142, 76561198405017903, "STEAM_1:1:222376087", "Andrew", 2, null, null);

            TeamInfo team1 = new TeamInfo(1, "Team1", 222897049899630592, 164803512633524225, 0, 0);
            TeamInfo team2 = new TeamInfo(2, "Team2", 171431827746193410, 953059699627012142, 0, 0);

            MatchInfo match = new MatchInfo(0, 1, 2, null, null,null, false, null, null);

            await AddPlayerAsync(august);
            await AddPlayerAsync(kenji);
            await AddPlayerAsync(gal);
            await AddPlayerAsync(park);

            await AddTeamAsync(team1);
            await AddTeamAsync(team2);

            await AddMatchAsync(match);
        }

        //TEMPORARY method. In the future we will only allow public methods here to output predefined types
        [Obsolete("Don't use it")]
        public IEnumerable<T> GetTable<T>(string strQuery, string databaseName = null)
        {
            return GetTableFromDBUsing<T>(strQuery, databaseName).Result;
        }

        #region Players CRUD
        public async Task<int> AddPlayerAsync(PlayerInfo player)
        {
            string query = $"INSERT INTO {playerTable} (DiscordID, Steam64ID, SteamID, Name, TeamID, Kills, Deaths) VALUES (@DiscordID, @Steam64ID, @SteamID, @Name, @TeamID, @Kills, @Deaths)";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query, player);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default;
            }
        }

        public async Task<PlayerInfo> GetPlayerAsync(long discordID)
        {
            string query = $"SELECT * FROM {playerTable} WHERE DiscordID = {discordID}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync(query).Result.FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> UpdatePlayerAsync(PlayerInfo player)
        {
            string query = $"UPDATE {playerTable} SET Steam64ID=@Steam64ID, SteamID=@SteamID, Name=@Name, TeamID=@TeamID, Kills=@Kills, Deaths=@Deaths WHERE DiscordID=@DiscordID";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query, player);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> DeletePlayerAsync(long discordID)
        {
            string query = $"DELETE from {playerTable} WHERE DiscordID = {discordID}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query);
                }
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region Players Extra
        public async Task<PlayerInfo> GetPlayerBySteam64IDAsync(long steam64ID)
        {
            string query = $"SELECT * FROM {playerTable} WHERE Steam64ID = {steam64ID}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync(query).Result.FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<PlayerInfo> GetPlayerBySteamIDAsync(string steamID)
        {
            string query = $"SELECT * FROM {playerTable} WHERE SteamID = {steamID}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync(query).Result.FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<IEnumerable<PlayerInfo>> GetAllPlayersAsync()
        {
            string query = $"SELECT * FROM {playerTable}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync<PlayerInfo>(query);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default;
            }
        }
        #endregion

        #region Teams CRUD
        public async Task<int> AddTeamAsync(TeamInfo team)
        {
            string query = "INSERT INTO teams (TeamID, TeamName, Player1, Player2, Wins, Losses) VALUES (@TeamID, @TeamName, @Player1, @Player2, @Wins, @Losses)";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query, team);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<TeamInfo> GetTeamAsync(long teamID)
        {
            string query = $"SELECT * FROM {playerTable} WHERE TeamID = {teamID}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync(query).Result.FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> UpdateTeamAsync(TeamInfo team)
        {
            string query = $"UPDATE {teamTable} SET TeamName=@TeamName, Player1=@Player1, Player2=@Player2, Wins=@Wins, Losses=@Losses WHERE TeamID=@TeamID";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query, team);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> DeleteTeamAsync(long teamID)
        {
            string query = $"DELETE from {teamTable} WHERE TeamID = {teamID}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query);
                }
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region Teams Extra
        public async Task<TeamInfo> GetTeamByDiscordIDAsync(long discordID)
        {
            string query = $"SELECT * FROM {teamTable} WHERE (Player1 = {discordID}) OR (Player2 = {discordID})";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync(query).Result.FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<IEnumerable<TeamInfo>> GetAllTeamsAsync()
        {
            string query = $"SELECT * FROM {teamTable}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync<TeamInfo>(query);
                }
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region Matches CRUD
        public async Task<int> AddMatchAsync(MatchInfo match)
        {
            string query = $"INSERT INTO {matchTable} (MatchID, TeamHomeID, TeamAwayID, ScoreHome, ScoreAway, MatchFinished, MapID) VALUES (@MatchID, @TeamHomeID, @TeamAwayID, @ScoreHome, @ScoreAway, @MatchFinished, @MapID)";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query, match);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<MatchInfo> GetMatchAsync(long matchID)
        {
            string query = $"SELECT * FROM {matchTable} WHERE TeamID = {matchID}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync(query).Result.FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> UpdateMatchAsync(MatchInfo match)
        {
            string query = $"UPDATE {matchTable} SET TeamHomeID=@TeamHomeID, TeamAwayID=@TeamAwayID, ScoreHome=@ScoreHome, ScoreAway=@ScoreAway, MatchFinished=@MatchFinished, MapID=@MapID WHERE MatchID=@MatchID";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query, match);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> DeleteMatchAsync(long matchID)
        {
            string query = $"DELETE from {matchTable} WHERE TeamID = {matchID}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query);
                }
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region Matches Extra
        public async Task<IEnumerable<MatchInfo>> GetMatchesFromPlayerAsync(long discordID, bool finished)
        {
            PlayerInfo player = await GetPlayerAsync(discordID);
            string query = $"SELECT * FROM {matchTable} WHERE (TeamHome = {player.TeamID}) OR (TeamAway = {player.TeamID}) AND (MatchFinished = {finished}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync<MatchInfo>(query);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<IEnumerable<MatchInfo>> GetAllMatchesAsync()
        {
            string query = $"SELECT * FROM {matchTable}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync<MatchInfo>(query);
                }
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region Maps CRUD
        public async Task<int> AddMapAsync(MapInfo map)
        {
            string query = $"INSERT INTO {mapTable} (MapID, MapName, WorkshopID, OfficialID, OfficialMap) VALUES (@MapID, @MapName, @WorkshopID, @OfficialID, @OfficialMap)";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query, map);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<MapInfo> GetMapAsync(long mapID)
        {
            string query = $"SELECT * FROM {mapTable} WHERE MapID = {mapID}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync(query).Result.FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> UpdateMapAsync(MapInfo map)
        {
            string query = $"UPDATE {mapTable} SET MapName=@MapName, WorkshopID=@WorkshopID, OfficialID=@OfficialID, OfficialMap=@OfficialMap WHERE MapID=@MapID";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query, map);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> DeleteMapAsync(long mapID)
        {
            string query = $"DELETE from {mapTable} WHERE TeamID = {mapID}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query);
                }
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region Maps Extra
        public async Task<IEnumerable<MapInfo>> GetAllMapsAsync()
        {
            string query = $"SELECT * FROM {mapTable}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync<MapInfo>(query);
                }
            }
            catch
            {
                return default;

            }
        }

        [Obsolete("Use MapIDs instead")]
        public async Task<MapInfo> GetMapByNameAsync(string mapName)
        {
            string query = $"SELECT * FROM {mapTable} WHERE MapName = {mapName}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync(query).Result.FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }
        #endregion

        private async Task<IEnumerable<T>> GetTableFromDBUsing<T>(string strQuery, string databaseName = null)
        {
            strQuery = SanitizeString(strQuery);

            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync<T>(strQuery);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default;
            }
        }

        //We might need to whitelist more characters in the future
        private static string SanitizeString(string s)
        {
            if (s == null) return null;
            string str = new((from c in s where char.IsWhiteSpace(c) 
                                     || char.IsLetterOrDigit(c)
                                     || c == '_' || c == '*' || c == '=' select c)
                                     .ToArray());
            return str;
        }
    }
}
