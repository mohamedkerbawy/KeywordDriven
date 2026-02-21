using System;
using System.Reflection;
using KeywordDriven.Config;
using KeywordDriven.Utils;
using KeywordDriven.ActionKeywords;

namespace KeywordDriven.Execution
{
    public class DriverScript
    {
        static Keywords actionKeywords;
        static String sActionKeyword;
        static String sPageObject;
        static MethodInfo[] method;
        static int iTestStep;
        static int iTestLastStep;
        static String sTestCaseID;
        static String sTestCaseTitle;
        static String sTestCaseDesc;
        static String sTestStepDesc;
        static String sRunMode;
        static String sData;
        internal static int iOutcome; // 1-Pass 2-Fail 3-Error
        internal static bool bOutcomeFail;
        internal static bool bOutcomeError;

        static DriverScript()
        {
            actionKeywords = new Keywords();
            method = actionKeywords.GetType().GetMethods();
        }

        public static void Execute_TestCases()
        {
            int iTotalTestCases = ExcelManager.GetRowCount(ExcelSetting.Sheet_TestCases);
            for (int iTestcase = 1; iTestcase < iTotalTestCases; iTestcase++)
            {
                iOutcome = 1;
                bOutcomeFail = false;

                sTestCaseID = ExcelManager.GetCellData(iTestcase, ExcelSetting.Col_TestCases_ID, ExcelSetting.Sheet_TestCases);
                sTestCaseTitle = ExcelManager.GetCellData(iTestcase, ExcelSetting.Col_TestCases_Title, ExcelSetting.Sheet_TestCases);
                sTestCaseDesc = ExcelManager.GetCellData(iTestcase, ExcelSetting.Col_TestCases_Description, ExcelSetting.Sheet_TestCases);
                sRunMode = ExcelManager.GetCellData(iTestcase, ExcelSetting.Col_TestCases_RunMode, ExcelSetting.Sheet_TestCases);

                if (sRunMode.Equals("Yes"))
                {
                    Log.StartTestCase(sTestCaseID);
                    ExtentReporter.CreateTest(sTestCaseID + "_" + sTestCaseTitle, sTestCaseDesc);
                    ExtentReporter.StartTestCase(sTestCaseID + "_" + sTestCaseTitle);
                    iTestStep = ExcelManager.GetRowContains(sTestCaseID, ExcelSetting.Col_TestSteps_TestCaseID, ExcelSetting.Sheet_TestSteps);
                    iTestLastStep = ExcelManager.GetTestStepsCount(ExcelSetting.Sheet_TestSteps, sTestCaseID, iTestStep);
                    iOutcome = 1;
                    bOutcomeFail = false;

                    for (; iTestStep < iTestLastStep; iTestStep++)
                    {
                        sActionKeyword = ExcelManager.GetCellData(iTestStep, ExcelSetting.Col_TestSteps_ActionKeyword, ExcelSetting.Sheet_TestSteps);
                        sPageObject = ExcelManager.GetCellData(iTestStep, ExcelSetting.Col_TestSteps_PageObject, ExcelSetting.Sheet_TestSteps);
                        sData = ExcelManager.GetCellData(iTestStep, ExcelSetting.Col_TestSteps_TestData, ExcelSetting.Sheet_TestSteps);
                        sTestStepDesc = ExcelManager.GetCellData(iTestStep, ExcelSetting.Col_TestSteps_Description, ExcelSetting.Sheet_TestSteps);
                        ExtentReporter.CreateNode(sTestStepDesc);
                        Execute_Actions();

                        if (iOutcome == 3)
                        {
                            ExcelManager.SetCellData("Error", iTestcase, ExcelSetting.Col_TestCases_Result, ExcelSetting.Sheet_TestCases);
                            Log.EndTestCase(sTestCaseID);
                            ExtentReporter.Error("TestCase " + sTestCaseID + "_" + sTestCaseTitle + " Error");
                            ExtentReporter.EndTestCase(sTestCaseID + "_" + sTestCaseTitle);
                            break;
                        }
                    }

                    if (iOutcome == 1 && bOutcomeFail==false)
                    {
                        ExcelManager.SetCellData("Pass", iTestcase, ExcelSetting.Col_TestCases_Result, ExcelSetting.Sheet_TestCases);
                        Log.EndTestCase(sTestCaseID);
                        ExtentReporter.Pass("TestCase " + sTestCaseID + "_" + sTestCaseTitle + " Passed");
                        ExtentReporter.EndTestCase(sTestCaseID + "_" + sTestCaseTitle);
                    }
                    else if (iOutcome == 1 && bOutcomeFail == true)
                    {
                        ExcelManager.SetCellData("Fail", iTestcase, ExcelSetting.Col_TestCases_Result, ExcelSetting.Sheet_TestCases);
                        Log.EndTestCase(sTestCaseID);
                        ExtentReporter.Fail("TestCase " + sTestCaseID + "_" + sTestCaseTitle + " Failed");
                        ExtentReporter.EndTestCase(sTestCaseID + "_" + sTestCaseTitle);
                    }
                    else if (iOutcome == 2)
                    {
                        ExcelManager.SetCellData("Fail", iTestcase, ExcelSetting.Col_TestCases_Result, ExcelSetting.Sheet_TestCases);
                        Log.EndTestCase(sTestCaseID);
                        ExtentReporter.Fail("TestCase " + sTestCaseID + "_" + sTestCaseTitle + " Failed");
                        ExtentReporter.EndTestCase(sTestCaseID + "_" + sTestCaseTitle);
                    }
                }                
            }
        }

        static void Execute_Actions()
        {
            for (int i = 0; i < method.Length; i++)
            {

                if (method[i].Name.Equals(sActionKeyword))
                {
                    method[i].Invoke(actionKeywords, new object[] { sPageObject, sData });

                    if (iOutcome == 1)
                    {
                        ExcelManager.SetCellData("Pass", iTestStep, ExcelSetting.Col_TestSteps_Result, ExcelSetting.Sheet_TestSteps);
                        ExtentReporter.Pass(sTestStepDesc);
                        break;
                    }
                    else if (iOutcome == 2)
                    {
                        ExcelManager.SetCellData("Fail", iTestStep, ExcelSetting.Col_TestSteps_Result, ExcelSetting.Sheet_TestSteps);
                        ExtentReporter.Fail(sTestStepDesc);
                        break;
                    }
                    else if (iOutcome == 3)
                    {
                        ExcelManager.SetCellData("Error", iTestStep, ExcelSetting.Col_TestSteps_Result, ExcelSetting.Sheet_TestSteps);
                        ExtentReporter.Error(sTestStepDesc);
                        Keywords.CloseBrowser("", "");
                        break;
                    }
                }
            }
        }
    }
}
