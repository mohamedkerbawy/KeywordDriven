using System.Collections.ObjectModel;
using KeywordDriven.ActionKeywords;
using Moq;
using OpenQA.Selenium;
using Assert = NUnit.Framework.Assert;

namespace KeywordDriven.UnitTests;

public class WaitsTests : ActionKeywordTestBase
{
    [TestCase("https://example.com/login", "login", true)]
    public void WaitUntilUrlContains_ReturnsWhetherUrlContainsExpectedText(
        string currentUrl,
        string expectedText,
        bool expected)
    {
        var driver = new Mock<IWebDriver>();
        driver.SetupGet(item => item.Url).Returns(currentUrl);

        bool result = Waits.WaitUntilUrlContains(driver.Object, expectedText, timeoutSeconds: 1);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void WaitUntilUrlContainsIgnoreCase_MatchesWithoutCaseSensitivity()
    {
        var driver = new Mock<IWebDriver>();
        driver.SetupGet(item => item.Url).Returns("https://example.com/LOGIN");

        Assert.That(
            Waits.WaitUntilUrlContainsIgnoreCase(driver.Object, "login", timeoutSeconds: 1),
            Is.True);
    }

    [Test]
    public void WaitUntilUrlContains_ThrowsWhenConditionDoesNotBecomeTrue()
    {
        var driver = new Mock<IWebDriver>();
        driver.SetupGet(item => item.Url).Returns("https://example.com/home");

        Assert.That(
            () => Waits.WaitUntilUrlContains(driver.Object, "login", timeoutSeconds: 1),
            Throws.TypeOf<WebDriverTimeoutException>());
    }

    [Test]
    public void WaitUntilUrlEquals_RequiresExactUrl()
    {
        var driver = new Mock<IWebDriver>();
        driver.SetupGet(item => item.Url).Returns("https://example.com/path");

        Assert.That(
            Waits.WaitUntilUrlEquals(driver.Object, "https://example.com/path", timeoutSeconds: 1),
            Is.True);
    }

    [Test]
    public void WaitUntilUrlStartsAndEndsWith_IgnoresCase()
    {
        var driver = new Mock<IWebDriver>();
        driver.SetupGet(item => item.Url).Returns("https://example.com/LOGIN");

        Assert.Multiple(() =>
        {
            Assert.That(Waits.WaitUntilUrlStartsWith(driver.Object, "HTTPS://", 1), Is.True);
            Assert.That(Waits.WaitUntilUrlEndsWith(driver.Object, "/login", 1), Is.True);
        });
    }

    [Test]
    public void WaitUntilUrlMatchesPattern_MatchesUrl()
    {
        var driver = new Mock<IWebDriver>();
        driver.SetupGet(item => item.Url).Returns("https://example.com/users/123");

        Assert.That(
            Waits.WaitUntilUrlMatchesPattern(driver.Object, @"/users/\d+$", timeoutSeconds: 1),
            Is.True);
    }

    [Test]
    public void WaitUntilUrlMatchesPattern_RejectsInvalidRegularExpression()
    {
        var driver = new Mock<IWebDriver>();

        Assert.That(
            () => Waits.WaitUntilUrlMatchesPattern(driver.Object, "[", timeoutSeconds: 1),
            Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public void WaitUntilPageLoaded_ReturnsTrueWhenDocumentIsComplete()
    {
        var driver = new Mock<IWebDriver>();
        var javascript = driver.As<IJavaScriptExecutor>();
        javascript.Setup(executor => executor.ExecuteScript("return document.readyState;"))
            .Returns("complete");

        Assert.That(Waits.WaitUntilPageLoaded(driver.Object, timeoutSeconds: 1), Is.True);
    }

    [Test]
    public void WaitUntilTextContains_WaitsForVisibleElementText()
    {
        var element = new Mock<IWebElement>();
        element.SetupGet(item => item.Displayed).Returns(true);
        element.SetupGet(item => item.Text).Returns("Welcome back");

        var driver = new Mock<IWebDriver>();
        driver.Setup(item => item.FindElement(It.IsAny<By>())).Returns(element.Object);

        Assert.That(
            Waits.WaitUntilTextContains(driver.Object, By.Id("message"), "Welcome", timeoutSeconds: 1),
            Is.True);
    }

    [Test]
    public void WaitUntilExists_ReturnsWhenMatchingElementIsFound()
    {
        var driver = new Mock<IWebDriver>();
        driver.Setup(item => item.FindElements(It.IsAny<By>()))
            .Returns(new ReadOnlyCollection<IWebElement>([Mock.Of<IWebElement>()]));

        Assert.DoesNotThrow(
            () => Waits.WaitUntilExists(driver.Object, By.Id("ready"), timeoutSeconds: 1));
    }

    [Test]
    public void WaitUntilClickable_ReturnsWhenElementIsVisibleAndEnabled()
    {
        var element = new Mock<IWebElement>();
        element.SetupGet(item => item.Displayed).Returns(true);
        element.SetupGet(item => item.Enabled).Returns(true);
        var driver = new Mock<IWebDriver>();
        driver.Setup(item => item.FindElement(It.IsAny<By>())).Returns(element.Object);

        Assert.DoesNotThrow(
            () => Waits.WaitUntilClickable(driver.Object, By.Id("submit"), timeoutSeconds: 1));
    }

    [Test]
    public void WaitUntilEditable_ReturnsWhenElementCanReceiveInput()
    {
        var element = new Mock<IWebElement>();
        element.SetupGet(item => item.Displayed).Returns(true);
        element.SetupGet(item => item.Enabled).Returns(true);
        element.Setup(item => item.GetAttribute("readonly")).Returns((string)null!);
        var driver = new Mock<IWebDriver>();
        driver.Setup(item => item.FindElement(It.IsAny<By>())).Returns(element.Object);

        Assert.DoesNotThrow(
            () => Waits.WaitUntilEditable(driver.Object, By.Id("email"), timeoutSeconds: 1));
    }

    [Test]
    public void WaitUntilAllVisible_ReturnsWhenEveryMatchIsVisible()
    {
        var visibleElement = new Mock<IWebElement>();
        visibleElement.SetupGet(item => item.Displayed).Returns(true);
        var driver = new Mock<IWebDriver>();
        driver.Setup(item => item.FindElements(It.IsAny<By>()))
            .Returns(new ReadOnlyCollection<IWebElement>([visibleElement.Object]));

        Assert.DoesNotThrow(
            () => Waits.WaitUntilAllVisible(driver.Object, By.ClassName("result"), timeoutSeconds: 1));
    }

    [Test]
    public void WaitUntilNotExists_ReturnsWhenNoMatchingElementsExist()
    {
        var driver = new Mock<IWebDriver>();
        driver.Setup(item => item.FindElements(It.IsAny<By>()))
            .Returns(new ReadOnlyCollection<IWebElement>([]));
        driver.As<IJavaScriptExecutor>()
            .Setup(executor => executor.ExecuteScript(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(0);

        Assert.DoesNotThrow(
            () => Waits.WaitUntilNotExists(driver.Object, By.Id("loading"), timeoutSeconds: 1));
    }
}
