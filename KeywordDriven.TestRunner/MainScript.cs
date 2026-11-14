using KeywordDriven.Config;
using KeywordDriven.Execution;
using KeywordDriven.Utils;
using NUnit.Framework;
using System;
using System.IO;

namespace KeywordDriven.TestRunner
{
    public class MainScript
    {
        [SetUp]
        public void TestSetUp()
        {
            ExcelManager.SetExcel(PathSetting.Path_TestDefinitionDir, "TestCases");
            
            Log.SetLogger(PathSetting.Path_LogDir);
            
            ExtentReporter.SetExtentReporter(PathSetting.Path_ReportDir);

            DriverSetting.DriverType = "local";
            DriverSetting.Headless = false;
            DriverSetting.DefaultTimeoutSeconds = 50;
            DriverSetting.PageLoadTimeoutSeconds = 60;
        }

        [Test]
        public void TestCases()
        {
            Assert.That(
                DriverScript.Execute_TestCases(),
                Is.True,
                "One or more enabled Excel test cases failed or contained errors. See the workbook results and test logs.");
        }

        [TearDown]
        public void TestTearDown()
        {
            ExtentReporter.Flush();
            ExcelManager.SaveCloseExcel();
        }
    }
}