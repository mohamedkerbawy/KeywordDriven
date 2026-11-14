using ClosedXML.Excel;
using KeywordDriven.Execution;
using KeywordDriven.Utils;

namespace KeywordDriven.UnitTests;

public class DriverScriptTests : ActionKeywordTestBase
{
    [Test]
    public void ExecuteTestCases_LogsCaseEndOnlyOnceAfterStepError()
    {
        string workbookDirectory = Path.Combine(TestDirectory, "execution-workbook");
        Directory.CreateDirectory(workbookDirectory);
        string workbookPath = Path.Combine(workbookDirectory, "TestCases.xlsx");

        using (var workbook = new XLWorkbook())
        {
            IXLWorksheet testCases = workbook.AddWorksheet("TestCases");
            testCases.Cell(1, 1).Value = "TestCaseID";
            testCases.Cell(1, 2).Value = "Title";
            testCases.Cell(1, 3).Value = "Description";
            testCases.Cell(1, 4).Value = "RunMode";
            testCases.Cell(2, 1).Value = "TS1";
            testCases.Cell(2, 2).Value = "Unknown action";
            testCases.Cell(2, 3).Value = "Triggers keyword dispatch error";
            testCases.Cell(2, 4).Value = "Yes";

            IXLWorksheet testSteps = workbook.AddWorksheet("TestSteps");
            testSteps.Cell(1, 1).Value = "TestCaseID";
            testSteps.Cell(1, 2).Value = "StepNo";
            testSteps.Cell(1, 3).Value = "Description";
            testSteps.Cell(1, 4).Value = "ActionKeyword";
            testSteps.Cell(2, 1).Value = "TS1";
            testSteps.Cell(2, 2).Value = 1;
            testSteps.Cell(2, 3).Value = "Invoke unknown action";
            testSteps.Cell(2, 4).Value = "UnknownAction";

            workbook.SaveAs(workbookPath);
        }

        ExcelManager.SetExcel(workbookDirectory, "TestCases");

        TextWriter originalOutput = Console.Out;
        using var output = new StringWriter();
        bool allTestCasesPassed;
        try
        {
            Console.SetOut(output);
            allTestCasesPassed = DriverScript.Execute_TestCases();
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        string[] caseEndMessages = output.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.Contains("End TestCase TS1 | Result:", StringComparison.Ordinal))
            .ToArray();

        Assert.That(caseEndMessages, Has.Length.EqualTo(1));
        Assert.That(caseEndMessages[0], Does.Contain("Result: ERROR"));
        Assert.That(allTestCasesPassed, Is.False);
    }
}
