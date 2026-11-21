using ClosedXML.Excel;
using KeywordDriven.Execution;
using KeywordDriven.Utils;
using Assert = NUnit.Framework.Assert;

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
            testCases.Cell(3, 1).Value = "TS2";
            testCases.Cell(3, 2).Value = "No steps";
            testCases.Cell(3, 3).Value = "Passes without executing steps";
            testCases.Cell(3, 4).Value = "Yes";
            testCases.Cell(4, 1).Value = "TS3";
            testCases.Cell(4, 2).Value = "Disabled case";
            testCases.Cell(4, 3).Value = "Should be skipped";
            testCases.Cell(4, 4).Value = "No";

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
        TestCaseExecutionSummary summary;
        try
        {
            Console.SetOut(output);
            summary = DriverScript.Execute_TestCasesWithSummary();
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
        Assert.Multiple(() =>
        {
            Assert.That(summary.Total, Is.EqualTo(3));
            Assert.That(summary.Passed, Is.EqualTo(1));
            Assert.That(summary.Failed, Is.Zero);
            Assert.That(summary.Errors, Is.EqualTo(1));
            Assert.That(summary.Skipped, Is.EqualTo(1));
            Assert.That(summary.AllPassed, Is.False);
            Assert.That(summary.ToString(), Does.Contain("All: 3 | Passed: 1 | Failed: 0 | Error: 1 | Skipped: 1"));
        });
    }
}
