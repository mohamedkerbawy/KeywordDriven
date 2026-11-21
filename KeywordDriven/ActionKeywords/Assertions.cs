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
        internal sealed record Result(string Expected, string Actual, bool Passed);

        private static string FormatValue(string value) =>
            value == null ? "<null>" : System.Text.Json.JsonSerializer.Serialize(value);

        private static Result CreateResult(string expected, string actual, bool passed) =>
            new(expected, actual, passed);

        internal static Result EvaluateElementExists(string obj, bool shouldExist)
        {
            IWebElement element = FindElement(Drivers.driver, Locators.GetLocator(obj));
            bool exists = element != null;
            return CreateResult(
                shouldExist ? "element exists" : "element does not exist",
                exists ? "element exists" : "element does not exist",
                exists == shouldExist);
        }

        internal static Result EvaluateElementState(string obj, string expected, Func<IWebElement, string> actualValue, Func<IWebElement, bool> predicate, bool missingIsPass = false)
        {
            IWebElement element = FindElement(Drivers.driver, Locators.GetLocator(obj));
            if (element == null)
                return CreateResult(expected, "element not found", missingIsPass);

            string actual = actualValue(element);
            return CreateResult(expected, actual, predicate(element));
        }

        internal static Result EvaluateElementMatch(string obj, string expected, string matchType)
        {
            IWebElement element = FindElement(Drivers.driver, Locators.GetLocator(obj));
            if (element == null)
                return CreateResult($"{matchType} {FormatValue(expected)}", "element not found", false);

            string actual = GetElementText(element);
            return CreateResult(
                $"{matchType} {FormatValue(expected)}",
                FormatValue(actual),
                EvaluateMatch(actual, expected, matchType, ignoreCase: false));
        }

        internal static Result EvaluatePageMatch(string actual, string expected, string matchType)
        {
            return CreateResult(
                $"{matchType} {FormatValue(expected)}",
                FormatValue(actual),
                EvaluateMatch(actual, expected, matchType, ignoreCase: false));
        }

        internal static Result EvaluateAttribute(string obj, string data, bool shouldExist)
        {
            string[] assertion = ParseAttributeAssertion(data);
            if (string.IsNullOrWhiteSpace(assertion[0]))
                throw new ArgumentException("Attribute assertion data must include an attribute name.", nameof(data));

            IWebElement element = FindElement(Drivers.driver, Locators.GetLocator(obj));
            string attributeValue = element?.GetDomAttribute(assertion[0]);
            string actual = element == null ? "element not found" : FormatValue(attributeValue);
            string expected = shouldExist
                ? assertion[1] == null
                    ? $"attribute {FormatValue(assertion[0])} exists"
                    : $"attribute {FormatValue(assertion[0])} equals {FormatValue(assertion[1])}"
                : assertion[1] == null
                    ? $"attribute {FormatValue(assertion[0])} does not exist"
                    : $"attribute {FormatValue(assertion[0])} does not equal {FormatValue(assertion[1])}";

            bool passed = element != null && (shouldExist
                ? attributeValue != null
                    && (assertion[1] == null || string.Equals(attributeValue, assertion[1], StringComparison.Ordinal))
                : assertion[1] == null
                    ? attributeValue == null
                    : !string.Equals(attributeValue, assertion[1], StringComparison.Ordinal));
            return CreateResult(expected, actual, passed);
        }

        internal static Result EvaluateSelectedOption(string obj, string expected, string selectionType)
        {
            IWebElement element = FindElement(Drivers.driver, Locators.GetLocator(obj));
            if (element == null)
                return CreateResult(FormatValue(expected), "element not found", false);

            SelectElement select = new(element);
            IWebElement selectedOption = select.AllSelectedOptions.FirstOrDefault();
            if (selectedOption == null)
                return CreateResult(FormatValue(expected), "no option selected", false);

            string actual = selectionType switch
            {
                "text" => selectedOption.Text,
                "value" => selectedOption.GetAttribute("value"),
                "index" => select.Options.ToList().FindIndex(option => option.Selected).ToString(),
                _ => throw new ArgumentException($"Unsupported selection type: {selectionType}", nameof(selectionType))
            };
            bool passed = selectionType == "index"
                ? int.TryParse(expected, out int expectedIndex)
                    && int.TryParse(actual, out int actualIndex)
                    && actualIndex == expectedIndex
                : string.Equals(actual, expected, StringComparison.Ordinal);
            return CreateResult(FormatValue(expected), FormatValue(actual), passed);
        }

        internal static Result EvaluateUrlMatch(string expected, string matchType) =>
            EvaluatePageMatch(Drivers.driver.Url, expected, matchType);

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

        internal static string GetSelectionState(IWebElement element, string expectedType)
        {
            string tagName = element.TagName;
            string type = element.GetAttribute("type");
            string role = element.GetAttribute("role");
            bool isNativeInput = tagName.Equals("input", StringComparison.OrdinalIgnoreCase)
                && string.Equals(type, expectedType, StringComparison.OrdinalIgnoreCase);
            if (isNativeInput)
                return element.Selected ? "selected" : "not selected";

            if (!string.Equals(role, expectedType, StringComparison.OrdinalIgnoreCase))
                return "unsupported control";

            string ariaChecked = element.GetAttribute("aria-checked");
            return ariaChecked switch
            {
                "true" => "selected",
                "false" => "not selected",
                _ => "unsupported control"
            };
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
        internal static string AssertionBooleanValue(bool value) => value ? "true" : "false";

        internal static bool AssertElementState(string obj, Func<IWebElement, bool> predicate)
        {
            IWebElement element = FindElement(Drivers.driver, Locators.GetLocator(obj));
            return element != null && predicate(element);
        }

        internal static void RunAssertionElementState(string keyword, string obj, string expected, Func<IWebElement, bool> predicate, Func<IWebElement, string> actual, bool missingIsPass = false) =>
            Assertions.Run(keyword, obj, () => Assertions.EvaluateElementState(obj, expected, actual, predicate, missingIsPass));

        internal static void RunAssertionSelection(string keyword, string obj, string type, bool selected) =>
           RunAssertionElementState(keyword, obj, selected ? "selected" : "not selected",
               element => Assertions.HasExpectedSelection(element, type, selected),
               element => Assertions.GetSelectionState(element, type));

        internal static void Run(string keyword, string details, Func<Result> assertion)
        {
            Log.Info($"{keyword} | {details}");
            ExtentReporter.NodeInfo($"{keyword} | {details}");

            try
            {
                Result result = assertion();
                string message = $"{keyword} | {details} | Expected: {result.Expected} | Actual: {result.Actual}";
                if (result.Passed)
                {
                    DriverScript.outcome = (int)Outcome.Pass;
                    Log.Pass($"{message} | PASS");
                    ExtentReporter.NodePass($"{message} | PASS");
                }
                else
                {
                    DriverScript.outcome = (int)Outcome.Fail;
                    Log.Fail($"{message} | FAIL");
                    ExtentReporter.NodeFail($"{message} | FAIL");
                } 
            }
            catch (Exception exception)
            {
                DriverScript.outcome = (int)Outcome.Error;
                Log.Error($"{keyword} | {details} | Expected: unavailable | Actual: unavailable | Exception: {exception.Message}");
                ExtentReporter.NodeError($"{keyword} | {details} | Expected: unavailable | Actual: unavailable | Exception: {exception.Message}");
            }
        }
    }

    public partial class Keywords
    {
        #region Keywords methods

        public static void AssertElementExists(string obj, string data) =>
            Assertions.Run(nameof(AssertElementExists), obj, () => Assertions.EvaluateElementExists(obj, shouldExist: true));
        public static void AssertElementNotExists(string obj, string data) =>
            Assertions.Run(nameof(AssertElementNotExists), obj, () => Assertions.EvaluateElementExists(obj, shouldExist: false));
        public static void AssertElementVisible(string obj, string data) =>
            Assertions.RunAssertionElementState(nameof(AssertElementVisible), obj, "true", element => element.Displayed, element => Assertions.AssertionBooleanValue(element.Displayed));
        public static void AssertElementNotVisible(string obj, string data) =>
            Assertions.RunAssertionElementState(nameof(AssertElementNotVisible), obj, "false", element => !element.Displayed, element => Assertions.AssertionBooleanValue(element.Displayed), missingIsPass: true);
        public static void AssertElementEditable(string obj, string data) =>
            Assertions.RunAssertionElementState(nameof(AssertElementEditable), obj, "editable", Assertions.IsEditable, element => Assertions.IsEditable(element) ? "editable" : "not editable");
        public static void AssertElementNotEditable(string obj, string data) =>
            Assertions.RunAssertionElementState(nameof(AssertElementNotEditable), obj, "not editable", element => !Assertions.IsEditable(element), element => Assertions.IsEditable(element) ? "editable" : "not editable");
        public static void AssertElementEnabled(string obj, string data) =>
            Assertions.RunAssertionElementState(nameof(AssertElementEnabled), obj, "enabled", element => element.Enabled && element.GetAttribute("aria-disabled") != "true",
                element => element.Enabled && element.GetAttribute("aria-disabled") != "true" ? "enabled" : "disabled");
        public static void AssertElementDisabled(string obj, string data) =>
            Assertions.RunAssertionElementState(nameof(AssertElementDisabled), obj, "disabled", element => !element.Enabled || element.GetAttribute("aria-disabled") == "true",
                element => !element.Enabled || element.GetAttribute("aria-disabled") == "true" ? "disabled" : "enabled");
        public static void AssertElementContainsText(string obj, string data) =>
            Assertions.Run(nameof(AssertElementContainsText), obj, () => Assertions.EvaluateElementMatch(obj, data, "contains"));
        public static void AssertElementHaveAttribute(string obj, string data) =>
            Assertions.Run(nameof(AssertElementHaveAttribute), obj, () => Assertions.EvaluateAttribute(obj, data, shouldExist: true));
        public static void AssertElementNotHaveAttribute(string obj, string data) =>
            Assertions.Run(nameof(AssertElementNotHaveAttribute), obj, () => Assertions.EvaluateAttribute(obj, data, shouldExist: false));
        public static void AssertElementNotContainsText(string obj, string data) =>
            Assertions.Run(nameof(AssertElementNotContainsText), obj, () => Assertions.EvaluateElementMatch(obj, data, "notcontains"));
        public static void AssertElementHaveText(string obj, string data) =>
            Assertions.Run(nameof(AssertElementHaveText), obj, () => Assertions.EvaluateElementMatch(obj, data, "exact"));
        public static void AssertElementNotHaveText(string obj, string data) =>
            Assertions.Run(nameof(AssertElementNotHaveText), obj, () => Assertions.EvaluateElementMatch(obj, data, "notequal"));
        public static void AssertElementTextStartsWith(string obj, string data) =>
            Assertions.Run(nameof(AssertElementTextStartsWith), obj, () => Assertions.EvaluateElementMatch(obj, data, "startswith"));
        public static void AssertElementTextEndsWith(string obj, string data) =>
            Assertions.Run(nameof(AssertElementTextEndsWith), obj, () => Assertions.EvaluateElementMatch(obj, data, "endswith"));
        public static void AssertElementTextMatchesPattern(string obj, string data) =>
            Assertions.Run(nameof(AssertElementTextMatchesPattern), obj, () => Assertions.EvaluateElementMatch(obj, data, "regex"));
        public static void AssertElementFocused(string obj, string data) =>
            Assertions.RunAssertionElementState(nameof(AssertElementFocused), obj, "focused",
                element => element.Equals(Drivers.driver.SwitchTo().ActiveElement()),
                element => element.Equals(Drivers.driver.SwitchTo().ActiveElement()) ? "focused" : "not focused");
        public static void AssertElementHaveAriaRole(string obj, string data) =>
            Assertions.RunAssertionElementState(nameof(AssertElementHaveAriaRole), obj, data,
                element => string.Equals(element.GetAttribute("role"), data, StringComparison.Ordinal),
                element => element.GetAttribute("role") ?? "<missing>");
        public static void AssertElementNotHaveAriaRole(string obj, string data) =>
            Assertions.RunAssertionElementState(nameof(AssertElementNotHaveAriaRole), obj, $"not {data}",
                element => !string.Equals(element.GetAttribute("role"), data, StringComparison.Ordinal),
                element => element.GetAttribute("role") ?? "<missing>");
        public static void AssertSelectedText(string obj, string data) =>
            Assertions.Run(nameof(AssertSelectedText), obj, () => Assertions.EvaluateSelectedOption(obj, data, "text"));
        public static void AssertSelectedValue(string obj, string data) =>
            Assertions.Run(nameof(AssertSelectedValue), obj, () => Assertions.EvaluateSelectedOption(obj, data, "value"));
        public static void AssertSelectedIndex(string obj, string data) =>
            Assertions.Run(nameof(AssertSelectedIndex), obj, () => Assertions.EvaluateSelectedOption(obj, data, "index"));       
        public static void AssertCheckboxChecked(string obj, string data) => Assertions.RunAssertionSelection(nameof(AssertCheckboxChecked), obj, "checkbox", true);
        public static void AssertCheckboxNotChecked(string obj, string data) => Assertions.RunAssertionSelection(nameof(AssertCheckboxNotChecked), obj, "checkbox", false);
        public static void AssertRadioSelected(string obj, string data) => Assertions.RunAssertionSelection(nameof(AssertRadioSelected), obj, "radio", true);
        public static void AssertRadioNotSelected(string obj, string data) => Assertions.RunAssertionSelection(nameof(AssertRadioNotSelected), obj, "radio", false);
        public static void AssertPageContainsTitle(string obj, string data) =>
            Assertions.Run(nameof(AssertPageContainsTitle), data, () => Assertions.EvaluatePageMatch(Drivers.driver.Title, data, "contains"));
        public static void AssertPageNotContainsTitle(string obj, string data) =>
            Assertions.Run(nameof(AssertPageNotContainsTitle), data, () => Assertions.EvaluatePageMatch(Drivers.driver.Title, data, "notcontains"));
        public static void AssertPageHaveTitle(string obj, string data) =>
            Assertions.Run(nameof(AssertPageHaveTitle), data, () => Assertions.EvaluatePageMatch(Drivers.driver.Title, data, "exact"));
        public static void AssertPageNotHaveTitle(string obj, string data) =>
            Assertions.Run(nameof(AssertPageNotHaveTitle), data, () => Assertions.EvaluatePageMatch(Drivers.driver.Title, data, "notequal"));
        public static void AssertPageContainsUrl(string obj, string data) =>
            Assertions.Run(nameof(AssertPageContainsUrl), data, () => Assertions.EvaluateUrlMatch(data, "contains"));
        public static void AssertPageNotContainsUrl(string obj, string data) =>
            Assertions.Run(nameof(AssertPageNotContainsUrl), data, () => Assertions.EvaluateUrlMatch(data, "notcontains"));
        public static void AssertPageHaveUrl(string obj, string data) =>
            Assertions.Run(nameof(AssertPageHaveUrl), data, () => Assertions.EvaluateUrlMatch(data, "exact"));
        public static void AssertPageUrlStartsWith(string obj, string data) =>
            Assertions.Run(nameof(AssertPageUrlStartsWith), data, () => Assertions.EvaluateUrlMatch(data, "startswith"));
        public static void AssertPageUrlEndsWith(string obj, string data) =>
            Assertions.Run(nameof(AssertPageUrlEndsWith), data, () => Assertions.EvaluateUrlMatch(data, "endswith"));
        public static void AssertPageUrlMatchesPattern(string obj, string data) =>
            Assertions.Run(nameof(AssertPageUrlMatchesPattern), data, () => Assertions.EvaluateUrlMatch(data, "regex"));
        
        #endregion
    }
}
