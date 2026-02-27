using System.IO;
using NUnit.Framework;
using KeywordDriven.Config;
using KeywordDriven.Utils;
using KeywordDriven.Execution;

namespace KeywordDriven.Tests
{
    public class MainScript
    {
        public static string assemblyDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static string projectDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(assemblyDir)));
        
        public static string Path_TestDefinition = Path.Combine(projectDir, @"TestDefinition\TestCases.xlsx");
        public static string Path_Log = Path.Combine(projectDir, @"TestLogs\log.txt");
        public static string Path_Report = Path.Combine(projectDir, @"TestReports\index.html");

        [SetUp]
        public void TestSetUp()
        {

            ExcelManager.SetExcel(Path_TestDefinition);
            Log.SetLogger(Path_Log);
            ExtentReporter.SetExtentReporter(Path_Report);

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