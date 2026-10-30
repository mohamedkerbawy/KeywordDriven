using System;
using System.Collections.Generic;
using KeywordDriven.Execution;
using KeywordDriven.Utils;

namespace KeywordDriven.ActionKeywords
{
    internal static class Verifications
    {
        private static readonly List<string> failures = new();
        private static bool currentStepFailed;

        internal static bool HasFailures => failures.Count > 0;
        internal static bool CurrentStepFailed => currentStepFailed;
        internal static string FailureSummary => string.Join("; ", failures);

        internal static void BeginTestCase()
        {
            failures.Clear();
            currentStepFailed = false;
        }

        internal static void Run(string keyword, string details, Func<bool> verification)
        {
            currentStepFailed = false;
            Log.Info($"{keyword}: {details}");
            ExtentReporter.NodeInfo($"{keyword}: {details}");

            try
            {
                if (verification())
                {
                    Log.Pass($"{keyword}: passed");
                    ExtentReporter.NodePass($"{keyword}: passed");
                    return;
                }

                currentStepFailed = true;
                failures.Add($"{keyword}: {details}");
                Log.Fail($"{keyword}: verification failed ({details})");
                ExtentReporter.NodeFail($"{keyword}: verification failed ({details})");
            }
            catch (Exception exception)
            {
                DriverScript.outcome = (int)Outcome.Error;
                Log.Error($"{keyword}: {exception.Message}");
                ExtentReporter.NodeError($"{keyword}: {exception.Message}");
            }
        }
    }

    public partial class Keywords
    {
        #region Keywords methods

        public static void VerifyElementExists(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementExists), obj,
                () => Assertions.FindElement(Drivers.driver, Locators.GetLocator(obj)) != null);

