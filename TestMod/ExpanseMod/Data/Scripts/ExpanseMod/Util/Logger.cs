using ExpanseMod.Models;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Utils;

namespace ExpanseMod.Util
{
    public static class Logger
    {
        public static string LogName = "ExpanseMod";
        public static string LogFilePath = "ExpanseLog";
        public static string StoragePath = "ExpanseStorage";

        private static TextWriter _logWriter;
        private static TextWriter _objectWriter;

        private static TextWriter InitWriter(string fileName)
        {
            TextWriter writer;
            if (MyAPIGateway.Utilities.FileExistsInLocalStorage(fileName, typeof(Logger)))
            {
                var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(fileName, typeof(Logger));

                var log = reader.ReadToEnd();

                reader.Close();

                writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(fileName, typeof(Logger));
                writer.WriteLine(log);
                writer.Flush();
            }
            else
                writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(fileName, typeof(Logger));

            return writer;
        }

        public static void Init()
        {
            _logWriter = InitWriter($"{LogFilePath}{DateTime.Now:yyyyMMdd}.log");
            _objectWriter = InitWriter($"{StoragePath}{DateTime.Now:yyyyMMdd}.xml");
        }

        public static void Log(string msg)
        {
            try
            {
                var whoAmI = MyAPIGateway.Session?.IsServer == true ? "Server" : "Player ID: " + (MyAPIGateway.Session?.Player != null ? MyAPIGateway.Session?.Player?.IdentityId : -1);

                MyLog.Default.WriteLineAndConsole($"[{LogName}] [{whoAmI}] {msg}");

                _logWriter.WriteLine($"[{whoAmI}] {msg}");
                _logWriter.Flush();
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLineAndConsole($"[{LogName}] [???] Unable to log message: {msg}. Exception {ex.Message} {ex.StackTrace}");
            }
        }

        public static void LogObject(LogEntry entry)
        {
            try
            {
                var data = MyAPIGateway.Utilities.SerializeToXML(entry);
                _objectWriter.WriteLine(data);
                _objectWriter.Flush();
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLineAndConsole($"[{LogName}] Unable to log object. ex: {ex.Message} {ex.StackTrace}");
            }
        }

        public static void Close()
        {
            try
            {
                _logWriter.Flush();
                _logWriter.Close();

                _objectWriter.Flush();
                _objectWriter.Close();
            }
            catch(Exception ex)
            {
                MyLog.Default.WriteLineAndConsole($"[{LogName}] Unable to close text writers. ex: {ex.Message} {ex.StackTrace}");
            }
        }
    }
}
