using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using KeywordDriven.Config;
using KeywordDriven.Utils;
using KeywordDriven.Execution;

namespace KeywordDriven.ActionKeywords
{
    internal static class Waits
    {
        /// <summary>
        /// Use provided timeout, otherwise fall back to global default
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        internal static WebDriverWait Create(IWebDriver driver, int? timeoutSeconds = null)
        {
            int timeout = timeoutSeconds ?? DriverSetting.DefaultTimeoutSeconds;

            return new WebDriverWait(driver, TimeSpan.FromSeconds(timeout))
            {
                PollingInterval = TimeSpan.FromMilliseconds(500)
            };
        }

        /// <summary>
        /// Waits until the element is visible, enabled, and ready to be clicked.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        internal static bool WaitUntilClickable(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException),
                typeof(ElementNotInteractableException)
            );

            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(by);

                    bool isDisplayed = element.Displayed;
                    bool isEnabled = element.Enabled;
                    bool isNotDisabled = element.GetAttribute("disabled") == null;
                    bool isNotHidden = element.GetAttribute("aria-hidden") != "true";
                    bool isNotAriaDisabled = element.GetAttribute("aria-disabled") != "true";
                    bool hasSize = element.Size.Width > 0 && element.Size.Height > 0;

                    return (isDisplayed && isEnabled && isNotDisabled && isNotHidden
                        && isNotAriaDisabled && hasSize)
                        ? true
                        : false; // not ready yet — retry
                }
                catch (NoSuchElementException)
                {
                    return false; // not in DOM yet — retry
                }
                catch (StaleElementReferenceException)
                {
                    return false; // DOM changed — retry
                }
                catch (ElementNotInteractableException)
                {
                    return false; // element exists but not interactable yet — retry
                }
            });
        }

        /// <summary>
        /// Waits until the element exists in the DOM (regardless of visibility).
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        internal static bool WaitUntilExists(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
            );

            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(by);
                    return true; // found in DOM 
                }
                catch (NoSuchElementException)
                {
                    return false; // not yet in DOM 
                }
                catch (StaleElementReferenceException)
                {
                    return false; // DOM changed
                }
            });
        }

        /// <summary>
        /// Waits until the element does NOT exist in the DOM (completely removed).
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        internal static bool WaitUntilNotExists(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
            );

            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(by);
                    return false; // still in DOM — retry
                }
                catch (NoSuchElementException)
                {
                    return true; // not in DOM — condition met
                }
                catch (StaleElementReferenceException)
                {
                    return true; // element was removed from DOM — condition met
                }
            });
        }

        /// <summary>
        /// Waits until the element is visible (displayed) in the DOM.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        internal static bool WaitUntilVisibleByDriver(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
            );

            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(by);
                    return element.Displayed ? true : false; // visible — return it, hidden — retry
                }
                catch (NoSuchElementException) { return false; } // not in DOM yet — retry
                catch (StaleElementReferenceException) { return false; } // DOM changed — retry
            });
        }

        /// <summary>
        /// Waits until element is visible using JavaScript computed style.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        internal static bool WaitUntilVisibleByJs(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);
            var js = (IJavaScriptExecutor)driver;

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
            );

            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(by);
                    return element.Displayed
                        && (bool)js.ExecuteScript(
                            "const style = window.getComputedStyle(arguments[0]);" +
                            "return style.visibility !== 'hidden' && style.display !== 'none' && Number(style.opacity) > 0;",
                            element);
                }
                catch (NoSuchElementException) { return false; }
                catch (StaleElementReferenceException) { return false; }
            });
        }

        /// <summary>
        /// Waits until ALL elements matching the locator are visible.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        internal static bool WaitUntilAllVisible(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
            );

            return wait.Until(d =>
            {
                try
                {
                    var elements = d.FindElements(by);
                    return elements.Count > 0 && elements.All(e => e.Displayed)
                        ? true
                        : false; // not all visible yet — retry
                }
                catch (StaleElementReferenceException) { return false; }
            });
        }
        /// <summary>
        /// Waits until the element is not visible (hidden).
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        internal static bool WaitUntilNotVisibleByDriver(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(by);
                    return !element.Displayed; // visible in DOM but hidden
                }
                catch (NoSuchElementException)
                {
                    return true; // absent elements are not visible
                }
                catch (StaleElementReferenceException)
                {
                    return true; // element was removed from DOM
                }
            });
        }

        /// <summary>
        /// Waits until the element is invisible using CSS visibility/opacity via JavaScript.
        /// Useful for elements that use CSS transitions or opacity to hide.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        internal static bool WaitUntilNotVisibleByJs(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);
            var js = (IJavaScriptExecutor)driver;

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
            );

            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(by);
                    return !element.Displayed
                        || (bool)js.ExecuteScript(
                            "const style = window.getComputedStyle(arguments[0]);" +
                            "return style.visibility === 'hidden' || style.display === 'none' || Number(style.opacity) <= 0;",
                            element);
                }
                catch (NoSuchElementException)
                {
                    return true;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Waits until ALL elements matching the locator are not visible.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        internal static bool WaitUntilAllNotVisible(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
            );

            return wait.Until(d =>
            {
                try
                {
                    var elements = d.FindElements(by);
                    return elements.Count == 0 || elements.All(e => !e.Displayed);
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Waits until the element exists AND is visible in the DOM.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        internal static bool WaitUntilExistsAndVisible(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
            );

            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(by);
                    return element.Displayed ? true : false; // must be visible too
                }
                catch (NoSuchElementException)
                {
                    return false; // not yet in DOM — retry
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Waits until the element is visible, enabled, and not read-only.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        internal static bool WaitUntilEditable(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

            return wait.Until(d =>
            {
                var element = d.FindElement(by);

                bool isDisplayed = element.Displayed;
                bool isEnabled = element.Enabled;
                bool isNotReadOnly = element.GetAttribute("readonly") == null
                                  && element.GetAttribute("aria-readonly") != "true";
                bool isNotDisabled = element.GetAttribute("disabled") == null;

                return isDisplayed && isEnabled && isNotReadOnly && isNotDisabled;
            });
        }

        /// <summary>
        /// Waits until the element is NOT editable (disabled, readonly, hidden).
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        internal static bool WaitUntilNotEditable(IWebDriver driver, By by, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(by);

                    bool isHidden = !element.Displayed;
                    bool isDisabled = !element.Enabled
                                      || element.GetAttribute("disabled") != null;
                    bool isReadOnly = element.GetAttribute("readonly") != null
                                      || element.GetAttribute("aria-readonly") == "true";

                    return isHidden || isDisabled || isReadOnly;
                }
                catch (NoSuchElementException)
                {
                    // Element removed from DOM
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    return false; //retry
                }
            });
        }

        /// <summary>
        /// Waits until the current URL contains the expected text (case-sensitive).
        /// Returns true when condition is met.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="expectedText"></param>
        /// <returns></returns>
        internal static bool WaitUntilUrlContains(IWebDriver driver, string expectedText, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            return wait.Until(d => d.Url.Contains(expectedText, StringComparison.Ordinal));
        }

        /// <summary>
        /// Waits until the current URL contains the expected text (ignore case-insensitive).
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="expectedText"></param>
        /// <returns></returns>
        internal static bool WaitUntilUrlContainsIgnoreCase(IWebDriver driver, string expectedText, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            return wait.Until(d =>
                d.Url.Contains(expectedText, StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// Waits until the current URL exactly matches the expected URL.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="expectedUrl"></param>
        /// <returns></returns>
        internal static bool WaitUntilUrlEquals(IWebDriver driver, string expectedUrl, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            return wait.Until(d =>
                d.Url.Equals(expectedUrl, StringComparison.Ordinal)
            );
        }

        /// <summary>
        /// Waits until the current URL starts with the expected text.
        /// Useful for checking base URLs or route prefixes.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="expectedStart"></param>
        /// <returns></returns>
        internal static bool WaitUntilUrlStartsWith(IWebDriver driver, string expectedStart, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            return wait.Until(d =>
                d.Url.StartsWith(expectedStart, StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// Waits until the current URL ends with the expected text.
        /// Useful for checking page slugs or file extensions.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="expectedEnd"></param>
        /// <returns></returns>
        internal static bool WaitUntilUrlEndsWith(IWebDriver driver, string expectedEnd, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);

            return wait.Until(d =>
                d.Url.EndsWith(expectedEnd, StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// Waits until the current URL matches a Regex pattern.
        /// Useful for dynamic URLs with IDs or tokens.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="regexPattern"></param>
        /// <returns></returns>
        internal static bool WaitUntilUrlMatchesPattern(IWebDriver driver, string regexPattern, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            return wait.Until(d => regex.IsMatch(d.Url));
        }

        /// <summary>
        /// Waits until the browser reports that the document has finished loading.
        /// </summary>
        internal static bool WaitUntilPageLoaded(IWebDriver driver, int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);
            var js = (IJavaScriptExecutor)driver;

            return wait.Until(d => js.ExecuteScript("return document.readyState;")?.ToString() == "complete");
        }

        /// <summary>
        /// Waits until the located element's rendered text contains the expected text.
        /// </summary>
        internal static bool WaitUntilTextContains(
            IWebDriver driver,
            By by,
            string expectedText,
            int? timeoutSeconds = null)
        {
            var wait = Waits.Create(driver, timeoutSeconds);
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

        /// <summary>
        /// Waits for a fixed number of milliseconds (hard wait).
        /// </summary>
        /// <param name="seconds"></param>
        internal static void WaitSeconds(int seconds)
        {
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
                (locator, _) => Waits.WaitUntilVisibleByJs(Drivers.driver, locator));

        public static void WaitUntilAllVisible(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilAllVisible), obj, data,
                (locator, _) => Waits.WaitUntilAllVisible(Drivers.driver, locator));

        public static void WaitUntilNotVisible(string obj, string data) =>
            Locators.ExecuteLocatorKeyword(nameof(WaitUntilNotVisible), obj, data,
                (locator, _) => Waits.WaitUntilNotVisibleByJs(Drivers.driver, locator));

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
