using System.Collections.ObjectModel;
using KeywordDriven.ActionKeywords;
using KeywordDriven.Execution;
using Moq;
using OpenQA.Selenium;
using Assert = NUnit.Framework.Assert;

namespace KeywordDriven.UnitTests;

public class AssertionsTests : ActionKeywordTestBase
{
    [TestCase("exact", "Welcome", "Welcome", true)]
    [TestCase("exact", "Welcome", "welcome", false)]
    [TestCase("contains", "Welcome back", "back", true)]
    [TestCase("startswith", "Welcome back", "Welcome", true)]
    [TestCase("endswith", "Welcome back", "back", true)]
    [TestCase("notequal", "Welcome", "Goodbye", true)]
    [TestCase("notcontains", "Welcome", "bye", true)]
    [TestCase("regex", "Order-123", @"Order-\d+", true)]
    public void AssertPageMatch_EvaluatesSupportedMatchTypes(
        string matchType,
        string actual,
        string expected,
        bool result)
    {
        Assert.That(Assertions.AssertPageMatch(actual, expected, matchType), Is.EqualTo(result));
    }

    [Test]
    public void AssertPageMatch_SupportsIgnoreCase()
    {
        Assert.That(Assertions.AssertPageMatch("WELCOME", "welcome", "exact", ignoreCase: true), Is.True);
    }

    [Test]
    public void AssertPageMatch_RejectsUnknownMatchType()
    {
        Assert.That(
            () => Assertions.AssertPageMatch("text", "text", "approximate"),
            Throws.ArgumentException.With.Message.Contains("Unsupported matchType"));
    }

    [TestCase(null, null, true)]
    [TestCase("readonly", "", false)]
    [TestCase("aria-disabled", "true", false)]
    [TestCase("aria-readonly", "true", false)]
    public void IsEditable_RequiresVisibleEnabledAndWritableElement(
        string? attributeName,
        string? attributeValue,
        bool expected)
    {
        var element = new Mock<IWebElement>();
        element.SetupGet(item => item.TagName).Returns("input");
        element.SetupGet(item => item.Displayed).Returns(true);
        element.SetupGet(item => item.Enabled).Returns(true);
        element.Setup(item => item.GetAttribute(It.IsAny<string>()))
            .Returns((string name) => name == attributeName ? attributeValue! : null!);

        Assert.That(Assertions.IsEditable(element.Object), Is.EqualTo(expected));
    }

    [TestCase("input", "checkbox", null, true, true, true)]
    [TestCase("input", "radio", null, false, false, false)]
    [TestCase("div", null, "checkbox", true, true, true)]
    [TestCase("div", null, "checkbox", false, false, true)]
    [TestCase("div", null, "checkbox", null, false, false)]
    public void HasExpectedSelection_HandlesNativeAndAriaControls(
        string tagName,
        string? type,
        string? role,
        bool? selected,
        bool expectedSelection,
        bool expected)
    {
        var element = new Mock<IWebElement>();
        element.SetupGet(item => item.TagName).Returns(tagName);
        element.SetupGet(item => item.Selected).Returns(selected == true);
        element.Setup(item => item.GetAttribute("type")).Returns(type!);
        element.Setup(item => item.GetAttribute("role")).Returns(role!);
        element.Setup(item => item.GetAttribute("aria-checked"))
            .Returns(selected.HasValue ? selected.Value ? "true" : "false" : null!);

        Assert.That(
            Assertions.HasExpectedSelection(element.Object, "checkbox", expectedSelection),
            Is.EqualTo(expected));
    }

    [Test]
    public void AssertElementMatch_UsesInputValueAsText()
    {
        CreateLocatorWorkbook(("user_id", "user"));
        var input = new Mock<IWebElement>();
        input.SetupGet(element => element.TagName).Returns("input");
        input.Setup(element => element.GetAttribute("value")).Returns("alice@example.com");
        SetElements(input.Object);

        Assert.That(Assertions.AssertElementMatch("user_id", "alice", "contains"), Is.True);
    }

