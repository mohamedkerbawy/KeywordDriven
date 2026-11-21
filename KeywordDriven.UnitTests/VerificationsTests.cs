using KeywordDriven.ActionKeywords;
using KeywordDriven.Execution;
using Assert = NUnit.Framework.Assert;

namespace KeywordDriven.UnitTests;

public class VerificationsTests : ActionKeywordTestBase
{
    [Test]
    public void Run_FailedVerificationIsRecordedWithoutChangingOutcome()
    {
        DriverScript.outcome = (int)Outcome.Pass;

        Verifications.Run("VerifyExample", "expected value", () => Assertions.EvaluatePageMatch("actual", "expected", "exact"));

        Assert.Multiple(() =>
        {
            Assert.That(Verifications.CurrentStepFailed, Is.True);
            Assert.That(Verifications.HasFailures, Is.True);
            Assert.That(Verifications.FailureSummary, Is.EqualTo("VerifyExample: expected value"));
            Assert.That(DriverScript.outcome, Is.EqualTo((int)Outcome.Pass));
        });
    }

    [Test]
    public void Run_LogsExpectedAndActualValues()
    {
        TextWriter originalOutput = Console.Out;
        using var output = new StringWriter();
        try
        {
            Console.SetOut(output);
            Verifications.Run(
                "VerifyExample",
                "page title",
                () => Assertions.EvaluatePageMatch("Welcome", "Dashboard", "exact"));
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        Assert.That(output.ToString(), Does.Contain("Expected: exact \"Dashboard\""));
        Assert.That(output.ToString(), Does.Contain("Actual: \"Welcome\""));
    }

    [Test]
    public void Run_PassingVerificationClearsCurrentStepFailureButKeepsEarlierFailures()
    {
        Verifications.Run("VerifyFirst", "first", () => Assertions.EvaluatePageMatch("actual", "expected", "exact"));

        Verifications.Run("VerifySecond", "second", () => Assertions.EvaluatePageMatch("same", "same", "exact"));

        Assert.Multiple(() =>
        {
            Assert.That(Verifications.CurrentStepFailed, Is.False);
            Assert.That(Verifications.HasFailures, Is.True);
            Assert.That(Verifications.FailureSummary, Is.EqualTo("VerifyFirst: first"));
        });
    }

    [Test]
    public void BeginTestCase_ClearsFailuresFromPreviousCase()
    {
        Verifications.Run("VerifyExample", "details", () => Assertions.EvaluatePageMatch("actual", "expected", "exact"));

        Verifications.BeginTestCase();

        Assert.Multiple(() =>
        {
            Assert.That(Verifications.CurrentStepFailed, Is.False);
            Assert.That(Verifications.HasFailures, Is.False);
            Assert.That(Verifications.FailureSummary, Is.Empty);
        });
    }

    [Test]
    public void Run_ExceptionSetsErrorOutcome()
    {
        DriverScript.outcome = (int)Outcome.Pass;

        Verifications.Run(
            "VerifyExample",
            "details",
            () => throw new InvalidOperationException("verification error"));

        Assert.That(DriverScript.outcome, Is.EqualTo((int)Outcome.Error));
    }
}
