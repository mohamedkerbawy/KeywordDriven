using System;
using System.Linq;
using System.Text.RegularExpressions;
using KeywordDriven.Execution;
using KeywordDriven.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace KeywordDriven.ActionKeywords
{
    internal static class Assertions
    {
        private static bool EvaluateMatch(string actual, string expected, string matchType, bool ignoreCase)
        {
            ArgumentNullException.ThrowIfNull(actual);
            ArgumentNullException.ThrowIfNull(expected);
            ArgumentException.ThrowIfNullOrWhiteSpace(matchType);

            StringComparison comparison = ignoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            return matchType.Trim().ToLowerInvariant() switch
            {
                "exact" => string.Equals(actual, expected, comparison),
                "contains" => actual.Contains(expected, comparison),
                "startswith" => actual.StartsWith(expected, comparison),
                "endswith" => actual.EndsWith(expected, comparison),
                "notequal" => !string.Equals(actual, expected, comparison),
                "notcontains" => !actual.Contains(expected, comparison),
                "regex" => Regex.IsMatch(
                    actual,
                    expected,
                    RegexOptions.CultureInvariant | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None),
                    TimeSpan.FromSeconds(2)),
                _ => throw new ArgumentException(
                    $"Unsupported matchType: '{matchType}'. Valid: exact, contains, startswith, endswith, notequal, notcontains, regex",
                    nameof(matchType))
            };
        }

        internal static void Run(string keyword, string details, Func<bool> assertion)
        {
            Log.Info($"{keyword}: {details}");
            ExtentReporter.NodeInfo($"{keyword}: {details}");

            try
            {
                if (assertion())
                {
                    DriverScript.outcome = (int)Outcome.Pass;
                    Log.Pass($"{keyword}: passed");
                    ExtentReporter.NodePass($"{keyword}: passed");
                }
                else
                {
                    DriverScript.outcome = (int)Outcome.Fail;
                    Log.Fail($"{keyword}: assertion failed ({details})");
                    ExtentReporter.NodeFail($"{keyword}: assertion failed ({details})");
                }
            }
            catch (Exception e)
            {
                DriverScript.outcome = (int)Outcome.Error;
                Log.Error($"{keyword}: {e.Message}");
                ExtentReporter.NodeError($"{keyword}: {e.Message}");
            }
        }

        internal static IWebElement FindElement(IWebDriver driver, By locator)
        {
            return driver.FindElements(locator).FirstOrDefault();
        }

        private static string GetElementText(IWebElement element)
        {
            string tagName = element.TagName;
            return tagName.Equals("input", StringComparison.OrdinalIgnoreCase)
                || tagName.Equals("textarea", StringComparison.OrdinalIgnoreCase)
                ? element.GetAttribute("value") ?? string.Empty
                : element.Text;
        }

        internal static bool IsEditable(IWebElement element)
        {
            return element.Displayed
                && element.Enabled
                && element.GetAttribute("disabled") == null
                && element.GetAttribute("aria-disabled") != "true"
                && element.GetAttribute("readonly") == null
                && element.GetAttribute("aria-readonly") != "true";
        }

        internal static bool HasExpectedSelection(IWebElement element, string expectedType, bool selected)
        {
            string tagName = element.TagName;
            string type = element.GetAttribute("type");
            string role = element.GetAttribute("role");
            bool isMatchingControl =
                (tagName.Equals("input", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(type, expectedType, StringComparison.OrdinalIgnoreCase))
                || string.Equals(role, expectedType, StringComparison.OrdinalIgnoreCase);

            if (!isMatchingControl)
                return false;

            bool isNativeInput = tagName.Equals("input", StringComparison.OrdinalIgnoreCase);
            string ariaChecked = element.GetAttribute("aria-checked");
            if (!isNativeInput && ariaChecked != "true" && ariaChecked != "false")
                return false;

            bool actual = isNativeInput ? element.Selected : ariaChecked == "true";
            return actual == selected;
        }

        internal static bool AssertElementMatch(string obj, string data, string matchType, bool ignoreCase = false)
        {
            By locator = Locators.GetLocator(obj);
            IWebElement element = FindElement(Drivers.driver, locator);
            return element != null
                && EvaluateMatch(GetElementText(element), data, matchType, ignoreCase);
        }

        internal static bool AssertPageMatch(string actual, string expected, string matchType, bool ignoreCase = false)
        {
            return EvaluateMatch(actual, expected, matchType, ignoreCase);
        }

        private static string[] ParseAttributeAssertion(string data)
        {
            int separator = data.IndexOf('=');
            return separator < 0
                ? [data.Trim(), null]
                : [data[..separator].Trim(), data[(separator + 1)..]];
        }

        internal static bool AssertAttribute(string obj, string data, bool shouldExist)
        {
            string[] assertion = ParseAttributeAssertion(data);
            if (string.IsNullOrWhiteSpace(assertion[0]))
                throw new ArgumentException("Attribute assertion data must include an attribute name.", nameof(data));

            IWebElement element = FindElement(Drivers.driver, Locators.GetLocator(obj));
            if (element == null)
                return false;

            string actual = element.GetDomAttribute(assertion[0]);
            if (shouldExist)
            {
                return actual != null
                    && (assertion[1] == null || string.Equals(actual, assertion[1], StringComparison.Ordinal));
            }

            return assertion[1] == null
                ? actual == null
                : !string.Equals(actual, assertion[1], StringComparison.Ordinal);
        }

        internal static bool AssertSelectedOption(string obj, string data, string selectionType)
        {
            IWebElement element = FindElement(Drivers.driver, Locators.GetLocator(obj));
            if (element == null)
                return false;

            SelectElement select = new(element);
            IWebElement selectedOption = select.AllSelectedOptions.FirstOrDefault();
            if (selectedOption == null)
                return false;

            return selectionType switch
            {
                "text" => string.Equals(selectedOption.Text, data, StringComparison.Ordinal),
                "value" => string.Equals(selectedOption.GetAttribute("value"), data, StringComparison.Ordinal),
                "index" => int.TryParse(data, out int expectedIndex)
                    && select.Options.ToList().FindIndex(option => option.Selected) == expectedIndex,
                _ => throw new ArgumentException($"Unsupported selection type: {selectionType}", nameof(selectionType))
            };
        }

        internal static bool AssertUrlMatch(string data, string matchType, bool ignoreCase = false)
        {
            return AssertPageMatch(Drivers.driver.Url, data, matchType, ignoreCase);
        }

        internal static bool AssertElementState(string obj, Func<IWebElement, bool> predicate)
        {
            IWebElement element = FindElement(Drivers.driver, Locators.GetLocator(obj));
            return element != null && predicate(element);
        }
    }

    public partial class Keywords
    {
        #region Keywords methods

        public static void AssertElementExists(String obj, String data) =>
            Assertions.Run(nameof(AssertElementExists), obj, () => Assertions.FindElement(Drivers.driver, Locators.GetLocator(obj)) != null);

        public static void AssertElementNotExists(String obj, String data) =>
            Assertions.Run(nameof(AssertElementNotExists), obj, () => Assertions.FindElement(Drivers.driver, Locators.GetLocator(obj)) == null);

        public static void AssertElementVisible(String obj, String data) =>
            Assertions.Run(nameof(AssertElementVisible), obj, () => Assertions.AssertElementState(obj, element => element.Displayed));

        public static void AssertElementNotVisible(String obj, String data) =>
            Assertions.Run(nameof(AssertElementNotVisible), obj, () =>
            {
                IWebElement element = Assertions.FindElement(Drivers.driver, Locators.GetLocator(obj));
                return element == null || !element.Displayed;
            });

        public static void AssertElementEditable(String obj, String data) =>
            Assertions.Run(nameof(AssertElementEditable), obj, () => Assertions.AssertElementState(obj, Assertions.IsEditable));

        public static void AssertElementNotEditable(String obj, String data) =>
            Assertions.Run(nameof(AssertElementNotEditable), obj, () => Assertions.AssertElementState(obj, element => !Assertions.IsEditable(element)));

        public static void AssertElementEnabled(String obj, String data) =>
            Assertions.Run(nameof(AssertElementEnabled), obj, () => Assertions.AssertElementState(
                obj, element => element.Enabled && element.GetAttribute("aria-disabled") != "true"));

        public static void AssertElementDisabled(String obj, String data) =>
            Assertions.Run(nameof(AssertElementDisabled), obj, () => Assertions.AssertElementState(
                obj, element => !element.Enabled || element.GetAttribute("aria-disabled") == "true"));

        public static void AssertElementContainsText(String obj, String data) =>
            Assertions.Run(nameof(AssertElementContainsText), obj, () => Assertions.AssertElementMatch(obj, data, "contains"));

        public static void AssertElementHaveAttribute(String obj, String data) =>
            Assertions.Run(nameof(AssertElementHaveAttribute), obj, () => Assertions.AssertAttribute(obj, data, shouldExist: true));

        public static void AssertElementNotHaveAttribute(String obj, String data) =>
            Assertions.Run(nameof(AssertElementNotHaveAttribute), obj, () => Assertions.AssertAttribute(obj, data, shouldExist: false));

        public static void AssertElementNotContainsText(String obj, String data) =>
            Assertions.Run(nameof(AssertElementNotContainsText), obj, () => Assertions.AssertElementMatch(obj, data, "notcontains"));

        public static void AssertElementHaveText(String obj, String data) =>
            Assertions.Run(nameof(AssertElementHaveText), obj, () => Assertions.AssertElementMatch(obj, data, "exact"));

        public static void AssertElementNotHaveText(String obj, String data) =>
            Assertions.Run(nameof(AssertElementNotHaveText), obj, () => Assertions.AssertElementMatch(obj, data, "notequal"));

        public static void AssertElementTextStartsWith(String obj, String data) =>
            Assertions.Run(nameof(AssertElementTextStartsWith), obj, () => Assertions.AssertElementMatch(obj, data, "startswith"));

        public static void AssertElementTextEndsWith(String obj, String data) =>
            Assertions.Run(nameof(AssertElementTextEndsWith), obj, () => Assertions.AssertElementMatch(obj, data, "endswith"));

        public static void AssertElementTextMatchesPattern(String obj, String data) =>
             Assertions.Run(nameof(AssertElementTextMatchesPattern), obj, () => Assertions.AssertElementMatch(obj, data, "regex"));

        public static void AssertElementFocused(String obj, String data) =>
            Assertions.Run(nameof(AssertElementFocused), obj, () =>
            {
                IWebElement element = Assertions.FindElement(Drivers.driver, Locators.GetLocator(obj));
                return element != null && element.Equals(Drivers.driver.SwitchTo().ActiveElement());
            });

        public static void AssertElementHaveAriaRole(String obj, String data) =>
            Assertions.Run(nameof(AssertElementHaveAriaRole), obj, () =>
                Assertions.AssertElementState(obj, element =>
                    string.Equals(element.GetAttribute("role"), data, StringComparison.Ordinal)));

        public static void AssertElementNotHaveAriaRole(String obj, String data) =>
            Assertions.Run(nameof(AssertElementNotHaveAriaRole), obj, () =>
                Assertions.AssertElementState(obj, element =>
                    !string.Equals(element.GetAttribute("role"), data, StringComparison.Ordinal)));

        public static void AssertSelectedText(String obj, String data) =>
            Assertions.Run(nameof(AssertSelectedText), obj, () => Assertions.AssertSelectedOption(obj, data, "text"));

        public static void AssertSelectedValue(String obj, String data) =>
            Assertions.Run(nameof(AssertSelectedValue), obj, () => Assertions.AssertSelectedOption(obj, data, "value"));

        public static void AssertSelectedIndex(String obj, String data) =>
            Assertions.Run(nameof(AssertSelectedIndex), obj, () => Assertions.AssertSelectedOption(obj, data, "index"));

        public static void AssertCheckboxChecked(String obj, String data) =>
            Assertions.Run(nameof(AssertCheckboxChecked), obj, () => Assertions.AssertElementState(
                obj, element => Assertions.HasExpectedSelection(element, "checkbox", selected: true)));

        public static void AssertCheckboxNotChecked(String obj, String data) =>
            Assertions.Run(nameof(AssertCheckboxNotChecked), obj, () => Assertions.AssertElementState(
                obj, element => Assertions.HasExpectedSelection(element, "checkbox", selected: false)));

        public static void AssertRadioSelected(String obj, String data) =>
            Assertions.Run(nameof(AssertRadioSelected), obj, () => Assertions.AssertElementState(
                obj, element => Assertions.HasExpectedSelection(element, "radio", selected: true)));

        public static void AssertRadioNotSelected(String obj, String data) =>
            Assertions.Run(nameof(AssertRadioNotSelected), obj, () => Assertions.AssertElementState(
                obj, element => Assertions.HasExpectedSelection(element, "radio", selected: false)));

        public static void AssertRadioSeleced(String obj, String data) => AssertRadioSelected(obj, data);

        public static void AssertRadioNotSeleced(String obj, String data) => AssertRadioNotSelected(obj, data);

        public static void AssertPageContainsTitle(String obj, String data) =>
            Assertions.Run(nameof(AssertPageContainsTitle), data, () => Assertions.AssertPageMatch(Drivers.driver.Title, data, "contains"));

        public static void AssertPageNotContainsTitle(String obj, String data) =>
            Assertions.Run(nameof(AssertPageNotContainsTitle), data, () => Assertions.AssertPageMatch(Drivers.driver.Title, data, "notcontains"));

        public static void AssertPageHaveTitle(String obj, String data) =>
            Assertions.Run(nameof(AssertPageHaveTitle), data, () => Assertions.AssertPageMatch(Drivers.driver.Title, data, "exact"));

        public static void AssertPageNotHaveTitle(String obj, String data) =>
            Assertions.Run(nameof(AssertPageNotHaveTitle), data, () => Assertions.AssertPageMatch(Drivers.driver.Title, data, "notequal"));

        public static void AssertPageContainsUrl(String obj, String data) =>
            Assertions.Run(nameof(AssertPageContainsUrl), data, () => Assertions.AssertUrlMatch(data, "contains"));

        public static void AssertPageHaveUrl(String obj, String data) =>
            Assertions.Run(nameof(AssertPageHaveUrl), data, () => Assertions.AssertUrlMatch(data, "exact"));

        public static void AssertPageUrlStartsWith(String obj, String data) =>
            Assertions.Run(nameof(AssertPageUrlStartsWith), data, () => Assertions.AssertUrlMatch(data, "startswith"));

        public static void AssertPageUrlEndsWith(String obj, String data) =>
            Assertions.Run(nameof(AssertPageUrlEndsWith), data, () => Assertions.AssertUrlMatch(data, "endswith"));

        public static void AssertPageUrlMatchesPattern(String obj, String data) =>
            Assertions.Run(nameof(AssertPageUrlMatchesPattern), data, () => Assertions.AssertUrlMatch(data, "regex"));
        #endregion
    }
}
