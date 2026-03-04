using System;
using System.IO;

namespace KeywordDriven.Utils
{
    public enum LogLevel
    {
        STEP,
        INFO,
        PASS,
        FAIL,
        WARNING,
        ERROR,
        FATAL,
        DEBUG
    }

    public class Log
    {
        private static string _filepath;

        private static object _setLoggerLock = new object();

        public static void SetLogger(string logDirectory, string logFileName = null)
        {
            if (!Directory.Exists(Path.Combine(logDirectory))) 
                Directory.CreateDirectory(Path.Combine(logDirectory));

            logFileName ??= $"TestLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            _filepath = Path.Combine(logDirectory, logFileName);
        }

        internal static void WriteLog(LogLevel level, string message)
        {
            lock (_setLoggerLock)
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level,-7}] {message}";

                // Write to Console (for IDE output)
                Console.WriteLine(logEntry);

                // Write to File
                if (!string.IsNullOrEmpty(_filepath))
                {
                    File.AppendAllLines(_filepath, [logEntry]);
                }
            }
        }
        
        internal static void StartTestCase(String sTestCaseName)
        {
            Info($"Start TestCase {sTestCaseName}");
        }
        
        internal static void EndTestCase(int outcome, String sTestCaseName)
        {
            if (outcome == 0)
                Pass($"End TestCase {sTestCaseName}, Result: PASS");
            else if (outcome == 1)
                Fail($"End TestCase {sTestCaseName}, Result: FAIL");
            else if (outcome == 2)
                Error($"End TestCase {sTestCaseName}, Result: ERROR");
            else
                Info($"End TestCase {sTestCaseName}");
        }

        internal static void Info(string message) => WriteLog(LogLevel.INFO, message);
        
        internal static void Pass(string message) => WriteLog(LogLevel.PASS, message);
        
        internal static void Fail(string message) => WriteLog(LogLevel.FAIL, message);
        
        internal static void Warning(string message) => WriteLog(LogLevel.WARNING, message);
        
        internal static void Error(string message) => WriteLog(LogLevel.ERROR, message);
        
        internal static void Fatal (string message) => WriteLog(LogLevel.FATAL, message);
    }
}
