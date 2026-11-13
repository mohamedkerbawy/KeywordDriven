using KeywordDriven.ActionKeywords;
using KeywordDriven.Execution;

namespace KeywordDriven.UnitTests;

public class VerificationsTests : ActionKeywordTestBase
{
    [Test]
    public void Run_FailedVerificationIsRecordedWithoutChangingOutcome()
    {
        DriverScript.outcome = (int)Outcome.Pass;

        Verifications.Run("VerifyExample", "expected value", () => false);

        Assert.Multiple(() =>
        {
            Assert.That(Verifications.CurrentStepFailed, Is.True);
            Assert.That(Verifications.HasFailures, Is.True);
            Assert.That(Verifications.FailureSummary, Is.EqualTo("VerifyExample: expected value"));
            Assert.That(DriverScript.outcome, Is.EqualTo((int)Outcome.Pass));
        });
    }

    [Test]
    public void Run_PassingVerificationClearsCurrentStepFailureButKeepsEarlierFailures()
    {
        Verifications.Run("VerifyFirst", "first", () => false);

        Verifications.Run("VerifySecond", "second", () => true);

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
        Verifications.Run("VerifyExample", "details", () => false);

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
