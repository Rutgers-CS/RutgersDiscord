using Dapper;
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
        const string tokenTable = "servertokens";

        const string databaseName = "Data Source=database.db";

        public async Task<string> BackupDatabase()
        {
            string timestamp = DateTime.Now.ToString("MM-dd-HH-mm-ss");
            var fileName = $"{timestamp}.db";

            using (var sqliteConnection = new SqliteConnection(databaseName))
            {
                sqliteConnection.Open();
                using (var bckcon = new SqliteConnection($"Data Source={fileName}"))
                {
                    sqliteConnection.BackupDatabase(bckcon);
                    return fileName;
                }
            }
        }

        //TEMPORARY method. In the future we will only allow public methods here to output predefined types
        [Obsolete("Don't use it")]
        public async Task<IEnumerable<dynamic>> GetTable<T>(string strQuery, string databaseName = databaseName)
        {
            strQuery = SanitizeString(strQuery);
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync(strQuery);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default;
            }
        }

        #region Players CRUD
        public async Task<int> AddPlayerAsync(PlayerInfo player)
        {
            //string query = $"INSERT INTO {playerTable} (DiscordID, Steam64ID, SteamID, Name, TeamID, Kills, Deaths) VALUES (@DiscordID, @Steam64ID, @SteamID, @Name, @TeamID, @Kills, @Deaths)";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    //return await sqliteConnection.ExecuteAsync(query, player);
                    try
                    {
                        return await sqliteConnection.InsertAsync(player);
                    }
                    catch (SqliteException ex)
                    {
                        Console.WriteLine(ex);
                        return default;
                    }
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
            query = SanitizeString(query);
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return (await sqliteConnection.QueryAsync<PlayerInfo>(query)).FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> UpdatePlayerAsync(PlayerInfo player)
        {
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.UpdateAsync<PlayerInfo>(player);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> DeletePlayerAsync(PlayerInfo player)
        {
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.DeleteAsync(player);
                }
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region Players Extra
        public async Task<IEnumerable<PlayerInfo>> GetPlayerByAttribute(long? steam64ID = null, string steamID = null, string name = null, long? teamID = null, int? kills = null, int? deaths = null)
        {
            string filter = "true ";
            if (steam64ID != null)
            {
                filter += $"AND Steam64ID = {steam64ID} ";
            }
            if (steamID != null)
            {
                filter += $"AND SteamID = \"{steamID}\" ";
            }
            if (name != null)
            {
                filter += $"AND Name = \"{name}\" ";
            }
            if (teamID != null)
            {
                filter += $"AND TeamID = {teamID} ";
            }
            if (kills != null)
            {
                filter += $"AND Kills = {kills} ";
            }
            if (deaths != null)
            {
                filter += $"AND Deaths = {deaths}";
            }
            return await GetTableFromDBUsing<PlayerInfo>($"SELECT * FROM {playerTable} WHERE {filter}");
        }
        public async Task<PlayerInfo> GetPlayerBySteam64IDAsync(long steam64ID)
        {
            string query = $"SELECT * FROM {playerTable} WHERE Steam64ID = {steam64ID}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return (await sqliteConnection.QueryAsync<PlayerInfo>(query)).FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<PlayerInfo> GetPlayerBySteamIDAsync(string steamID)
        {
            string query = $"SELECT * FROM {playerTable} WHERE SteamID = \"{steamID}\"";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return (await sqliteConnection.QueryAsync<PlayerInfo>(query)).FirstOrDefault();
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
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.InsertAsync(team);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<TeamInfo> GetTeamAsync(int teamID)
        {
            string query = $"SELECT * FROM {teamTable} WHERE TeamID = {teamID}";
            query = SanitizeString(query);
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return (await sqliteConnection.QueryAsync<TeamInfo>(query)).FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> UpdateTeamAsync(TeamInfo team)
        {
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.UpdateAsync<TeamInfo>(team);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> DeleteTeamAsync(TeamInfo team)
        {
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.DeleteAsync(team);
                }
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region Teams Extra
        public async Task<TeamInfo> GetTeamByDiscordIDAsync(long discordID, bool captainOnly = false)
        {
            string query = $"SELECT * FROM {teamTable} WHERE (Player1 = {discordID})";
            if (!captainOnly)
            {
                query += $" OR (Player2 = {discordID})";
            }
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return (await sqliteConnection.QueryAsync<TeamInfo>(query)).FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<IEnumerable<TeamInfo>> GetTeamByAttribute(int? teamID = null, string teamName = null, long? player1 = null, long? player2 = null, int? wins = null, int? losses = null, int? roundWins = null, int? roundLosses = null)
        {
            string filter = "true ";
            if (teamID != null)
            {
                filter += $"AND TeamID = {teamID} ";
            }
            if (teamName != null)
            {
                filter += $"AND TeamName = \"{SanitizeString(teamName)}\" ";
            }
            if (player1 != null)
            {
                filter += $"AND Player1 = {player1} ";
            }
            if (player2 != null)
            {
                filter += $"AND Player2 = {player2} ";
            }
            if (wins != null)
            {
                filter += $"AND Wins = {wins} ";
            }
            if (losses != null)
            {
                filter += $"AND Losses = {losses} ";
            }
            if (roundWins != null)
            {
                filter += $"AND RoundWins = {roundWins} ";
            }
            if (roundLosses != null)
            {
                filter += $"AND RoundLosses = {roundLosses} ";
            }

            return await GetTableFromDBUsing<TeamInfo>($"SELECT * FROM {teamTable} WHERE {filter}");
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
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.InsertAsync<MatchInfo>(match);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<MatchInfo> GetMatchAsync(int matchID)
        {
            string query = $"SELECT * FROM {matchTable} WHERE TeamID = {matchID}";
            query = SanitizeString(query);
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return (await sqliteConnection.QueryAsync<MatchInfo>(query)).FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> UpdateMatchAsync(MatchInfo match)
        {
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.UpdateAsync<MatchInfo>(match);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> DeleteMatchAsync(MatchInfo match)
        {
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.DeleteAsync(match);
                }
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region Matches Extra

        public async Task<IEnumerable<MatchInfo>> GetMatchByAttribute(int? teamID1 = null, int? teamID2 = null, int? seriesID = null, long? matchTime = null, int? scoreHome = null, int? scoreAway = null, bool? matchFinished = null,bool? homeTeamWon = null, int? mapID = null, long? discordChannel = null, bool? teamHomeReady = null, bool? teamAwayReady = null, string datMatchID = null, string serverID = null)
        {
            string filter = "true ";
            if (teamID1 != null)
            {
                filter += $"AND (TeamHomeID = {teamID1} OR TeamAwayID = {teamID1}) ";
            }
            if (teamID2 != null)
            {
                filter += $"AND (TeamHomeID = {teamID2} OR TeamAwayID = {teamID2}) ";
            }
            if (seriesID != null)
            {
                filter += $"AND SeriesID = {seriesID} ";
            }
            if (matchTime != null)
            {
                filter += $"AND MatchTime = {matchTime} ";
            }
            if(scoreHome != null)
            {
                filter += $"AND ScoreHome = {scoreHome} ";
            }
            if(scoreAway != null)
            {
                filter += $"AND ScoreAway = {scoreAway} ";
            }
            if (matchFinished != null)
            {
                filter += $"AND MatchFinished = {matchFinished} ";
            }
            if(homeTeamWon != null)
            {
                filter += $"AND HomeTeamWon = {homeTeamWon} ";
            }
            if(mapID != null)
            {
                filter += $"AND MapID = {mapID} ";
            }
            if(discordChannel != null)
            {
                filter += $"AND DiscordChannel = {discordChannel} ";
            }
            if(teamHomeReady != null)
            {
                filter += $"AND TeamHomeReady = {teamHomeReady} ";
            }
            if (teamAwayReady != null)
            {
                filter += $"AND TeamAwayReady = {teamAwayReady} ";
            }
            if (datMatchID != null)
            {
                filter += $"AND DatMatchID = \"{datMatchID}\" ";
            }
            if (serverID != null)
            {
                filter += $"AND ServerID = \"{serverID}\" ";
            }
            return await GetTableFromDBUsing<MatchInfo>($"SELECT * FROM {matchTable} WHERE {filter}");
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

        public async Task<MapInfo> GetMapAsync(int mapID)
        {
            string query = $"SELECT * FROM {mapTable} WHERE MapID = {mapID}";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return (await sqliteConnection.QueryAsync<MapInfo>(query)).FirstOrDefault();
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

        public async Task<int> DeleteMapAsync(int mapID)
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
            string query = $"SELECT * FROM {mapTable} WHERE MapName = \"{mapName}\"";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return (await sqliteConnection.QueryAsync<MapInfo>(query)).FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }
        #endregion

        public async Task<ServerTokens> GetUnusedToken()
        {
            string query = $"SELECT * FROM {tokenTable} WHERE ServerID IS null";
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    var res = (await sqliteConnection.QueryAsync<ServerTokens>(query)).FirstOrDefault();
                    if (res != null)
                    {
                        return res;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> UpdateToken(ServerTokens token)
        {
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.UpdateAsync<ServerTokens>(token);
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<ServerTokens> GetTokenByServerID(string serverID)
        {
            string query = $"SELECT * FROM {tokenTable} WHERE ServerID = \"{serverID}\"";
            query = SanitizeString(query);
            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return (await sqliteConnection.QueryAsync<ServerTokens>(query)).FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        private async Task<IEnumerable<T>> GetTableFromDBUsing<T>(string strQuery, string databaseName = databaseName)
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

        public async Task<IEnumerable<T>> GenQuery<T>(string query)
        {
            query = SanitizeString(query);

            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.QueryAsync<T>(query);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default;
            }
        }

        public async Task<int> GenExec(string query)
        {
            query = SanitizeString(query);

            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(query);
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
                                     || c == '_' || c == '*' || c == '=' || c == '(' || c == ')' || c == '\\' || c == '"' || c == ':' || c == ',' || c == '<' || c == '>' select c)
                                     .ToArray());
            return str;
        }
    }
}