        public static void VerifyElementNotExists(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementNotExists), obj,
                () => Assertions.FindElement(Drivers.driver, Locators.GetLocator(obj)) == null);

        public static void VerifyElementVisible(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementVisible), obj,
                () => Assertions.AssertElementState(obj, element => element.Displayed));

        public static void VerifyElementNotVisible(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementNotVisible), obj, () =>
            {
                var element = Assertions.FindElement(Drivers.driver, Locators.GetLocator(obj));
                return element == null || !element.Displayed;
            });

        public static void VerifyElementEditable(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementEditable), obj,
                () => Assertions.AssertElementState(obj, Assertions.IsEditable));

        public static void VerifyElementNotEditable(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementNotEditable), obj,
                () => Assertions.AssertElementState(obj, element => !Assertions.IsEditable(element)));

        public static void VerifyElementEnabled(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementEnabled), obj,
                () => Assertions.AssertElementState(
                    obj, element => element.Enabled && element.GetAttribute("aria-disabled") != "true"));

        public static void VerifyElementDisabled(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementDisabled), obj,
                () => Assertions.AssertElementState(
                    obj, element => !element.Enabled || element.GetAttribute("aria-disabled") == "true"));

        public static void VerifyElementFocused(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementFocused), obj, () =>
            {
                var element = Assertions.FindElement(Drivers.driver, Locators.GetLocator(obj));
                return element != null && element.Equals(Drivers.driver.SwitchTo().ActiveElement());
            });

        public static void VerifyElementHaveAttribute(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementHaveAttribute), obj,
                () => Assertions.AssertAttribute(obj, data, shouldExist: true));

        public static void VerifyElementNotHaveAttribute(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementNotHaveAttribute), obj,
                () => Assertions.AssertAttribute(obj, data, shouldExist: false));

        public static void VerifyElementHaveAriaRole(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementHaveAriaRole), obj,
                () => Assertions.AssertElementState(obj, element =>
                    string.Equals(element.GetAttribute("role"), data, StringComparison.Ordinal)));

        public static void VerifyElementNotHaveAriaRole(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementNotHaveAriaRole), obj,
                () => Assertions.AssertElementState(obj, element =>
                    !string.Equals(element.GetAttribute("role"), data, StringComparison.Ordinal)));

        public static void VerifyElementContainsText(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementContainsText), obj,
                () => Assertions.AssertElementMatch(obj, data, "contains"));

        public static void VerifyElementNotContainsText(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementNotContainsText), obj,
                () => Assertions.AssertElementMatch(obj, data, "notcontains"));

        public static void VerifyElementHaveText(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementHaveText), obj,
                () => Assertions.AssertElementMatch(obj, data, "exact"));

        public static void VerifyElementNotHaveText(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementNotHaveText), obj,
                () => Assertions.AssertElementMatch(obj, data, "notequal"));

        public static void VerifyElementTextStartsWith(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementTextStartsWith), obj,
                () => Assertions.AssertElementMatch(obj, data, "startswith"));

        public static void VerifyElementTextEndsWith(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementTextEndsWith), obj,
                () => Assertions.AssertElementMatch(obj, data, "endswith"));

        public static void VerifyElementTextMatchesPattern(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementTextMatchesPattern), obj,
                () => Assertions.AssertElementMatch(obj, data, "regex"));

        public static void VerifySelectedText(string obj, string data) =>
            Verifications.Run(nameof(VerifySelectedText), obj,
                () => Assertions.AssertSelectedOption(obj, data, "text"));

        public static void VerifySelectedValue(string obj, string data) =>
            Verifications.Run(nameof(VerifySelectedValue), obj,
                () => Assertions.AssertSelectedOption(obj, data, "value"));

        public static void VerifySelectedIndex(string obj, string data) =>
            Verifications.Run(nameof(VerifySelectedIndex), obj,
                () => Assertions.AssertSelectedOption(obj, data, "index"));

        public static void VerifyCheckboxChecked(string obj, string data) =>
            Verifications.Run(nameof(VerifyCheckboxChecked), obj,
                () => Assertions.AssertElementState(
                    obj, element => Assertions.HasExpectedSelection(element, "checkbox", selected: true)));

        public static void VerifyCheckboxNotChecked(string obj, string data) =>
            Verifications.Run(nameof(VerifyCheckboxNotChecked), obj,
                () => Assertions.AssertElementState(
                    obj, element => Assertions.HasExpectedSelection(element, "checkbox", selected: false)));

        public static void VerifyRadioSelected(string obj, string data) =>
            Verifications.Run(nameof(VerifyRadioSelected), obj,
                () => Assertions.AssertElementState(
                    obj, element => Assertions.HasExpectedSelection(element, "radio", selected: true)));

        public static void VerifyRadioNotSelected(string obj, string data) =>
            Verifications.Run(nameof(VerifyRadioNotSelected), obj,
                () => Assertions.AssertElementState(
                    obj, element => Assertions.HasExpectedSelection(element, "radio", selected: false)));

        public static void VerifyRadioSeleced(string obj, string data) => VerifyRadioSelected(obj, data);

        public static void VerifyRadioNotSeleced(string obj, string data) => VerifyRadioNotSelected(obj, data);

        public static void VerifyPageContainsTitle(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageContainsTitle), data,
                () => Assertions.AssertPageMatch(Drivers.driver.Title, data, "contains"));

        public static void VerifyPageNotContainsTitle(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageNotContainsTitle), data,
                () => Assertions.AssertPageMatch(Drivers.driver.Title, data, "notcontains"));

        public static void VerifyPageHaveTitle(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageHaveTitle), data,
                () => Assertions.AssertPageMatch(Drivers.driver.Title, data, "exact"));

        public static void VerifyPageNotHaveTitle(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageNotHaveTitle), data,
                () => Assertions.AssertPageMatch(Drivers.driver.Title, data, "notequal"));

        public static void VerifyPageContainsUrl(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageContainsUrl), data,
                () => Assertions.AssertUrlMatch(data, "contains"));

        public static void VerifyPageNotContainsUrl(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageNotContainsUrl), data,
                () => Assertions.AssertUrlMatch(data, "notcontains"));

        public static void VerifyPageHaveUrl(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageHaveUrl), data,
                () => Assertions.AssertUrlMatch(data, "exact"));

        public static void VerifyPageUrlStartsWith(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageUrlStartsWith), data,
                () => Assertions.AssertUrlMatch(data, "startswith"));

        public static void VerifyPageUrlEndsWith(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageUrlEndsWith), data,
                () => Assertions.AssertUrlMatch(data, "endswith"));

        public static void VerifyPageUrlMatchesPattern(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageUrlMatchesPattern), data,
                () => Assertions.AssertUrlMatch(data, "regex"));

        #endregion
    }
}
