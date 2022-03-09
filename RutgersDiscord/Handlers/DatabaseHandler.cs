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
        static readonly string m_strMySQLConnectionString = Environment.GetEnvironmentVariable("dbConnectionString");

        //TEMPORARY method. In the future we will only allow public methods here to output predefined types
        public IEnumerable<T> GetTable<T>(string strQuery, string databaseName = null)
        {
            return GetTableFromDBUsing<T>(strQuery, databaseName);
        }


        private IEnumerable<T> GetTableFromDBUsing<T>(string strQuery, string databaseName = null)
        {
            IEnumerable<T> outputList;
            strQuery = SanitizeString(strQuery);
            databaseName = SanitizeString(databaseName);
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
