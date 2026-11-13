using KeywordDriven.ActionKeywords;
using KeywordDriven.Config;
using Moq;
using OpenQA.Selenium;

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
}
