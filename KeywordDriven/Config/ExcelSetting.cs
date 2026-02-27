namespace KeywordDriven.Config
{
    internal class ExcelSetting
    {
        //Excel Workbook Default Worksheets
        internal static string Sheet_Settings = "Settings";
        internal static string Sheet_Locators = "Locators";
        internal static string Sheet_TestCases = "TestCases";
        internal static string Sheet_TestSteps = "TestSteps";

        //Locators Worksheet Default Columns
        internal static int Col_Locators_PageObject = 0;
        internal static int Col_Locators_Locator = 1;

        //TestCases Worksheet Default Columns
        internal static int Col_TestCases_ID = 0;
        internal static int Col_TestCases_Title = 1;
        internal static int Col_TestCases_Description = 2;
        internal static int Col_TestCases_RunMode = 3;
        internal static int Col_TestCases_Result = 4;

        //TestSteps Worksheet Default Columns
        internal static int Col_TestSteps_TestCaseID = 0;
        internal static int Col_TestSteps_StepNo = 1;
        internal static int Col_TestSteps_Description = 2;
        internal static int Col_TestSteps_PageObject = 3;
        internal static int Col_TestSteps_ActionKeyword = 4;
        internal static int Col_TestSteps_TestData = 5;
        internal static int Col_TestSteps_Result = 6;
    }
}
