using System;
using System.IO;

namespace KeywordDriven.Utils
{
    public class Log
    {
        private static string _filepath;

        private static object _setLoggerLock = new object();

        public static void SetLogger(string filepath)
        {
            lock (_setLoggerLock)
            {
                _filepath = filepath;

                using (var log = File.CreateText(_filepath))
                {
                    log.WriteLine($"Starting timestamp: {DateTime.Now.ToLocalTime()}");
                }
            }
        }

        private static void WriteLine(string text)
        {
            using (var log = File.AppendText(_filepath))
            {
                log.WriteLine(text);
            }
        }

        private static void Write(string text)
        {
            using (var log = File.AppendText(_filepath))
            {
                log.Write(text);
            }
        }      
        
        internal static void StartTestCase(String sTestCaseName)
        {
            Info("Start TestCase " + sTestCaseName);
        }
        
        internal static void EndTestCase(String sTestCaseName)
        {
            Info("End TestCase " + sTestCaseName);
            WriteLine($"-------------------------");
        }
        
        internal static void Info(string message)
        {
            WriteLine($"[INFO]: {message}");
        }
        
        internal static void Step(string message)
        {
            WriteLine($"    [STEP]: {message}");
        }
       
        internal static void Warning(string message)
        {
            WriteLine($"[WARNING]: {message}");
        }
     
        internal static void Error(string message)
        {
            WriteLine($"[ERROR]: {message}");
        }
        
        internal static void Fatal(string message)
        {
            WriteLine($"[FATAL]: {message}");
        }
        
        internal static void Debug(string message)
        {
            WriteLine($"[DEBUG]: {message}");
        }
    }
}
