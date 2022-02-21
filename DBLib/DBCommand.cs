using Microsoft.SqlServer.Management.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLib
{

    public class DBCommand
    {
        private DBUtils db;
        private LogSqlMessage log;
        public DBCommand(LogSqlMessage logSqlMessage)
        {
            db = new DBUtils(logSqlMessage);
            log = logSqlMessage;
        }
        public string RunCommand(string query, string database)
        {
            string final = $"USE [{database}];" +
                            query;
            try
            {
                db.ExecuteNonQuery(final);
            }
            catch (ExecutionFailureException ex)
            {
                return ex.Message;
            }
            return null;
        }
        public List<string> GetAllDatabaseNames()
        {
            var result = new List<string>();
            DbDataReader reader = null;
            try
            {
                reader = db.ExecuteReader("SELECT name from sys.databases");
                while (reader.Read())
                {
                    var name = reader[0].ToString();
                    result.Add(name);
                }
            }
            catch (Exception ex)
            {
                log(ex.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return result;
        }
    }

}
