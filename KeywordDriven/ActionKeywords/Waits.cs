using KeywordDriven.Config;
using KeywordDriven.Execution;
using KeywordDriven.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Modules.BrowsingContext;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace KeywordDriven.ActionKeywords
{
    internal static class Waits
    {
        private static WebDriverWait Create(IWebDriver driver, int? timeoutSeconds = null)
        {

            int timeout = timeoutSeconds ?? DriverSetting.DefaultTimeoutSeconds;

            return new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));          

            //return new WebDriverWait(driver, TimeSpan.FromSeconds(timeout))
            //{
            //    PollingInterval = TimeSpan.FromMilliseconds(500)
            //};
        }

        private static bool Safe(Func<bool> check, bool valueWhenMissing)
        {
            try { return check(); }
            catch (NoSuchElementException) { return valueWhenMissing; }
            catch (StaleElementReferenceException) { return valueWhenMissing; }
        }

        private static bool ReceivesPointerEvents(IWebDriver driver, IWebElement element)
        {
            const string script = @"
                var el = arguments[0];
                el.scrollIntoView({ block: 'center', inline: 'center' });

                var rect = el.getBoundingClientRect();
                if (rect.width === 0 || rect.height === 0) return false;

                var style = window.getComputedStyle(el);
                if (style.display === 'none' || style.visibility === 'hidden' || style.pointerEvents === 'none') return false;

                var topElement = document.elementFromPoint(rect.left + rect.width / 2, rect.top + rect.height / 2);
                return topElement === el || el.contains(topElement);";

            return (bool)((IJavaScriptExecutor)driver).ExecuteScript(script, element);
        }

        private static bool IsVisibleByJs(IWebDriver driver, IWebElement element)
        {
            const string script = @"
                var el = arguments[0];
                var style = window.getComputedStyle(el);
                if (style.display === 'none' || style.visibility === 'hidden' || style.visibility === 'collapse') return false;
                var rect = el.getBoundingClientRect();
                return rect.width > 0 && rect.height > 0;";

            return (bool)((IJavaScriptExecutor)driver).ExecuteScript(script, element);
        }

        private static bool IsVisible(IWebDriver d, IWebElement e) =>
            e.Displayed || IsVisibleByJs(d, e);

        private static bool IsClickable(IWebDriver d, IWebElement e) =>
            e.Enabled && (e.Displayed || ReceivesPointerEvents(d, e));

        private static bool IsEditable(IWebDriver d, IWebElement e) =>
            IsVisible(d, e) && e.Enabled && e.GetAttribute("readonly") == null;

        private static int? CountByJs(IWebDriver driver, By by)
        {
            string mechanism = by.Mechanism?.ToLowerInvariant();
            string criteria = by.Criteria;
            string script;

            switch (mechanism)
            {
                case "css selector":
                    script = "return document.querySelectorAll(arguments[0]).length;";
                    break;
                case "id":
                    script = "return document.querySelectorAll('#' + CSS.escape(arguments[0])).length;";
                    break;
                case "name":
                    script = "return document.getElementsByName(arguments[0]).length;";
                    break;
                case "class name":
                    script = "return document.getElementsByClassName(arguments[0]).length;";
                    break;
                case "tag name":
                    script = "return document.getElementsByTagName(arguments[0]).length;";
                    break;
                case "xpath":
                    script = "return document.evaluate(arguments[0], document, null, XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, null).snapshotLength;";
                    break;
                default:
                    return null; // custom/unsupported locator: JS can't evaluate it
            }

            return Convert.ToInt32(((IJavaScriptExecutor)driver).ExecuteScript(script, criteria));
        }

        private static bool IsExists(IWebDriver d, By by)
        {
            //driver.FindElements(by).Any()
            if (d.FindElements(by).Count > 0) return true;
            return (CountByJs(d, by) ?? 0) > 0;
        }

        internal static void WaitUntilClickable(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Create(driver, timeoutSeconds);

            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));

            wait.Until(d => Safe(() => IsClickable(d, d.FindElement(by)), false));
            
        }

        internal static void WaitUntilExists(IWebDriver driver, By by, int? timeoutSeconds = null)
        {

            var wait = Create(driver, timeoutSeconds);

            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));

            wait.Until(d => Safe(() => IsExists(d, by), false));
        }

        internal static void WaitUntilNotExists(IWebDriver driver, By by, int? timeoutSeconds = null)
        {

            var wait = Create(driver, timeoutSeconds);

            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(by));

            wait.Until(d => Safe(() => !IsExists(d, by), true));
        }

        internal static void WaitUntilVisible(IWebDriver driver, By by, int? timeoutSeconds = null)
        {

            var wait = Create(driver, timeoutSeconds);

            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));

            wait.Until(d => Safe(() => IsVisible(d, d.FindElement(by)), false));
        }

        internal static void WaitUntilNotVisible(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Create(driver, timeoutSeconds);

            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(by));

            wait.Until(d => Safe(() => !IsVisible(d, d.FindElement(by)), true));
        }

        internal static void WaitUntilAllVisible(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Create(driver, timeoutSeconds);

            wait.Until(d => Safe(() =>
            {
                var elements = d.FindElements(by);
                return elements.Count > 0 && elements.All(e => IsVisible(d, e));
            }, false));
        }

        internal static void WaitUntilAllNotVisible(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Create(driver, timeoutSeconds);

            wait.Until(d => Safe(() => d.FindElements(by).All(e => !IsVisible(d, e)), true));
        }

        internal static void WaitUntilExistsAndVisible(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Create(driver, timeoutSeconds);

            wait.Until(d => Safe(() =>
            {
                var elements = d.FindElements(by);
                return elements.Count > 0 && IsVisible(d, elements[0]);
            }, false));
        }

        internal static void WaitUntilEditable(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Create(driver, timeoutSeconds);

            wait.Until(d => Safe(() => IsEditable(d, d.FindElement(by)), false));
        }

        internal static void WaitUntilNotEditable(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Create(driver, timeoutSeconds);

            wait.Until(d => Safe(() => !IsEditable(d, d.FindElement(by)), false));
        }

        internal static bool WaitUntilUrlContains(IWebDriver driver, string expectedText, int? timeoutSeconds = null)
        {
            // Waits until the current URL contains the expected text (case-sensitive).
            // Returns true when condition is met.

            var wait = Create(driver, timeoutSeconds);

            return wait.Until(d => d.Url.Contains(expectedText, StringComparison.Ordinal));
        }

        internal static bool WaitUntilUrlContainsIgnoreCase(IWebDriver driver, string expectedText, int? timeoutSeconds = null)
        {
            // Waits until the current URL contains the expected text (ignore case-insensitive).
            var wait = Create(driver, timeoutSeconds);

            return wait.Until(d =>
                d.Url.Contains(expectedText, StringComparison.OrdinalIgnoreCase)
            );
        }

        internal static bool WaitUntilUrlEquals(IWebDriver driver, string expectedUrl, int? timeoutSeconds = null)
        {
            // Waits until the current URL exactly matches the expected URL.
            var wait = Create(driver, timeoutSeconds);

            return wait.Until(d =>
                d.Url.Equals(expectedUrl, StringComparison.Ordinal)
            );
        }

        internal static bool WaitUntilUrlStartsWith(IWebDriver driver, string expectedStart, int? timeoutSeconds = null)
        {
            // Waits until the current URL starts with the expected text.
            // Useful for checking base URLs or route prefixes.
            var wait = Create(driver, timeoutSeconds);

            return wait.Until(d =>
                d.Url.StartsWith(expectedStart, StringComparison.OrdinalIgnoreCase)
            );
        }

        internal static bool WaitUntilUrlEndsWith(IWebDriver driver, string expectedEnd, int? timeoutSeconds = null)
        {
            // Waits until the current URL ends with the expected text.
            // Useful for checking page slugs or file extensions.
            var wait = Create(driver, timeoutSeconds);

            return wait.Until(d =>
                d.Url.EndsWith(expectedEnd, StringComparison.OrdinalIgnoreCase)
            );
        }

        internal static bool WaitUntilUrlMatchesPattern(IWebDriver driver, string regexPattern, int? timeoutSeconds = null)
        {
            // Waits until the current URL matches a Regex pattern.
            // Useful for dynamic URLs with IDs or tokens.
            var wait = Create(driver, timeoutSeconds);
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            return wait.Until(d => regex.IsMatch(d.Url));
        }

        internal static bool WaitUntilPageLoaded(IWebDriver driver, int? timeoutSeconds = null)
        {
            var wait = Create(driver, timeoutSeconds);
            var js = (IJavaScriptExecutor)driver;

            return wait.Until(d => js.ExecuteScript("return document.readyState;")?.ToString() == "complete");
        }

        internal static bool WaitUntilTextContains(IWebDriver driver,By by,string expectedText,int? timeoutSeconds = null)
        {
            var wait = Create(driver, timeoutSeconds);
            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException));

            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(by);
                    return element.Displayed
                        && element.Text.Contains(expectedText, StringComparison.Ordinal);
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        internal static void WaitSeconds(int seconds)
        {
            // Waits for a fixed number of milliseconds (hard wait).
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }

        internal static void ExecuteWaitKeyword(string keyword, string details, Action operation)
        {
            string message = string.IsNullOrWhiteSpace(details) ? keyword : $"{keyword} | {details}";
            Log.Info(message);
            ExtentReporter.NodeInfo(message);

            try
            {
                operation();
                DriverScript.outcome = (int)Outcome.Pass;
            }
            catch (Exception exception)
            {
                string errorMessage = $"{keyword} | Exception: {exception.Message}";
                Log.Error(errorMessage);
                ExtentReporter.NodeError(errorMessage);
                DriverScript.outcome = (int)Outcome.Error;
            }
        }
    }

    public partial class Keywords
    {
        #region Keywords methods

        public static void WaitUntilClickable(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilClickable), obj, data,
                (locator, _) => Waits.WaitUntilClickable(Drivers.driver, locator));

        public static void WaitUntilExists(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilExists), obj, data,
                (locator, _) => Waits.WaitUntilExists(Drivers.driver, locator));

        public static void WaitUntilNotExists(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilNotExists), obj, data,
                (locator, _) => Waits.WaitUntilNotExists(Drivers.driver, locator));

        public static void WaitUntilVisible(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilVisible), obj, data,
                (locator, _) => Waits.WaitUntilVisible(Drivers.driver, locator));

        public static void WaitUntilNotVisible(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilNotVisible), obj, data,
                (locator, _) => Waits.WaitUntilNotVisible(Drivers.driver, locator));

        public static void WaitUntilAllVisible(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilAllVisible), obj, data,
                (locator, _) => Waits.WaitUntilAllVisible(Drivers.driver, locator));

        public static void WaitUntilAllNotVisible(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilAllNotVisible), obj, data,
                (locator, _) => Waits.WaitUntilAllNotVisible(Drivers.driver, locator));

        public static void WaitUntilExistsAndVisible(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilExistsAndVisible), obj, data,
                (locator, _) => Waits.WaitUntilExistsAndVisible(Drivers.driver, locator));

        public static void WaitUntilEditable(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilEditable), obj, data,
                (locator, _) => Waits.WaitUntilEditable(Drivers.driver, locator));

        public static void WaitUntilNotEditable(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilNotEditable), obj, data,
                (locator, _) => Waits.WaitUntilNotEditable(Drivers.driver, locator));

        public static void WaitUntilUrlContains(string obj, string data) =>
            Waits.ExecuteWaitKeyword(nameof(WaitUntilUrlContains), $"URL contains: {data}",
                () => Waits.WaitUntilUrlContains(Drivers.driver, data));

        public static void WaitUntilUrlContainsIgnoreCase(string obj, string data) =>
            Waits.ExecuteWaitKeyword(nameof(WaitUntilUrlContainsIgnoreCase), $"URL contains (ignore case): {data}",
                () => Waits.WaitUntilUrlContainsIgnoreCase(Drivers.driver, data));

        public static void WaitUntilUrlEquals(string obj, string data) =>
            Waits.ExecuteWaitKeyword(nameof(WaitUntilUrlEquals), $"URL equals: {data}",
                () => Waits.WaitUntilUrlEquals(Drivers.driver, data));

        public static void WaitUntilUrlStartsWith(string obj, string data) =>
            Waits.ExecuteWaitKeyword(nameof(WaitUntilUrlStartsWith), $"URL starts with: {data}",
                () => Waits.WaitUntilUrlStartsWith(Drivers.driver, data));

        public static void WaitUntilUrlEndsWith(string obj, string data) =>
            Waits.ExecuteWaitKeyword(nameof(WaitUntilUrlEndsWith), $"URL ends with: {data}",
                () => Waits.WaitUntilUrlEndsWith(Drivers.driver, data));

        public static void WaitUntilUrlMatchesPattern(string obj, string data) =>
            Waits.ExecuteWaitKeyword(nameof(WaitUntilUrlMatchesPattern), $"URL pattern: {data}",
                () => Waits.WaitUntilUrlMatchesPattern(Drivers.driver, data));

        public static void WaitUntilPageLoaded(string obj, string data) =>
            Waits.ExecuteWaitKeyword(nameof(WaitUntilPageLoaded), "Wait for document load",
                () => Waits.WaitUntilPageLoaded(Drivers.driver));

        public static void WaitUntilTextContains(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilTextContains), obj, data,
                (locator, text) => Waits.WaitUntilTextContains(Drivers.driver, locator, text));

        public static void WaitSeconds(string obj, string data) =>
            Waits.ExecuteWaitKeyword(nameof(WaitSeconds), $"Seconds: {data}",
                () => Waits.WaitSeconds(Convert.ToInt32(data)));

        #endregion
    }
}
