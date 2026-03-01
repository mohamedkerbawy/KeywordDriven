using System.IO;
using NUnit.Framework;
using KeywordDriven.Config;
using KeywordDriven.Utils;
using KeywordDriven.Execution;

namespace KeywordDriven.Tests
{
    public class MainScript
    {

        public static string Path_TestDefinition = Path.Combine(PathSetting.projectDir, @"TestDefinition\TestCases.xlsx");
        public static string Path_Log = Path.Combine(PathSetting.projectDir, @"TestLogs\log.txt");
        public static string Path_Report = Path.Combine(PathSetting.projectDir, @"TestReports\index.html");
        public static string Path_ScreenShots = Path.Combine(PathSetting.projectDir, @"ScreenShots\");

        [SetUp]
        public void TestSetUp()
        {

            ExcelManager.SetExcel(Path_TestDefinition);
            Log.SetLogger(Path_Log);
            ExtentReporter.SetExtentReporter(Path_Report);
            ScreenShot.SetScreenShot(Path_ScreenShots);

            DriverSetting.DriverType = "local";
            DriverSetting.Headless = false;
            DriverSetting.DefaultTimeoutSeconds = 10;
            DriverSetting.PageLoadTimeoutSeconds = 60;
        }

        [Test]
        public void TestCases()
        {       
            DriverScript.Execute_TestCases();   
        }

        [TearDown]
        public void TestTearDown()
        {
            ExtentReporter.Flush();
            ExcelManager.SaveCloseExcel();
        }
    }
}