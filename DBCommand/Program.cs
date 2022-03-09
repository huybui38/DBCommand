
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
            [Option('s', "sql", HelpText = "Input SQL file to be processed.")]
            public IEnumerable<string> InputSqlFiles { get; set; }

            [Option(
                'p', "prefix",
              Required = true,
              HelpText = "Input prefix database to be processed.")]
            public IEnumerable<string> Prefix { get; set; }

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
            if (opts.InputSqlFiles.Count() == 0 && opts.Command == null)
            {
                logMessage("command or sql file is missing!");
                return;
            }
            if (opts.Prefix.Count() == 0)
            {
                logMessage("Prefix is missing!");
                return;
            }
            bool isExcluded = opts.ExcludedDatabases.Any();
            var dbCommand = new DBCommand(logSqlMessage);
            var list = dbCommand.GetAllDatabaseNames();
            if (list is null)
            {
                logMessage("Không thể đọc dữ liệu!");
                return;
            }
            var filteredResult = new List<string>();
            foreach (var prefix in opts.Prefix)
            {
                var filtered = list.Where(a => a.StartsWith(prefix)).ToList()
                                    .Except(opts.ExcludedDatabases).ToList();
                filteredResult.AddRange(filtered);
            }
            filteredResult = filteredResult.Distinct().ToList();
            if (filteredResult.Count() == 0) {
                logMessage("Empty database list!");
                return;
            }
            logMessage("Database List:");
            filteredResult.ForEach(a => logMessage(a));
            try
            {
                if (opts.InputSqlFiles.Count() != 0)
                {
                    var inputSqlFiles = opts.InputSqlFiles.ToList();
                    for (int i = 0; i < inputSqlFiles.Count; i++)
                    {
                        string sqlText = File.ReadAllText(inputSqlFiles[i]);
                        foreach (var dbName in filteredResult)
                        {
                            logMessage($"Running {inputSqlFiles[i]} ", dbName);
                            RunCommand(sqlText, dbName, dbCommand);
                        }
                    }
                }
                if (opts.Command != null)
                {
                    foreach (var dbName in filteredResult)
                    {
                        logMessage($"Running command", dbName);
                        RunCommand(opts.Command, dbName, dbCommand);
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
