using System;
using System.Collections.Generic;
using KeywordDriven.Execution;
using KeywordDriven.Utils;
using OpenQA.Selenium;

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

        internal static string VerificationBooleanValue(bool value) => value ? "true" : "false";
        
        internal static void RunVerificationElementState(string keyword, string obj, string expected, Func<IWebElement, bool> predicate, Func<IWebElement, string> actual, bool missingIsPass = false) =>
            Run(keyword, obj, () => Assertions.EvaluateElementState(obj, expected, actual, predicate, missingIsPass));
        
        internal static void RunVerificationSelection(string keyword, string obj, string type, bool selected) =>
            RunVerificationElementState(keyword, obj, selected ? "selected" : "not selected",
                element => Assertions.HasExpectedSelection(element, type, selected),
                element => Assertions.GetSelectionState(element, type));
        
        internal static void Run(string keyword, string details, Func<Assertions.Result> verification)
        {
            currentStepFailed = false;
            Log.Info($"{keyword} | {details}");
            ExtentReporter.NodeInfo($"{keyword} | {details}");

            try
            {
                Assertions.Result result = verification();
                string message = $"{keyword} | {details} | Expected: {result.Expected} | Actual: {result.Actual}";
                if (result.Passed)
                {
                    Log.Pass($"{message} | PASS");
                    ExtentReporter.NodePass($"{message} | PASS");
                    return;
                }

                currentStepFailed = true;
                failures.Add($"{keyword} | {details}");
                Log.Fail($"{message} | FAIL");
                ExtentReporter.NodeFail($"{message} | FAIL");
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

        public static void VerifyElementExists(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementExists), obj, () => Assertions.EvaluateElementExists(obj, shouldExist: true));
        public static void VerifyElementNotExists(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementNotExists), obj, () => Assertions.EvaluateElementExists(obj, shouldExist: false));
        public static void VerifyElementVisible(string obj, string data) =>
            Verifications.RunVerificationElementState(nameof(VerifyElementVisible), obj, "true", element => element.Displayed, element => Verifications.VerificationBooleanValue(element.Displayed));
        public static void VerifyElementNotVisible(string obj, string data) =>
            Verifications.RunVerificationElementState(nameof(VerifyElementNotVisible), obj, "false", element => !element.Displayed, element => Verifications.VerificationBooleanValue(element.Displayed), missingIsPass: true);
        public static void VerifyElementEditable(string obj, string data) =>
            Verifications.RunVerificationElementState(nameof(VerifyElementEditable), obj, "editable", Assertions.IsEditable, element => Assertions.IsEditable(element) ? "editable" : "not editable");
        public static void VerifyElementNotEditable(string obj, string data) =>
            Verifications.RunVerificationElementState(nameof(VerifyElementNotEditable), obj, "not editable", element => !Assertions.IsEditable(element), element => Assertions.IsEditable(element) ? "editable" : "not editable");
        public static void VerifyElementEnabled(string obj, string data) =>
            Verifications.RunVerificationElementState(nameof(VerifyElementEnabled), obj, "enabled", element => element.Enabled && element.GetAttribute("aria-disabled") != "true",
                element => element.Enabled && element.GetAttribute("aria-disabled") != "true" ? "enabled" : "disabled");
        public static void VerifyElementDisabled(string obj, string data) =>
            Verifications.RunVerificationElementState(nameof(VerifyElementDisabled), obj, "disabled", element => !element.Enabled || element.GetAttribute("aria-disabled") == "true",
                element => !element.Enabled || element.GetAttribute("aria-disabled") == "true" ? "disabled" : "enabled");
        public static void VerifyElementFocused(string obj, string data) =>
            Verifications.RunVerificationElementState(nameof(VerifyElementFocused), obj, "focused",
                element => element.Equals(Drivers.driver.SwitchTo().ActiveElement()),
                element => element.Equals(Drivers.driver.SwitchTo().ActiveElement()) ? "focused" : "not focused");
        public static void VerifyElementHaveAttribute(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementHaveAttribute), obj, () => Assertions.EvaluateAttribute(obj, data, shouldExist: true));
        public static void VerifyElementNotHaveAttribute(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementNotHaveAttribute), obj, () => Assertions.EvaluateAttribute(obj, data, shouldExist: false));
        public static void VerifyElementHaveAriaRole(string obj, string data) =>
            Verifications.RunVerificationElementState(nameof(VerifyElementHaveAriaRole), obj, data,
                element => string.Equals(element.GetAttribute("role"), data, StringComparison.Ordinal),
                element => element.GetAttribute("role") ?? "<missing>");
        public static void VerifyElementNotHaveAriaRole(string obj, string data) =>
            Verifications.RunVerificationElementState(nameof(VerifyElementNotHaveAriaRole), obj, $"not {data}",
                element => !string.Equals(element.GetAttribute("role"), data, StringComparison.Ordinal),
                element => element.GetAttribute("role") ?? "<missing>");
        public static void VerifyElementContainsText(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementContainsText), obj, () => Assertions.EvaluateElementMatch(obj, data, "contains"));
        public static void VerifyElementNotContainsText(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementNotContainsText), obj, () => Assertions.EvaluateElementMatch(obj, data, "notcontains"));
        public static void VerifyElementHaveText(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementHaveText), obj, () => Assertions.EvaluateElementMatch(obj, data, "exact"));
        public static void VerifyElementNotHaveText(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementNotHaveText), obj, () => Assertions.EvaluateElementMatch(obj, data, "notequal"));
        public static void VerifyElementTextStartsWith(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementTextStartsWith), obj, () => Assertions.EvaluateElementMatch(obj, data, "startswith"));
        public static void VerifyElementTextEndsWith(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementTextEndsWith), obj, () => Assertions.EvaluateElementMatch(obj, data, "endswith"));
        public static void VerifyElementTextMatchesPattern(string obj, string data) =>
            Verifications.Run(nameof(VerifyElementTextMatchesPattern), obj, () => Assertions.EvaluateElementMatch(obj, data, "regex"));
        public static void VerifySelectedText(string obj, string data) =>
            Verifications.Run(nameof(VerifySelectedText), obj, () => Assertions.EvaluateSelectedOption(obj, data, "text"));
        public static void VerifySelectedValue(string obj, string data) =>
            Verifications.Run(nameof(VerifySelectedValue), obj, () => Assertions.EvaluateSelectedOption(obj, data, "value"));
        public static void VerifySelectedIndex(string obj, string data) =>
            Verifications.Run(nameof(VerifySelectedIndex), obj, () => Assertions.EvaluateSelectedOption(obj, data, "index"));
        public static void VerifyCheckboxChecked(string obj, string data) => Verifications.RunVerificationSelection(nameof(VerifyCheckboxChecked), obj, "checkbox", true);
        public static void VerifyCheckboxNotChecked(string obj, string data) => Verifications.RunVerificationSelection(nameof(VerifyCheckboxNotChecked), obj, "checkbox", false);
        public static void VerifyRadioSelected(string obj, string data) => Verifications.RunVerificationSelection(nameof(VerifyRadioSelected), obj, "radio", true);
        public static void VerifyRadioNotSelected(string obj, string data) => Verifications.RunVerificationSelection(nameof(VerifyRadioNotSelected), obj, "radio", false);
        public static void VerifyPageContainsTitle(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageContainsTitle), data, () => Assertions.EvaluatePageMatch(Drivers.driver.Title, data, "contains"));
        public static void VerifyPageNotContainsTitle(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageNotContainsTitle), data, () => Assertions.EvaluatePageMatch(Drivers.driver.Title, data, "notcontains"));
        public static void VerifyPageHaveTitle(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageHaveTitle), data, () => Assertions.EvaluatePageMatch(Drivers.driver.Title, data, "exact"));
        public static void VerifyPageNotHaveTitle(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageNotHaveTitle), data, () => Assertions.EvaluatePageMatch(Drivers.driver.Title, data, "notequal"));
        public static void VerifyPageContainsUrl(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageContainsUrl), data, () => Assertions.EvaluateUrlMatch(data, "contains"));
        public static void VerifyPageNotContainsUrl(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageNotContainsUrl), data, () => Assertions.EvaluateUrlMatch(data, "notcontains"));
        public static void VerifyPageHaveUrl(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageHaveUrl), data, () => Assertions.EvaluateUrlMatch(data, "exact"));
        public static void VerifyPageUrlStartsWith(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageUrlStartsWith), data, () => Assertions.EvaluateUrlMatch(data, "startswith"));
        public static void VerifyPageUrlEndsWith(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageUrlEndsWith), data, () => Assertions.EvaluateUrlMatch(data, "endswith"));
        public static void VerifyPageUrlMatchesPattern(string obj, string data) =>
            Verifications.Run(nameof(VerifyPageUrlMatchesPattern), data, () => Assertions.EvaluateUrlMatch(data, "regex"));

        #endregion
    }
}
