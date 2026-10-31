using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeywordDriven.Config
{
    public class PathSetting
    {
        public static string AssemblyDir { get; }= Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        
        public static string ProjectDir { get; }= Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(AssemblyDir)));

        public static string Path_TestDefinitionDir { get; }= Path.Combine(PathSetting.ProjectDir, @"TestDefinition");
        public static string Path_LogDir { get; }= Path.Combine(PathSetting.ProjectDir, @"TestLogs");
        public static string Path_ReportDir { get; }= Path.Combine(PathSetting.ProjectDir, @"TestReports");
        public static string Path_ScreenShotsDir { get; }= Path.Combine(PathSetting.ProjectDir, @"ScreenShots");

    }
}
