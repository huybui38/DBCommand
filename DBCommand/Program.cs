
using CommandLine;
using DBLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBCommandConsole
{
    public class Program
    {
        private static LogSqlMessage logSqlMessage = logMessage;
        public static void logMessage(string mess)
        {
            logMessage(mess, "sys");
        }
        public static void logMessage(string mess, string database)
        {
            Console.WriteLine($"[{database} - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]:" + mess);
        }
        class Options
        {
            [Option('s', "sql", Required = true, HelpText = "Input SQL file to be processed.")]
            public IEnumerable<string> InputSqlFiles { get; set; }

            [Option(
                'p', "prefix",
              Required = true,
              HelpText = "Input prefix database to be processed.")]
            public string Prefix { get; set; }

            [Option(
                'c', "command",
              Required = false,
              HelpText = "Input SQL command to be processed.")]
            public string Command { get; set; }

            [Option(
                  'e', "exclude",
                Required = false,
                HelpText = "Databases need to be excluded!")]
            public IEnumerable<string> ExcludedDatabases { get; set; }
        }

        static void Main(string[] args)
        {
            
            CommandLine.Parser.Default.ParseArguments<Options>(args)
              .WithParsed(RunOptions)
              .WithNotParsed(HandleParseError);
        }
        static void RunOptions(Options opts)
        {
            bool isExcluded = opts.ExcludedDatabases.Any();
            var dbCommand = new DBCommand(logSqlMessage);
            var list = dbCommand.GetAllDatabaseNames();
            if (list is null)
            {
                logMessage("Không thể đọc dữ liệu!");
                return;
            }
            var filtered = list.Where(a => a.StartsWith(opts.Prefix)).ToList()
                .Except(opts.ExcludedDatabases).ToList();
            logMessage("Database List:");
            if (filtered.Count() == 0) return;
            filtered.ForEach(a => logMessage(a));
            try
            {
                var inputSqlFiles = opts.InputSqlFiles.ToList();
                for (int i = 0; i < inputSqlFiles.Count; i++)
                {
                    string sqlText = File.ReadAllText(inputSqlFiles[i]);
                    foreach (var dbName in filtered)
                    {
                        logMessage($"Running {inputSqlFiles[i]} ", dbName);
                        RunCommand(sqlText, dbName, dbCommand);
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage(ex.Message);
            }
        }

        private static void RunCommand(string command, string database, DBCommand db)
        {
            var error = db.RunCommand(command, database);
            if (error != null)
            {
                logMessage(error, database);
                return;
            }
            logMessage("Run command succesfully!", database);
        }
        static void HandleParseError(IEnumerable<Error> errs)
        {
            //Console.ReadKey();
        }
    }
}