    [Test]
    public void AssertAttribute_ValidatesNameAndOptionalExpectedValue()
    {
        CreateLocatorWorkbook(("button_id", "submit"));
        var button = new Mock<IWebElement>();
        button.Setup(element => element.GetDomAttribute("type")).Returns("submit");
        SetElements(button.Object);

        Assert.Multiple(() =>
        {
            Assert.That(Assertions.AssertAttribute("button_id", "type=submit", shouldExist: true), Is.True);
            Assert.That(Assertions.AssertAttribute("button_id", "type=button", shouldExist: true), Is.False);
            Assert.That(Assertions.AssertAttribute("button_id", "type=button", shouldExist: false), Is.True);
        });
    }

    [Test]
    public void AssertAttribute_RejectsMissingAttributeName()
    {
        Assert.That(
            () => Assertions.AssertAttribute("button_id", "=submit", shouldExist: true),
            Throws.ArgumentException.With.Message.Contains("must include an attribute name"));
    }

    [Test]
    public void AssertSelectedOption_MatchesSelectedOptionTextValueAndIndex()
    {
        CreateLocatorWorkbook(("country_id", "country"));
        var firstOption = CreateOption("Canada", "ca", selected: false);
        var selectedOption = CreateOption("United States", "us", selected: true);
        var select = new Mock<IWebElement>();
        select.SetupGet(element => element.TagName).Returns("select");
        select.Setup(element => element.FindElements(It.Is<By>(by => by.ToString() == "By.TagName: option")))
            .Returns(new ReadOnlyCollection<IWebElement>([firstOption.Object, selectedOption.Object]));
        SetElements(select.Object);

        Assert.Multiple(() =>
        {
            Assert.That(Assertions.AssertSelectedOption("country_id", "United States", "text"), Is.True);
            Assert.That(Assertions.AssertSelectedOption("country_id", "us", "value"), Is.True);
            Assert.That(Assertions.AssertSelectedOption("country_id", "1", "index"), Is.True);
        });
    }

    [Test]
    public void AssertUrlMatch_UsesCurrentBrowserUrl()
    {
        var driver = new Mock<IWebDriver>();
        driver.SetupGet(item => item.Url).Returns("https://example.com/orders/42");
        Drivers.driver = driver.Object;

        Assert.That(Assertions.AssertUrlMatch(@"/orders/\d+$", "regex"), Is.True);
    }

    [Test]
    public void Run_UpdatesOutcomeForPassAndFailure()
    {
        DriverScript.outcome = (int)Outcome.Error;
        Assertions.Run("AssertExample", "details", () => Assertions.EvaluatePageMatch("same", "same", "exact"));
        Assert.That(DriverScript.outcome, Is.EqualTo((int)Outcome.Pass));

        Assertions.Run("AssertExample", "details", () => Assertions.EvaluatePageMatch("actual", "expected", "exact"));
        Assert.That(DriverScript.outcome, Is.EqualTo((int)Outcome.Fail));
    }

    [Test]
    public void Run_LogsExpectedAndActualValues()
    {
        TextWriter originalOutput = Console.Out;
        using var output = new StringWriter();
        try
        {
            Console.SetOut(output);
            Assertions.Run(
                "AssertExample",
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

    private static void SetElements(params IWebElement[] elements)
    {
        var driver = new Mock<IWebDriver>();
        driver.Setup(item => item.FindElements(It.IsAny<By>()))
            .Returns(new ReadOnlyCollection<IWebElement>(elements));
        Drivers.driver = driver.Object;
    }

    private static Mock<IWebElement> CreateOption(string text, string value, bool selected)
    {
        var option = new Mock<IWebElement>();
        option.SetupGet(element => element.Text).Returns(text);
        option.SetupGet(element => element.Selected).Returns(selected);
        option.Setup(element => element.GetAttribute("value")).Returns(value);
        return option;
    }
}
