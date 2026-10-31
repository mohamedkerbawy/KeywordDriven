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
            if (string.IsNullOrWhiteSpace(reportDirectory))
                throw new ArgumentException("Report directory cannot be empty.", nameof(reportDirectory));

            string fullReportDirectory = Path.GetFullPath(reportDirectory);
            Directory.CreateDirectory(fullReportDirectory);

            reportName ??= $"TestReport_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            string reportPath = Path.GetFullPath(Path.Combine(fullReportDirectory, reportName));

            var htmlReporter = new ExtentSparkReporter(reportPath);

            htmlReporter.Config.ReportName = "Automation Test Report";
            htmlReporter.Config.DocumentTitle = "Test Results";
            htmlReporter.Config.Theme = Theme.Dark;
            htmlReporter.Config.TimeStampFormat = "MM/dd/yyyy HH:mm:ss";
            htmlReporter.Config.Encoding = "utf-8";

            var newExtent = new ExtentReports();
            newExtent.AttachReporter(htmlReporter);
            newExtent.AddSystemInfo("OS", Environment.OSVersion.ToString());
            newExtent.AddSystemInfo("Machine", Environment.MachineName);
            newExtent.AddSystemInfo(".NET Version", Environment.Version.ToString());
            newExtent.AddSystemInfo("Executed By", Environment.UserName);

            extent = newExtent;
            _reportPath = reportPath;
            testcase = null;
            node = null;
        }

        public static void Flush()
        {
            if (extent == null)
                return;

            extent.Flush();
            Console.WriteLine($"Report generated: {_reportPath}");
        }

        internal static void CreateTest(string testName, string description)
        {
            EnsureExtentInitialized();
            if (string.IsNullOrWhiteSpace(testName))
                throw new ArgumentException("Extent test name cannot be empty.", nameof(testName));

            testcase = extent.CreateTest(testName, description ?? string.Empty);
            node = null;
        }

        internal static void CreateNode(string stepName, string description = "")
        {
            EnsureTestCaseCreated();
            if (string.IsNullOrWhiteSpace(stepName))
                throw new ArgumentException("Extent test step name cannot be empty.", nameof(stepName));

            node = testcase.CreateNode(stepName, description ?? string.Empty);
        }

        internal static void StartTestCase(String sTestCaseName)
        {
            EnsureTestCaseCreated();
            testcase.Log(Status.Info,"Start TestCase " + sTestCaseName);
        }

        internal static void EndTestCase(int outcome, String sTestCaseName)
        {
            EnsureTestCaseCreated();
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
            EnsureTestCaseCreated();
            testcase.Log(Status.Info, message);
        }

        internal static void Pass(String message)
        {
            EnsureTestCaseCreated();
            testcase.Log(Status.Pass, message);
        }

        internal static void Fail(String message)
        {
            EnsureTestCaseCreated();
            testcase.Log(Status.Fail,message);
        }

        internal static void Error(String message)
        {
            EnsureTestCaseCreated();
            testcase.Log(Status.Error,message);
        }

        internal static void Warn(String message)
        {
            EnsureTestCaseCreated();
            testcase.Log(Status.Warning,message);
        }

        internal static void AddScreenShot(String path)
        {
            EnsureTestCaseCreated();
            testcase.AddScreenCaptureFromPath(path);
        }
        
        internal static void NodeInfo(String message)
        {
            EnsureNodeCreated();
            node.Log(Status.Info,message);
        }

        internal static void NodePass(String message)
        {
            EnsureNodeCreated();
            node.Log(Status.Pass,message);
        }

        internal static void NodeFail(String message)
        {
            EnsureNodeCreated();
            node.Log(Status.Fail,message);
        }

        internal static void NodeError(String message)
        {
            EnsureNodeCreated();
            node.Log(Status.Error, message);
        }

        internal static void NodeWarn(String message)
        {
            EnsureNodeCreated();
            node.Log(Status.Warning,message);
        }

        internal static void NodeAddScreenShot(String path)
        {
            EnsureNodeCreated();
            node.AddScreenCaptureFromPath(path);
        }

        internal static void AssignCategory(params string[] categories)
        {
            EnsureTestCaseCreated();
            foreach (var cat in categories)
                testcase.AssignCategory(cat);
        }

        internal static void AssignAuthor(params string[] authors)
        {
            EnsureTestCaseCreated();
            foreach (var author in authors)
                testcase.AssignAuthor(author);
        }

        private static void EnsureExtentInitialized()
        {
            if (extent == null)
                throw new InvalidOperationException("ExtentReporter is not initialized. Call SetExtentReporter first.");
        }

        private static void EnsureTestCaseCreated()
        {
            EnsureExtentInitialized();
            if (testcase == null)
                throw new InvalidOperationException("No Extent test case exists. Call CreateTest first.");
        }

        private static void EnsureNodeCreated()
        {
            EnsureTestCaseCreated();
            if (node == null)
                throw new InvalidOperationException("No Extent test step exists. Call CreateNode first.");
        }
    }
}
