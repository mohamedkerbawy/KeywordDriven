using System.Reflection;
using KeywordDriven.Config;
using KeywordDriven.Utils;
using KeywordDriven.ActionKeywords;

namespace KeywordDriven.Execution
{
    //Results Outcome Enum
    public enum Outcome
    {
        Pass,
        Fail,
        Error
    }

    //Run Mode options Enum
    public enum RunMode
    {
        Yes,
        No
    }   

    public class DriverScript
    {
        //Public action keywords class object
        internal static Keywords actionKeywords;

        //reflection class object
        internal static MethodInfo[] method;

        //Locators variables
        internal static string actionKeyword;
        internal static string pageObject;

        //Test Cases variables
        internal static string testCaseID;
        internal static string testCaseTitle;
        internal static string testCaseDesc;
        internal static string runMode;

        //Test Steps variables
        internal static int testStep;
        internal static int testLastStep;
        internal static string testStepDesc;
        internal static string data;

        //result variable
        internal static int outcome;

        // static constructor to initialize the action keywords class object
        static DriverScript()
        {
            actionKeywords = new Keywords();
        }

        // This method will execute all the test cases and test steps based on the run mode value in the TestCases sheet
        public static void Execute_TestCases()
        {
            // get the total number of testcases from the TestCases sheet
            int totalTestCases = ExcelManager.GetRowCount(ExcelSetting.Sheet_TestCases);

            // This loop will execute number of times equal to total number of test cases
            for (int iTestcase = 1; iTestcase < totalTestCases; iTestcase++)
            {
                outcome = (int)Outcome.Pass;

                //get the testcase ID,Title,Description and run mode for the current test case from the TestCases sheet
                testCaseID = ExcelManager.GetCellData(iTestcase, ExcelSetting.Col_TestCases_ID, ExcelSetting.Sheet_TestCases);
                testCaseTitle = ExcelManager.GetCellData(iTestcase, ExcelSetting.Col_TestCases_Title, ExcelSetting.Sheet_TestCases);
                testCaseDesc = ExcelManager.GetCellData(iTestcase, ExcelSetting.Col_TestCases_Description, ExcelSetting.Sheet_TestCases);
                runMode = ExcelManager.GetCellData(iTestcase, ExcelSetting.Col_TestCases_RunMode, ExcelSetting.Sheet_TestCases);

                if (runMode != null && runMode.Equals(RunMode.Yes.ToString()))
                {
                    Log.StartTestCase(testCaseID);
                    ExtentReporter.CreateTest(testCaseID + "_" + testCaseTitle, testCaseDesc);
                    ExtentReporter.StartTestCase(testCaseID + "_" + testCaseTitle);

                    //get the first and last test step number for the current test case
                    testStep = ExcelManager.GetRowContains(testCaseID, ExcelSetting.Col_TestSteps_TestCaseID, ExcelSetting.Sheet_TestSteps);
                    testLastStep = ExcelManager.GetTestStepsCount(ExcelSetting.Sheet_TestSteps, testCaseID, testStep);

                    outcome = (int)Outcome.Pass;

                    // This loop will execute number of times equal to total number of test steps for the current test case
                    for (; testStep < testLastStep; testStep++)
                    {
                        //get the value of column ActionKeyword, pageObject, DataSet and Description for the current test step from the TestSteps sheet
                        actionKeyword = ExcelManager.GetCellData(testStep, ExcelSetting.Col_TestSteps_ActionKeyword, ExcelSetting.Sheet_TestSteps);
                        pageObject = ExcelManager.GetCellData(testStep, ExcelSetting.Col_TestSteps_PageObject, ExcelSetting.Sheet_TestSteps);
                        data = ExcelManager.GetCellData(testStep, ExcelSetting.Col_TestSteps_TestData, ExcelSetting.Sheet_TestSteps);
                        testStepDesc = ExcelManager.GetCellData(testStep, ExcelSetting.Col_TestSteps_Description, ExcelSetting.Sheet_TestSteps);

                        ExtentReporter.CreateNode(testStepDesc);

                        //call the method to execute the action keyword for the current test step
                        Execute_Actions();

                        //check The Fail results for the current test step and set the overall test case result as Fail if any test step has Fail result
                        if (outcome == (int)Outcome.Fail)
                        {
                            ExcelManager.SetCellData(Outcome.Fail.ToString(), iTestcase, ExcelSetting.Col_TestCases_Result, ExcelSetting.Sheet_TestCases);

                            Log.EndTestCase(testCaseID);
                            ExtentReporter.Error($"TestCase {testCaseID}_{testCaseTitle} {Outcome.Fail.ToString()}");
                            ExtentReporter.EndTestCase(testCaseID + "_" + testCaseTitle);
                            break;
                        }
                        //check The Error results for the current test step and set the overall test case result as Error if any test step has Error result
                        else if (outcome == (int)Outcome.Error)
                        {
                            ExcelManager.SetCellData(Outcome.Error.ToString(), iTestcase, ExcelSetting.Col_TestCases_Result, ExcelSetting.Sheet_TestCases);

                            Log.EndTestCase(testCaseID);
                            ExtentReporter.Error($"TestCase {testCaseID}_{testCaseTitle} {Outcome.Error.ToString()}");
                            ExtentReporter.EndTestCase(testCaseID + "_" + testCaseTitle);
                            break;
                        }
                    }

                    //check if all results is Pass for the current test case and set the overall test case result as Pass
                    if (outcome == (int)Outcome.Pass)
                    {
                        ExcelManager.SetCellData(Outcome.Pass.ToString(), iTestcase, ExcelSetting.Col_TestCases_Result, ExcelSetting.Sheet_TestCases);

                        Log.EndTestCase(testCaseID);
                        ExtentReporter.Pass($"TestCase {testCaseID}_{testCaseTitle} {Outcome.Pass.ToString()}");
                        ExtentReporter.EndTestCase(testCaseID + "_" + testCaseTitle);
                    }
                    //check if any results is Fail for the current test case and set the overall test case result as Fail
                    else if (outcome == (int)Outcome.Fail)
                    {
                        ExcelManager.SetCellData(Outcome.Fail.ToString(), iTestcase, ExcelSetting.Col_TestCases_Result, ExcelSetting.Sheet_TestCases);

                        Log.EndTestCase(testCaseID);
                        ExtentReporter.Fail($"TestCase {testCaseID}_{testCaseTitle} {Outcome.Fail.ToString()}");
                        ExtentReporter.EndTestCase(testCaseID + "_" + testCaseTitle);
                    }
                    //check if any results is Error for the current test case and set the overall test case result as Error
                    else if (outcome == (int)Outcome.Error)
                    {
                        ExcelManager.SetCellData(Outcome.Error.ToString(), iTestcase, ExcelSetting.Col_TestCases_Result, ExcelSetting.Sheet_TestCases);

                        Log.EndTestCase(testCaseID);
                        ExtentReporter.Error($"TestCase {testCaseID}_{testCaseTitle} {Outcome.Error.ToString()}");
                        ExtentReporter.EndTestCase(testCaseID + "_" + testCaseTitle);
                    }
                }
            }
        }

