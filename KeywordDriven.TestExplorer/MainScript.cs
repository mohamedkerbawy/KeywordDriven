using System.IO;
using NUnit.Framework;
using KeywordDriven.Config;
using KeywordDriven.Utils;
using KeywordDriven.Execution;

namespace KeywordDriven.TestExplorer
{
    public class MainScript
    {
        [SetUp]
        public void TestSetUp()
        {
            ExcelManager.SetExcel(@"D:\Bravo\Creation\TestCases.xlsx");
            ExtentReporter.SetExtentReporter(@"D:\Bravo\TestResults\index.html");
            Log.SetLogger(@"D:\Bravo\Logs\log.txt");

            DriverSetting.WebDriver("local", 20, 200, false);
            DriverSetting.AndroidDriver("Pixel_3a_API_30_x86", "emulator-5554", "11", @"D:\Bravo\Resources\Bravo-Product-v5.1.4.apk");
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