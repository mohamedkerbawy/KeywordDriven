using KeywordDriven.Config;
using KeywordDriven.Execution;
using KeywordDriven.Utils;
using NUnit.Framework;
using System;
using System.IO;

namespace KeywordDriven.Tests
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