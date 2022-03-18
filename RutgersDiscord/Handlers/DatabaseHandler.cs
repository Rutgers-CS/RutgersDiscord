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

        const string databaseName = "Data Source=database.db";

        public async Task AddTestData()
        {
            PlayerInfo august = new PlayerInfo(222897049899630592, 76561198049694740, "STEAM_1:0:44714506", "August", 1, null, null);
            PlayerInfo kenji = new PlayerInfo(164803512633524225, 76561198064565542, "STEAM_1:0:52149907", "Kenji", 1, null, null);
            PlayerInfo gal = new PlayerInfo(171431827746193410, 76561198143013155, "STEAM_1:1:91373713", "Galifi", 2, null, null);
            PlayerInfo park = new PlayerInfo(953059699627012142, 76561198405017903, "STEAM_1:1:222376087", "Andrew", 2, null, null);

            TeamInfo team1 = new TeamInfo(1, "Team1", 222897049899630592, 164803512633524225, 0, 0);
            TeamInfo team2 = new TeamInfo(2, "Team2", 171431827746193410, 953059699627012142, 0, 0);

            //MatchInfo match = new MatchInfo(0, 1, 2, null, null,null, false, null, null);

            await AddPlayerAsync(august);
            await AddPlayerAsync(kenji);
            await AddPlayerAsync(gal);
            await AddPlayerAsync(park);

            await AddTeamAsync(team1);
            await AddTeamAsync(team2);

            //await AddMatchAsync(match);
        }

        //TEMPORARY method. In the future we will only allow public methods here to output predefined types
        [Obsolete("Don't use it")]
        public async Task<IEnumerable<T>> GetTable<T>(string strQuery, string databaseName = null)
        {
            return await GetTableFromDBUsing<T>(strQuery, databaseName);
        }

        #region Players
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
                    return (await sqliteConnection.QueryAsync<PlayerInfo>(query)).FirstOrDefault();
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
        public async Task<IEnumerable<PlayerInfo>> GetPlayerByAttribute(long? steam64ID = null, string steamID = null, string name = null, long? teamID = null, int? kills = null, int? deaths = null)
        {
            string filter = "true ";
            if (steam64ID != null)
            {
                filter += $"AND Steam64ID = {steam64ID} ";
            }
            if (steamID != null)
            {
                filter += $"AND SteamID = {steamID} ";
            }
            if (name != null)
            {
                filter += $"AND Name = {name} ";
            }
            if (teamID != null)
            {
                filter += $"AND TeamID = {teamID}";
            }
            if (kills != null)
            {
                filter += $"AND Kills = {kills} ";
            }
            if (deaths != null)
            {
                filter += $"AND Deaths = {deaths}";
            }
            return await GenericQueryAsync<PlayerInfo>($"SELECT * FROM {playerTable} WHERE {filter}");
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
        #endregion

        #region Teams
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
            string query = $"SELECT * FROM {teamTable} WHERE TeamID = {teamID}";
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
        [Obsolete("Switch to GetTeamsByAttribute()")]
        public async Task<TeamInfo> GetTeamByDiscordIDAsync(long discordID)
        {
            string query = $"SELECT * FROM {teamTable} WHERE (Player1 = {discordID}) OR (Player2 = {discordID})";
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

        public async Task<IEnumerable<TeamInfo>> GetTeamsByAttributeAsync(long? teamID = null, string teamName = null, long? playerID = null, int? wins = null, int? losses = null)
        {
            string filter = "true ";
            if (teamID != null)
            {
                filter += $"AND TeamID = {teamID} ";
            }
            if (teamName != null)
            {
                filter += $"AND TeamName = {teamName} ";
            }
            if (playerID != null)
            {
                filter += $"AND (Player1 = {playerID} OR Player2 = {playerID}) ";
            }
            if (wins != null)
            {
                filter += $"AND Wins = {wins} ";
            }
            if (losses != null)
            {
                filter += $"AND Losses = {losses}";
            }
            return await GenericQueryAsync<TeamInfo>($"SELECT * FROM {teamTable} WHERE {filter}");
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
        #endregion

        #region Matches
        #region Matches CRUD
        public async Task<int> AddMatchAsync(MatchInfo match)
        {
            string query = $"INSERT INTO {matchTable} (MatchID, TeamHomeID, TeamAwayID, ScoreHome, ScoreAway, MatchFinished, MapID, DiscordChannel) VALUES (@MatchID, @TeamHomeID, @TeamAwayID, @ScoreHome, @ScoreAway, @MatchFinished, @MapID, @DiscordChannel)";
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
                    return (await sqliteConnection.QueryAsync<MatchInfo>(query)).FirstOrDefault();
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> UpdateMatchAsync(MatchInfo match)
        {
            string query = $"UPDATE {matchTable} SET TeamHomeID=@TeamHomeID, TeamAwayID=@TeamAwayID, ScoreHome=@ScoreHome, ScoreAway=@ScoreAway, MatchFinished=@MatchFinished, MapID=@MapID, DiscordChannel=@DiscordChannel WHERE MatchID=@MatchID";
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

        //TODO: Add more attributes here later I'm lazy
        public async Task<IEnumerable<MatchInfo>> GetMatchByAttribute(long? teamID1 = null, long? teamID2 = null, long? matchTime = null, bool? matchFinished = null)
        {
            string filter = "true ";
            if (teamID1 != null)
            {
                filter += $"AND (teamHomeID = {teamID1} OR teamAwayID = {teamID1}) ";
            }
            if (teamID2 != null)
            {
                filter += $"AND (teamHomeID = {teamID2} OR teamAwayID = {teamID2}) ";
            }
            if (matchTime != null)
            {
                filter += $"AND matchTime = {matchTime} ";
            }
            if (matchFinished != null)
            {
                filter += $"AND matchFinished = {matchFinished}";
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
        #endregion

        #region Maps
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

        public async Task<IEnumerable<MatchInfo>> GetMapsByAttribute(long? mapID = null, string mapName = null, long? workshopID = null, string officialID = null, bool? officialMap = null)
        {
            string filter = "true ";
            if (mapID != null)
            {
                filter += $"AND (MapID = {mapID}) ";
            }
            if (mapName != null)
            {
                filter += $"AND (MapName = {mapName}) ";
            }
            if (workshopID != null)
            {
                filter += $"AND WorkshopID = {workshopID} ";
            }
            if (officialID != null)
            {
                filter += $"AND OfficialID = {officialID} ";
            }
            if (officialMap != null)
            {
                filter += $"AND OfficialMap = {officialMap}";
            }
            return await GenericQueryAsync<MatchInfo>($"SELECT * FROM {mapTable} WHERE {filter}");
        }
        #endregion
        #endregion

        public async Task<IEnumerable<T>> GenericQueryAsync<T>(string query)
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

        public async Task<int> GenericExecuteAsync(string cmd)
        {
            cmd = SanitizeString(cmd);

            try
            {
                using (var sqliteConnection = new SqliteConnection(databaseName))
                {
                    return await sqliteConnection.ExecuteAsync(cmd);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default;
            }
        }

        [Obsolete("Use GenericQueryAsync() Instead")]
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

        //We might need to whitelist more characters in the future
        private static string SanitizeString(string s)
        {
            if (s == null) return null;
            string str = new((from c in s where char.IsWhiteSpace(c) 
                                     || char.IsLetterOrDigit(c)
                                     || c == '_' || c == '*' || c == '=' || c == '(' || c == ')' || c == '\\' || c == '"' select c)
                                     .ToArray());
            return str;
        }
    }
}
