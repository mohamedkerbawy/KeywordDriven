using System;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;

namespace KeywordDriven.Utils
{
    public class ExtentReporter
    {
        private static ExtentReports extent;
        private static ExtentTest testcase;
        private static ExtentTest node;

        public static void SetExtentReporter(string reportpath)
        {
            extent = new ExtentReports();
            
            var spark = new ExtentSparkReporter(reportpath);
            
            extent.AttachReporter(spark);
        }

        public static void Flush()
        {
            extent.Flush();
        }

        internal static void CreateTest(String sTestCaseID, String sTestCaseTitle)
        {
            //testcase = extent.CreateTest("TestCase: " + sTestCaseID, "Title: " + sTestCaseTitle);
            testcase = extent.CreateTest(sTestCaseID,sTestCaseTitle);
        }

        internal static void CreateNode(String sTestStepNo)
        {
            node = testcase.CreateNode(sTestStepNo);
        }

        internal static void StartTestCase(String sTestCaseName)
        {
            testcase.Log(Status.Info,"Start TestCase " + sTestCaseName);
        }

        internal static void EndTestCase(String sTestCaseName)
        {
            testcase.Log(Status.Info, "End TestCase " + sTestCaseName);
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
    }
}
