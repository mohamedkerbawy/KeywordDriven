using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;
using System;
using System.IO;

namespace KeywordDriven.Utils
{
    public class ExtentReporter
    {
        private static ExtentReports extent;
        private static ExtentTest testcase;
        private static ExtentTest node;

        private static string _reportPath;

        public static void SetExtentReporter(string reportDirectory, string reportName = null)
        {
            if (!Directory.Exists(Path.Combine(reportDirectory)))
                Directory.CreateDirectory(Path.Combine(reportDirectory));

            reportName ??= $"TestReport_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            _reportPath = Path.Combine(reportDirectory, reportName);

            var htmlReporter = new ExtentSparkReporter(_reportPath);

            // Report Config

            htmlReporter.Config.ReportName = "Automation Test Report";
            htmlReporter.Config.DocumentTitle = "Test Results";
            htmlReporter.Config.Theme = Theme.Dark;
            htmlReporter.Config.TimeStampFormat = "MM/dd/yyyy HH:mm:ss";
            htmlReporter.Config.Encoding = "utf-8";

            // System Info (shown in report dashboard)
            extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
            extent.AddSystemInfo("Machine", Environment.MachineName);
            extent.AddSystemInfo(".NET Version", Environment.Version.ToString());
            extent.AddSystemInfo("Executed By", Environment.UserName);

            extent = new ExtentReports();
            extent.AttachReporter(htmlReporter);
        }

        public static void Flush()
        {
            extent.Flush();
            Console.WriteLine($"Report generated: {_reportPath}");
        }

        internal static void CreateTest(String sTestCaseID, String sTestCaseTitle)
        {
            testcase = extent.CreateTest(sTestCaseID,sTestCaseTitle);
        }

        internal static void CreateNode(String sTestStepNo, string description = null)
        {
            node = testcase.CreateNode(sTestStepNo, description);
        }

        internal static void StartTestCase(String sTestCaseName)
        {
            testcase.Log(Status.Info,"Start TestCase " + sTestCaseName);
        }

        internal static void EndTestCase(int outcome, String sTestCaseName)
        {
            if (outcome == 0)
                Pass("End TestCase " + sTestCaseName);
            else if (outcome == 1)
                Fail("End TestCase " + sTestCaseName);
            else if (outcome == 2)
                Error("End TestCase " + sTestCaseName);
            else
                Info("End TestCase " + sTestCaseName);
        }

        internal static void Info(String message)
        {
            testcase.Log(Status.Info, message);
        }

        internal static void Pass(String message)
        {
            testcase.Log(Status.Pass, message);
        }

        internal static void Fail(String message)
        {
            testcase.Log(Status.Fail,message);
        }

        internal static void Error(String message)
        {
            testcase.Log(Status.Error,message);
        }

        internal static void Warn(String message)
        {
            testcase.Log(Status.Warning,message);
        }

        internal static void AddScreenShot(String path)
        {
            testcase.AddScreenCaptureFromPath(path);
        }
        
        internal static void NodeInfo(String message)
        {
            node.Log(Status.Info,message);
        }

        internal static void NodePass(String message)
        {
            node.Log(Status.Pass,message);
        }

        internal static void NodeFail(String message)
        {
            node.Log(Status.Fail,message);
        }

        internal static void NodeError(String message)
        {
            node.Log(Status.Error, message);
        }

        internal static void NodeWarn(String message)
        {
            node.Log(Status.Warning,message);
        }

        internal static void NodeAddScreenShot(String path)
        {
            node.AddScreenCaptureFromPath(path);
        }

        internal static void AssignCategory(params string[] categories)
        {
            foreach (var cat in categories)
                testcase.AssignCategory(cat);
        }

        internal static void AssignAuthor(params string[] authors)
        {
            foreach (var author in authors)
                testcase.AssignAuthor(author);
        }
    }
}
