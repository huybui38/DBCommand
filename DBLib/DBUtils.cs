using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLib
{
    public class DBUtils
    {
        private string _connStr;

        public LogSqlMessage _logSqlMessage { get; }

        public DBUtils(LogSqlMessage logSqlMessage)
        {
            LoadConnString();
            _logSqlMessage = logSqlMessage;
        }
        public bool LoadConnString()
        {
            try
            {
                NameValueCollection appSettings = ConfigurationManager.AppSettings;
                _connStr = appSettings.Get("ConnStr");
                return true;
            }
            catch (ConfigurationErrorsException ex)
            {
            }
            return false;
        }
        public SqlConnection GetSqlConnection()
        {
            return new SqlConnection(_connStr);


        }
        public DbDataReader ExecuteReader(string query)
        {
            SqlConnection conn = GetSqlConnection();
            var serverConnection = new ServerConnection(conn);
            var server = new Microsoft.SqlServer.Management.Smo.Server(serverConnection);
            return server.ConnectionContext.ExecuteReader(query);
        }

        void InfoMessageHandler(object sender, SqlInfoMessageEventArgs e)
        {
            string myMsg = e.Message;
            _logSqlMessage(myMsg);
        }
        public int ExecuteNonQuery(string query)
        {
            SqlCommand cmd = new SqlCommand(query);
            SqlConnection conn = GetSqlConnection();
            conn.InfoMessage += new SqlInfoMessageEventHandler(InfoMessageHandler);
            var serverConnection = new ServerConnection(conn);
            var server = new Microsoft.SqlServer.Management.Smo.Server(serverConnection);
            return server.ConnectionContext.ExecuteNonQuery(query);

        }
    }
}
