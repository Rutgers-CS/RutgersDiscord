using Dapper;
using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace RutgersDiscord.Handlers
{
    public class DatabaseHandler
    {
        //Constants. maybe move them in the future
        const string matchTable = "match_list";
        const string playerTable = "player_list";
        const string teamTable = "team_list";

        static readonly string m_strMySQLConnectionString = Environment.GetEnvironmentVariable("dbConnectionString");

        //TEMPORARY method. In the future we will only allow public methods here to output predefined types
        public IEnumerable<T> GetTable<T>(string strQuery, string databaseName = null)
        {
            return GetTableFromDBUsing<T>(strQuery, databaseName);
        }


        public IEnumerable<MatchInfo> GetMatchInfo(ulong discordID, bool matchFinished = false)
        {
            PlayerInfo player = GetPlayerInfo(discordID);
            return GetTableFromDBUsing<MatchInfo>($"SELECT * FROM {matchTable} " +
                                                  $"WHERE (teamHome = {player.TeamID} OR " +
                                                  $"teamAway = {player.TeamID}) AND " +
                                                  $"matchFinished = {matchFinished}");
        }

        //Retrieves player info search either with discordID or SteamID
        public PlayerInfo GetPlayerInfo(ulong discordID)
        {
            return GetTableFromDBUsing<PlayerInfo>($"SELECT * FROM {playerTable} WHERE discordID = {discordID}").First();
        }

        public PlayerInfo GetPlayerBySteam(ulong steamID)
        {
            return GetTableFromDBUsing<PlayerInfo>($"SELECT * FROM {playerTable} WHERE steamID = {steamID}").First();
        }
        

        public TeamInfo GetTeamByUser(ulong discordID, bool captainOnly)
        {
            if(captainOnly)
            {
                return GetTableFromDBUsing<TeamInfo>($"SELECT * FROM {teamTable} " +
                                                       $"WHERE player1 = {discordID}").First();
            }
            return GetTableFromDBUsing<TeamInfo>($"SELECT * FROM {teamTable} " +
                                                 $"WHERE player1 = {discordID} OR " +
                                                 $"player2 = {discordID}").First();
        }

        public TeamInfo GetTeamById(ulong teamID)
        {
            return GetTableFromDBUsing<TeamInfo>($"SELECT * FROM {teamTable} " +
                                                 $"WHERE teamID = {teamID}").First();
        }

        private IEnumerable<T> GetTableFromDBUsing<T>(string strQuery, string databaseName = null)
        {
            IEnumerable<T> outputList;
            strQuery = SanitizeString(strQuery);
            if(databaseName != null)
            {
                databaseName = SanitizeString(databaseName);
            }
            string connectionString;

            if (databaseName == null)
            {
                connectionString = m_strMySQLConnectionString + "database=" + Environment.GetEnvironmentVariable("defaultDatabase");
            }
            else
            {
                connectionString = m_strMySQLConnectionString + "database=" + databaseName;
            }


            try
            {
                using var mysqlconnection = new MySqlConnection(connectionString);
                outputList = mysqlconnection.Query<T>(strQuery);
            }
            catch (MySqlException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }

            return outputList;
        }

        //We might need to whitelist more characters in the future
        private string SanitizeString(string s)
        {
            string str = new string((from c in s where char.IsWhiteSpace(c) 
                                     || char.IsLetterOrDigit(c)
                                     || c == '_' || c == '*' || c == '=' select c)
                                     .ToArray());
            return str;
        }
    }
}
