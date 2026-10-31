namespace KeywordDriven.Config
{
    internal class ExcelSetting
    {
        //Excel Workbook Default Worksheets
        internal static string Sheet_Settings { get; }= "Settings";
        internal static string Sheet_Locators { get; }= "Locators";
        internal static string Sheet_TestCases { get; }= "TestCases";
        internal static string Sheet_TestSteps { get; }= "TestSteps";

        //Locators Worksheet Default Columns
        internal static int Col_Locators_PageObject { get; }= 0;
        internal static int Col_Locators_Locator { get; }= 1;

        //TestCases Worksheet Default Columns
        internal static int Col_TestCases_ID { get; }= 0;
        internal static int Col_TestCases_Title { get; }= 1;
        internal static int Col_TestCases_Description { get; }= 2;
        internal static int Col_TestCases_RunMode { get; }= 3;
        internal static int Col_TestCases_Result { get; }= 4;

        //TestSteps Worksheet Default Columns
        internal static int Col_TestSteps_TestCaseID { get; }= 0;
        internal static int Col_TestSteps_StepNo { get; }= 1;
        internal static int Col_TestSteps_Description { get; }= 2;
        internal static int Col_TestSteps_PageObject { get; }= 3;
        internal static int Col_TestSteps_ActionKeyword { get; }= 4;
        internal static int Col_TestSteps_TestData { get; }= 5;
        internal static int Col_TestSteps_Result { get; }= 6;
    }
}
