namespace KeywordDriven.Config
{
    public class ExcelSetting
    {
        //Excel sheets Default Data
        internal static string Sheet_Locators = "Locators";
        internal static string Sheet_TestCases = "TestCases";
        internal static string Sheet_TestSteps = "TestSteps";

        //Locators Sheet Default Columns Indexes
        internal static int Col_Locators_PageObject = 0;
        internal static int Col_Locators_Locator = 1;

        //TestCases Sheet Default Columns Indexes
        internal static int Col_TestCases_ID = 0;
        internal static int Col_TestCases_Title = 1;
        internal static int Col_TestCases_Description = 2;
        internal static int Col_TestCases_RunMode = 3;
        internal static int Col_TestCases_Result = 4;

        //TestSteps Sheet Default Columns Indexes
        internal static int Col_TestSteps_TestCaseID = 0;
        internal static int Col_TestSteps_StepNo = 1;
        internal static int Col_TestSteps_Description = 2;
        internal static int Col_TestSteps_PageObject = 4;
        internal static int Col_TestSteps_ActionKeyword = 5;
        internal static int Col_TestSteps_TestData = 6;
        internal static int Col_TestSteps_Result = 7;

        public static void Locators_Columns_Index(int PageObject, int Locator)
        {
            Col_Locators_PageObject = PageObject;
            Col_Locators_Locator = Locator;
        }

        public static void TestCases_Columns_Index(int ID, int Title,int Description,int RunMode,int Result)
        {
            Col_TestCases_ID = ID;
            Col_TestCases_Title = Title;
            Col_TestCases_Description = Description;
            Col_TestCases_RunMode = RunMode;
            Col_TestCases_Result = Result;
        }

        public static void TestSteps_Columns_Index(int TestCaseID, int StepNo, int Description,int PageObject, int ActionKeyword, int TestData, int Result)
        {
            Col_TestSteps_TestCaseID = TestCaseID;
            Col_TestSteps_StepNo = StepNo;
            Col_TestSteps_Description = Description;
            Col_TestSteps_PageObject = PageObject;
            Col_TestSteps_ActionKeyword = ActionKeyword;
            Col_TestSteps_TestData = TestData;
            Col_TestSteps_Result = Result;
        }
    }
}
