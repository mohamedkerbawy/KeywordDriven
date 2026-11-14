using System.Collections.ObjectModel;
using KeywordDriven.ActionKeywords;
using KeywordDriven.Config;
using KeywordDriven.Execution;
using Moq;
using OpenQA.Selenium;

namespace KeywordDriven.UnitTests;

public class LocatorsTests : ActionKeywordTestBase
{
    [TestCase("email_id", "login-email", "By.Id: login-email")]
    [TestCase("email_css", "input.email", "By.CssSelector: input.email")]
    [TestCase("email_xpath", "//input[@name='email']", "By.XPath: //input[@name='email']")]
    [TestCase("email_name", "email", "By.Name: email")]
    [TestCase("email_class", "email-field", "By.ClassName[Contains]: email-field")]
    [TestCase("email_tag", "input", "By.TagName: input")]
    [TestCase("email_linktext", "Sign in", "By.LinkText: Sign in")]
    [TestCase("email_partiallinktext", "Sign", "By.PartialLinkText: Sign")]
    [TestCase("email_testid", "login-form", "By.XPath: //*[@data-testid='login-form']")]
    [TestCase("email_arialabel", "Email address", "By.XPath: //*[@aria-label='Email address']")]
    [TestCase("email_role", "textbox", "By.XPath: //*[@role='textbox']")]
    [TestCase("email_text", "Continue", "By.XPath: //*[normalize-space(.)='Continue']")]
    [TestCase("email_partialtext", "Cont", "By.XPath: //*[contains(normalize-space(.), 'Cont')]")]
    public void GetLocator_MapsSupportedLocatorType(string pageObject, string value, string expected)
    {
        CreateLocatorWorkbook((pageObject, value));

        Assert.That(Locators.GetLocator(pageObject).ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void GetLocator_RejectsPageObjectWithoutLocatorType()
    {
        Assert.That(
            () => Locators.GetLocator("email"),
            Throws.ArgumentException.With.Message.Contains("must include a locator type"));
    }

    [Test]
    public void GetLocator_RejectsUnsupportedLocatorType()
    {
        CreateLocatorWorkbook(("email_unknown", "value"));

        Assert.That(
            () => Locators.GetLocator("email_unknown"),
            Throws.ArgumentException.With.Message.Contains("Unsupported locator type"));
    }

    [Test]
    public void GetLocator_RejectsPageObjectWithoutConfiguredValue()
    {
        CreateLocatorWorkbook();

        Assert.That(
            () => Locators.GetLocator("email_id"),
            Throws.InvalidOperationException.With.Message.Contains("No locator value"));
    }

    [Test]
    public void GetLocator_EscapesBothQuoteTypesInTextLocator()
    {
        const string pageObject = "message_text";
        const string value = "It's a \"test\"";
        CreateLocatorWorkbook((pageObject, value));

        Assert.That(
            Locators.GetLocator(pageObject).ToString(),
            Is.EqualTo("By.XPath: //*[normalize-space(.)=concat('It', \"'\", 's a \"test\"')]"));
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    public void CheckCheckbox_ClicksOnlyWhenStateNeedsToChange(bool selected, bool shouldClick)
    {
        var checkbox = new Mock<IWebElement>();
        checkbox.SetupGet(element => element.TagName).Returns("input");
        checkbox.Setup(element => element.GetAttribute("type")).Returns("checkbox");
        checkbox.SetupGet(element => element.Selected).Returns(selected);

        var driver = new Mock<IWebDriver>();
        driver.Setup(item => item.FindElement(It.IsAny<By>())).Returns(checkbox.Object);

        Locators.CheckCheckbox(driver.Object, By.Id("accept"));

        checkbox.Verify(element => element.Click(), shouldClick ? Times.Once() : Times.Never());
    }

    [Test]
    public void SelectRadioButtonByValue_ClicksMatchingUnselectedRadio()
    {
        var radio = new Mock<IWebElement>();
        radio.SetupGet(element => element.TagName).Returns("input");
        radio.Setup(element => element.GetAttribute("type")).Returns("radio");
        radio.Setup(element => element.GetAttribute("role")).Returns((string)null!);
        radio.Setup(element => element.GetAttribute("value")).Returns("standard");
        radio.SetupGet(element => element.Selected).Returns(false);

        var driver = new Mock<IWebDriver>();
        driver.Setup(item => item.FindElements(It.IsAny<By>()))
            .Returns(new ReadOnlyCollection<IWebElement>([radio.Object]));

        Locators.SelectRadioButtonByValue(driver.Object, By.Name("shipping"), "standard");

        radio.Verify(element => element.Click(), Times.Once);
    }

    [Test]
    public void SelectRadioButtonByIndex_RejectsIndexOutsideGroup()
    {
        var driver = new Mock<IWebDriver>();
        driver.Setup(item => item.FindElements(It.IsAny<By>()))
            .Returns(new ReadOnlyCollection<IWebElement>([]));

        Assert.That(
            () => Locators.SelectRadioButtonByIndex(driver.Object, By.Name("shipping"), 0),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ClickByDriver_ReturnsFalseWhenWebDriverClickFails()
    {
        var driver = new Mock<IWebDriver>();
        driver.Setup(item => item.FindElement(It.IsAny<By>()))
            .Throws(new WebDriverException("click failed"));

        Assert.That(Locators.ClickByDriver(driver.Object, By.Id("submit")), Is.False);
    }

    [Test]
    public void ExecuteLocatorKeyword_LogsInfoThenPassWithNonEmptyDetails()
    {
        CreateLocatorWorkbook(("submit_id", "submit"));
        var driver = new Mock<IWebDriver>();
        var screenshotDriver = driver.As<ITakesScreenshot>();
        screenshotDriver
            .Setup(item => item.GetScreenshot())
            .Returns(new Screenshot(Convert.ToBase64String(new byte[] { 1, 2, 3 })));
        Drivers.driver = driver.Object;

        string screenshotDirectory = PathSetting.Path_ScreenShotsDir;
        bool screenshotDirectoryExisted = Directory.Exists(screenshotDirectory);
        HashSet<string> existingScreenshots = screenshotDirectoryExisted
            ? Directory.GetFiles(screenshotDirectory).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        TextWriter originalOutput = Console.Out;
        using var output = new StringWriter();

        try
        {
            Console.SetOut(output);
            Locators.ExecuteLocatorKeyword("Click", "submit_id", "", (_, _) => { });
        }
        finally
        {
            Console.SetOut(originalOutput);
            if (Directory.Exists(screenshotDirectory))
            {
                foreach (string screenshot in Directory.GetFiles(screenshotDirectory))
                {
                    if (!existingScreenshots.Contains(screenshot))
                        File.Delete(screenshot);
                }

                if (!screenshotDirectoryExisted && !Directory.EnumerateFileSystemEntries(screenshotDirectory).Any())
                    Directory.Delete(screenshotDirectory);
            }
        }

        string[] logLines = output.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.Contains("[INFO   ]", StringComparison.Ordinal)
                || line.Contains("[PASS   ]", StringComparison.Ordinal))
            .ToArray();
        Assert.That(logLines, Has.Length.EqualTo(2));
        Assert.That(logLines[0], Does.Contain("[INFO   ] Click | submit_id"));
        Assert.That(logLines[1], Does.Contain("[PASS   ] Click | submit_id"));
        Assert.That(logLines.Any(line => line.Contains(" |  | ", StringComparison.Ordinal)), Is.False);
        Assert.That(DriverScript.outcome, Is.EqualTo((int)Outcome.Pass));
    }

    [Test]
    public void ExecuteLocatorKeyword_LogsErrorAndOmitsEmptyObjectAndData()
    {
        TextWriter originalOutput = Console.Out;
        using var output = new StringWriter();

        try
        {
            Console.SetOut(output);
            Locators.ExecuteLocatorKeyword("Click", "", " ", (_, _) => { });
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        string[] logLines = output.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.Contains("[INFO   ]", StringComparison.Ordinal)
                || line.Contains("[ERROR  ]", StringComparison.Ordinal))
            .ToArray();
        Assert.That(logLines, Has.Length.EqualTo(2));
        Assert.That(logLines[0], Does.Contain("[INFO   ] Click"));
        Assert.That(logLines[1], Does.Contain("[ERROR  ] Click | Page object name cannot be null or empty."));
        Assert.That(logLines.Any(line => line.Contains(" |  | ", StringComparison.Ordinal)), Is.False);
        Assert.That(DriverScript.outcome, Is.EqualTo((int)Outcome.Error));
    }

    [Test]
    public void FindElementReturnsFirstMatchOrNull()
    {
        var first = Mock.Of<IWebElement>();
        var second = Mock.Of<IWebElement>();
        var driver = new Mock<IWebDriver>();
        driver.Setup(item => item.FindElements(It.IsAny<By>()))
            .Returns(new ReadOnlyCollection<IWebElement>([first, second]));

        Assert.That(Assertions.FindElement(driver.Object, By.ClassName("item")), Is.SameAs(first));

        driver.Setup(item => item.FindElements(It.IsAny<By>()))
            .Returns(new ReadOnlyCollection<IWebElement>([]));
        Assert.That(Assertions.FindElement(driver.Object, By.ClassName("item")), Is.Null);
    }
}
