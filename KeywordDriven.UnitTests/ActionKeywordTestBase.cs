using KeywordDriven.ActionKeywords;
using KeywordDriven.Config;
using KeywordDriven.Execution;
using KeywordDriven.Utils;
using ClosedXML.Excel;

namespace KeywordDriven.UnitTests;

public abstract class ActionKeywordTestBase
{
    private int originalDefaultTimeoutSeconds;
    private int originalPageLoadTimeoutSeconds;

    protected string TestDirectory { get; private set; } = null!;

    [SetUp]
    public void SetUpActionKeywordTest()
    {
        originalDefaultTimeoutSeconds = DriverSetting.DefaultTimeoutSeconds;
        originalPageLoadTimeoutSeconds = DriverSetting.PageLoadTimeoutSeconds;
        DriverSetting.DefaultTimeoutSeconds = 1;
        DriverSetting.PageLoadTimeoutSeconds = 1;

        TestDirectory = Path.Combine(Path.GetTempPath(), $"KeywordDriven.UnitTests.{Guid.NewGuid():N}");
        Directory.CreateDirectory(TestDirectory);
        Log.SetLogger(Path.Combine(TestDirectory, "logs"));
        ExtentReporter.SetExtentReporter(Path.Combine(TestDirectory, "reports"));
        ExtentReporter.CreateTest("Unit test", "Action keyword unit test");
        ExtentReporter.CreateNode("Test step");

        Drivers.driver = null;
        DriverScript.outcome = (int)Outcome.Pass;
        Verifications.BeginTestCase();
    }

    [TearDown]
    public void TearDownActionKeywordTest()
    {
        ExcelManager.CloseExcel();
        Drivers.driver = null;
        DriverSetting.DefaultTimeoutSeconds = originalDefaultTimeoutSeconds;
        DriverSetting.PageLoadTimeoutSeconds = originalPageLoadTimeoutSeconds;
        ExtentReporter.Flush();

        if (Directory.Exists(TestDirectory))
            Directory.Delete(TestDirectory, recursive: true);
    }

    protected void CreateLocatorWorkbook(params (string PageObject, string Locator)[] locators)
    {
        string workbookDirectory = Path.Combine(TestDirectory, "workbook");
        Directory.CreateDirectory(workbookDirectory);
        string workbookPath = Path.Combine(workbookDirectory, "TestCases.xlsx");

        using (var workbook = new XLWorkbook())
        {
            IXLWorksheet worksheet = workbook.AddWorksheet("Locators");
            for (int index = 0; index < locators.Length; index++)
            {
                worksheet.Cell(index + 1, 1).Value = locators[index].PageObject;
                worksheet.Cell(index + 1, 2).Value = locators[index].Locator;
            }

            workbook.SaveAs(workbookPath);
        }

        ExcelManager.SetExcel(workbookDirectory, "TestCases");
    }
}