        // This method will execute the action keyword for the current test step and set the result in the TestSteps sheet based on the outcome of the action keyword execution
        static void Execute_Actions()
        {

            if (actionKeyword != null)
            {
                // the reflection class to invoke ActionKeyword methods based on actionKeyword variable value
                MethodInfo method = actionKeywords.GetType().GetMethod(actionKeyword);

                if (method != null)
                {
                    // Passing PageObject and data as Arguments to this class instance
                    string[] args = { pageObject, data };
                    method.Invoke(actionKeywords, args);
                }
                else
                {
                    Log.Error("Action Keyword: " + actionKeyword + " not found.");
                    outcome = (int)Outcome.Error;
                }

                if (outcome == (int)Outcome.Pass)
                {
                    ExcelManager.SetCellData(Outcome.Pass.ToString(), testStep, ExcelSetting.Col_TestSteps_Result, ExcelSetting.Sheet_TestSteps);
                    ExtentReporter.Pass(testStepDesc);
                }
                else if (outcome == (int)Outcome.Fail)
                {
                    ExcelManager.SetCellData(Outcome.Fail.ToString(), testStep, ExcelSetting.Col_TestSteps_Result, ExcelSetting.Sheet_TestSteps);
                    ExtentReporter.Fail(testStepDesc);

                    Keywords.CloseBrowser("", "");
                }
                else if (outcome == (int)Outcome.Error)
                {
                    ExcelManager.SetCellData(Outcome.Error.ToString(), testStep, ExcelSetting.Col_TestSteps_Result, ExcelSetting.Sheet_TestSteps);
                    ExtentReporter.Error(testStepDesc);

                    Keywords.CloseBrowser("", "");
                }
            }
        }
    }
}
