using KeywordDriven.ActionKeywords;
using KeywordDriven.Config;
using KeywordDriven.Execution;
using Moq;
using OpenQA.Selenium;
using Assert = NUnit.Framework.Assert;

namespace KeywordDriven.UnitTests;

public class DriversTests : ActionKeywordTestBase
{
    [Test]
    public void OpenBrowser_RejectsEmptyBrowserName()
    {
        Assert.That(
            () => Drivers.OpenBrowser(" "),
            Throws.ArgumentException.With.Property("ParamName").EqualTo("browserType"));
    }

    [Test]
    public void OpenBrowser_RejectsUnsupportedBrowserWithoutStartingSession()
    {
        Assert.That(
            () => Drivers.OpenBrowser("safari"),
            Throws.ArgumentException.With.Message.Contains("Unsupported browser"));
        Assert.That(Drivers.driver, Is.Null);
    }

    [Test]
    public void OpenBrowser_RejectsInvalidTimeoutSettings()
    {
        DriverSetting.DefaultTimeoutSeconds = 0;

        Assert.That(
            () => Drivers.OpenBrowser("chrome"),
            Throws.InvalidOperationException.With.Message.Contains("timeout settings"));
    }

    [Test]
    public void OpenBrowser_RejectsSecondBrowserSession()
    {
        Drivers.driver = Mock.Of<IWebDriver>();

        Assert.That(
            () => Drivers.OpenBrowser("chrome"),
            Throws.InvalidOperationException.With.Message.Contains("already open"));
    }

    [Test]
    public void NavigateToUrl_RequiresAnOpenBrowser()
    {
        Assert.That(
            () => Drivers.NavigateToUrl("https://example.com"),
            Throws.InvalidOperationException.With.Message.Contains("Browser is not open"));
    }

    [TestCase("")]
    [TestCase("javascript:alert(1)")]
    [TestCase("https://")]
    public void NavigateToUrl_RejectsInvalidHttpAddress(string url)
    {
        Drivers.driver = Mock.Of<IWebDriver>();

        Assert.That(() => Drivers.NavigateToUrl(url), Throws.ArgumentException);
    }

    [Test]
    public void NavigateToUrl_AddsHttpsSchemeAndWaitsForDocumentLoad()
    {
        var navigation = new Mock<INavigation>();
        var webDriver = new Mock<IWebDriver>();
        webDriver.Setup(driver => driver.Navigate()).Returns(navigation.Object);
        webDriver.SetupGet(driver => driver.Title).Returns("Example");
        webDriver.SetupGet(driver => driver.Url).Returns("https://example.com/");
        webDriver.As<IJavaScriptExecutor>()
            .Setup(executor => executor.ExecuteScript("return document.readyState"))
            .Returns("complete");
        Drivers.driver = webDriver.Object;

        Drivers.NavigateToUrl(" example.com ");

        navigation.Verify(
            item => item.GoToUrl(It.Is<Uri>(uri => uri.AbsoluteUri == "https://example.com/")),
            Times.Once);
    }

    [Test]
    public void SetWindowSize_RejectsNonPositiveDimensions()
    {
        Drivers.driver = Mock.Of<IWebDriver>();

        Assert.That(
            () => Drivers.SetWindowSize(1280, 0),
            Throws.ArgumentException.With.Message.Contains("must be greater than 0"));
    }

    [Test]
    public void ExecuteDriverKeyword_LogsInfoThenPassAndOmitsEmptyDetails()
    {
        TextWriter originalOutput = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);
        try
        {
            Drivers.ExecuteDriverKeyword(
                "RefreshPage",
                (string.Empty, " "),
                () => { });
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        string[] logLines = output.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(logLines, Has.Length.EqualTo(2));
        Assert.That(logLines[0], Does.Contain("[INFO   ] RefreshPage"));
        Assert.That(logLines[1], Does.Contain("[PASS   ] RefreshPage"));
        Assert.That(logLines.Any(line => line.Contains(" | ", StringComparison.Ordinal)), Is.False);
        Assert.That(DriverScript.outcome, Is.EqualTo((int)Outcome.Pass));
    }

    [Test]
    public void ExecuteDriverKeyword_LogsErrorWithDetailsAndException()
    {
        TextWriter originalOutput = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);
        try
        {
            Drivers.ExecuteDriverKeyword(
                "SwitchToWindow",
                ("popup", string.Empty),
                () => throw new InvalidOperationException("window not found"));
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        string[] logLines = output.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(logLines, Has.Length.EqualTo(2));
        Assert.That(logLines[0], Does.Contain("[INFO   ] SwitchToWindow | popup"));
        Assert.That(logLines[1], Does.Contain("[ERROR  ] SwitchToWindow | popup | window not found"));
        Assert.That(DriverScript.outcome, Is.EqualTo((int)Outcome.Error));
    }

    [Test]
    public void ExecuteDriverKeyword_PreservesExistingFailureWhenCleanupSucceeds()
    {
        DriverScript.outcome = (int)Outcome.Fail;
        TextWriter originalOutput = Console.Out;
        using var output = new StringWriter();

        try
        {
            Console.SetOut(output);
            Drivers.ExecuteDriverKeyword(
                "CloseBrowser",
                (string.Empty, string.Empty),
                () => { },
                preserveExistingOutcome: true);
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        string[] logLines = output.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(logLines, Has.Length.EqualTo(1));
        Assert.That(logLines[0], Does.Contain("[INFO   ] CloseBrowser"));
        Assert.That(DriverScript.outcome, Is.EqualTo((int)Outcome.Fail));
    }

    [Test]
    public void ExecuteDriverKeyword_PreservesExistingFailureWhenCleanupFails()
    {
        DriverScript.outcome = (int)Outcome.Fail;

        Drivers.ExecuteDriverKeyword(
            "CloseBrowser",
            (string.Empty, string.Empty),
            () => throw new InvalidOperationException("cleanup failed"),
            preserveExistingOutcome: true);

        Assert.That(DriverScript.outcome, Is.EqualTo((int)Outcome.Fail));
    }
}
