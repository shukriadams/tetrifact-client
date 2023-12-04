using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TetrifactClient
{
    public class Log : ILog
    {
        #region FIELDS

        private string _logPath;

        private string _dataDirectory;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        public Log()
        {
            _dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Assembly.GetExecutingAssembly().GetName().Name);

            string logDirectory = PathHelper.GetLogsDirectory();
            _logPath = Path.Combine(logDirectory, $"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-log.txt");
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Deletes older log files
        /// </summary>
        public void Purge()
        {
            IEnumerable<FileInfo> files = Directory.GetFiles(_dataDirectory)
                .Where(r => r.EndsWith("-log.txt"))
                .Select(r => new FileInfo(r))
                .OrderByDescending(r => r.CreationTime)
                .Skip(5);

            foreach (FileInfo file in files)
                file.Delete();
        }

        public void LogInfo(string description = "")
        {
            Output(LogLevel.Info, null, description);
        }

        public void LogInfo(object item, string description = "")
        {
            Output(LogLevel.Info, item, description);
        }

        public void LogDebug(string description = "")
        {
            Output(LogLevel.Debug, null, description);
        }

        public void LogDebug(object item, string description = "")
        {
            Output(LogLevel.Debug, item, description);
        }

        public void LogError(string description = "") 
        {
            Output(LogLevel.Error, null, description);
        }

        public void LogError(object item, string description = "")
        {
            Output(LogLevel.Error, item, description);
        }

        public void LogUnstability(object item, string description = "")
        {
            Output(LogLevel.Error, item, description, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        /// <param name="description"></param>
        private void Output(LogLevel logLevel, object logObject, string description, bool writeToFile = true)
        {
            if (_logPath == null)
                return;

            if ((int)logLevel < (int)GlobalDataContext.Instance.LogLevel)
                return;

            string objectOut = string.Empty;
            if (logObject != null)
                objectOut = logObject.ToString();

            if (description == null)
                description = string.Empty;

            // log to other places
            Console.WriteLine($"{objectOut} {description}");

            //
            GlobalDataContext.Instance.Console.Add($"{objectOut} {description}");

            if (writeToFile)
            {
                try
                {
                    File.AppendAllText(_logPath, $"{DateTime.UtcNow.ToShortDateString()} {DateTime.UtcNow.ToLongTimeString()} {LogLevels.Error} : {description} {objectOut}\r\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        #endregion
    }
}
