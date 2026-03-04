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
        public static string AssemblyDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        
        public static string ProjectDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(AssemblyDir)));

        public static string Path_TestDefinitionDir = Path.Combine(PathSetting.ProjectDir, @"TestDefinition");
        public static string Path_LogDir = Path.Combine(PathSetting.ProjectDir, @"TestLogs");
        public static string Path_ReportDir = Path.Combine(PathSetting.ProjectDir, @"TestReports");
        public static string Path_ScreenShotsDir = Path.Combine(PathSetting.ProjectDir, @"ScreenShots");


    }
}
