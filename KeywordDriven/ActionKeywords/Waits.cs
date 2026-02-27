using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using KeywordDriven.Config;
using KeywordDriven.Utils;
using KeywordDriven.Execution; /////SHOULD REMOVED/////

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
        internal static bool WaitUntilClickable(By by, IWebDriver driver, int? timeoutSeconds = null)
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
                    bool hasSize = element.Size.Width > 0 && element.Size.Height > 0;

                    return (isDisplayed && isEnabled && isNotDisabled && isNotHidden && hasSize)
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
        internal static bool WaitUntilExists(By by, IWebDriver driver, int? timeoutSeconds = null)
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
        internal static bool WaitUntilNotExists(By by, IWebDriver driver, int? timeoutSeconds = null)
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
        internal static bool WaitUntilVisibleByDriver(By by, IWebDriver driver, int? timeoutSeconds = null)
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
        internal static bool WaitUntilVisibleByJs(By by, IWebDriver driver, int? timeoutSeconds = null)
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

                    var visibility = js.ExecuteScript(
                        "return window.getComputedStyle(arguments[0]).visibility;", element)?.ToString();
                    var display = js.ExecuteScript(
                        "return window.getComputedStyle(arguments[0]).display;", element)?.ToString();
                    var opacity = js.ExecuteScript(
                        "return window.getComputedStyle(arguments[0]).opacity;", element)?.ToString();

                    bool isVisibleByCss = visibility != "hidden"
                                       && display != "none"
                                       && opacity != "0";

                    return isVisibleByCss ? true : false;
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
        internal static bool WaitUntilAllVisible(By by, IWebDriver driver, int? timeoutSeconds = null)
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
        internal static bool WaitUntilNotVisibleByDriver(By by, IWebDriver driver, int? timeoutSeconds = null)
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
                    return false; // not in DOM at all — considered invisible
                }
                catch (StaleElementReferenceException)
                {
                    return false; // element was removed from DOM
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
        internal static bool WaitUntilNotVisibleByJs(By by, IWebDriver driver, int? timeoutSeconds = null)
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

                    var visibility = js.ExecuteScript(
                        "return window.getComputedStyle(arguments[0]).visibility;", element)?.ToString();
                    var display = js.ExecuteScript(
                        "return window.getComputedStyle(arguments[0]).display;", element)?.ToString();
                    var opacity = js.ExecuteScript(
                        "return window.getComputedStyle(arguments[0]).opacity;", element)?.ToString();

                    bool isHiddenByCss = visibility == "hidden"
                                      || display == "none"
                                      || opacity == "0";

                    bool isHiddenBySelenium = !element.Displayed;

                    return isHiddenByCss || isHiddenBySelenium;
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
        /// Waits until ALL elements matching the locator are not visible.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        internal static bool WaitUntilAllNotVisible(By by, IWebDriver driver, int? timeoutSeconds = null)
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
        internal static bool WaitUntilExistsAndVisible(By by, IWebDriver driver, int? timeoutSeconds = null)
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
        internal static bool WaitUntilEditable(By by, IWebDriver driver, int? timeoutSeconds = null)
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
        internal static bool WaitUntilNotEditable(By by, IWebDriver driver, int? timeoutSeconds = null)
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

            return wait.Until(d => d.Url.Contains(expectedText));
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
                d.Url.Equals(expectedUrl, StringComparison.OrdinalIgnoreCase)
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
        /// Waits for a fixed number of milliseconds (hard wait).
        /// </summary>
        /// <param name="seconds"></param>
        internal static void WaitSeconds(int seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }
    }

    public partial class Keywords
    {
        #region Keywords methods
        public static void WaitUntilClickable(String obj, String data)
        {
            Log.Info("WaitUntilClickable ..");
            ExtentReporter.NodeInfo("WaitUntilClickable ..");

            try
            {
                By by = Locators.GetLocator(obj);

                if (Waits.WaitUntilClickable(by, Drivers.driver))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilClickable | No such element");
                    ExtentReporter.NodeError("WaitUntilClickable | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }
            }
            catch (Exception e)
            {
                Log.Error("WaitUntilClickable | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilClickable | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilExists(String obj, String data)
        {
            Log.Info("WaitUntilExists ..");
            ExtentReporter.NodeInfo("WaitUntilExists ..");

            try
            {
                By by = Locators.GetLocator(obj);

                if (Waits.WaitUntilExists(by, Drivers.driver))
                {
                    DriverScript.outcome =(int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilExists | No such element");
                    ExtentReporter.NodeError("WaitUntilExists | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }
            }
            catch (Exception e)
            {
                Log.Error("WaitUntilExists | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilExists | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilNotExists(String obj, String data)
        {
            Log.Info("WaitUntilNotExists ..");
            ExtentReporter.NodeInfo("WaitUntilNotExists ..");

            try
            {
                By by = Locators.GetLocator(obj);

                if (Waits.WaitUntilNotExists(by, Drivers.driver))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilNotExists | No such element");
                    ExtentReporter.NodeError("WaitUntilNotExists | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }
            }
            catch (Exception e)
            {
                Log.Error("WaitUntilNotExists | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilNotExists | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilVisible(String obj, String data)
        {
            Log.Info("WaitUntilVisible ..");
            ExtentReporter.NodeInfo("WaitUntilVisible ..");
            try
            {
                By by = Locators.GetLocator(obj);

                if (Waits.WaitUntilVisibleByDriver(by, Drivers.driver))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else if (Waits.WaitUntilVisibleByJs(by, Drivers.driver))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilVisible | No such element");
                    ExtentReporter.NodeError("WaitUntilVisible | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }

            }
            catch (Exception e)
            {
                Log.Error("WaitUntilNotVisible | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilNotVisible | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilAllVisible(String obj, String data)
        {
            Log.Info("WaitUntilAllVisible ..");
            ExtentReporter.NodeInfo("WaitUntilAllVisible ..");
            try
            {
                By by = Locators.GetLocator(obj);

                if (Waits.WaitUntilAllVisible(by, Drivers.driver))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilAllVisible | No such element");
                    ExtentReporter.NodeError("WaitUntilAllVisible | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }

            }
            catch (Exception e)
            {
                Log.Error("WaitUntilAllVisible | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilAllVisible | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilNotVisible(String obj, String data)
        {
            Log.Info("WaitUntilNotVisible ..");
            ExtentReporter.NodeInfo("WaitUntilNotVisible ..");
            try
            {
                By by = Locators.GetLocator(obj);

                if (Waits.WaitUntilNotVisibleByDriver(by, Drivers.driver))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else if(Waits.WaitUntilNotVisibleByJs(by, Drivers.driver))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilNotVisible | No such element");
                    ExtentReporter.NodeError("WaitUntilNotVisible | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }
                
            }
            catch (Exception e)
            {
                Log.Error("WaitUntilNotVisible | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilNotVisible | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilAllNotVisible(String obj, String data)
        {
            Log.Info("WaitUntilAllNotVisible ..");
            ExtentReporter.NodeInfo("WaitUntilAllNotVisible ..");
            try
            {
                By by = Locators.GetLocator(obj);

                if (Waits.WaitUntilAllNotVisible(by, Drivers.driver))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilAllNotVisible | No such element");
                    ExtentReporter.NodeError("WaitUntilAllNotVisible | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }

            }
            catch (Exception e)
            {
                Log.Error("WaitUntilAllNotVisible | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilAllNotVisible | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilExistsAndVisible(String obj, String data)
        {
            Log.Info("WaitUntilExistsAndVisible ..");
            ExtentReporter.NodeInfo("WaitUntilExistsAndVisible ..");

            try
            {
                By by = Locators.GetLocator(obj);

                if (Waits.WaitUntilExistsAndVisible(by, Drivers.driver))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilExistsAndVisible | No such element");
                    ExtentReporter.NodeError("WaitUntilExistsAndVisible | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }
            }
            catch (Exception e)
            {
                Log.Error("WaitUntilExistsAndVisible | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilExistsAndVisible | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilEditable(String obj, String data)
        {
            Log.Info("WaitUntilEditable ..");
            ExtentReporter.NodeInfo("WaitUntilEditable ..");
            try
            {
                By by = Locators.GetLocator(obj);

                if (Waits.WaitUntilEditable(by, Drivers.driver))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilEditable | No such element");
                    ExtentReporter.NodeError("WaitUntilEditable | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }
            }
            catch (Exception e)
            {
                Log.Error("WaitUntilEditable | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilEditable | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilNotEditable(String obj, String data)
        {
            Log.Info("WaitUntilNotEditable ..");
            ExtentReporter.NodeInfo("WaitUntilNotEditable ..");
            try
            {
                By by = Locators.GetLocator(obj);

                if (Waits.WaitUntilNotEditable(by, Drivers.driver))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilNotEditable | No such element");
                    ExtentReporter.NodeError("WaitUntilNotEditable | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }
            }
            catch (Exception e)
            {
                Log.Error("WaitUntilNotEditable | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilNotEditable | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilUrlContains(String obj, String data)
        {
            Log.Info("WaitUntilUrlContains .." + data);
            ExtentReporter.NodeInfo("WaitUntilUrlContains ..");
            try
            {

                if (Waits.WaitUntilUrlContains(Drivers.driver, data))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilUrlContains | No such element");
                    ExtentReporter.NodeError("WaitUntilUrlContains | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }
            }
            catch (Exception e)
            {
                Log.Error("WaitUntilUrlContains | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilUrlContains | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilUrlContainsIgnoreCase(String obj, String data)
        {
            Log.Info("WaitUntilUrlContainsIgnoreCase .." + data);
            ExtentReporter.NodeInfo("WaitUntilUrlContainsIgnoreCase ..");
            try
            {
                if (Waits.WaitUntilUrlContainsIgnoreCase(Drivers.driver, data))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilUrlContainsIgnoreCase | No such element");
                    ExtentReporter.NodeError("WaitUntilUrlContainsIgnoreCase | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }
            }
            catch (Exception e)
            {
                Log.Error("WaitUntilUrlContainsIgnoreCase | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilUrlContainsIgnoreCase | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilUrlEquals(String obj, String data)
        {
            Log.Info("WaitUntilUrlEquals .." + data);
            ExtentReporter.NodeInfo("WaitUntilUrlEquals ..");
            try
            {

                if (Waits.WaitUntilUrlEquals(Drivers.driver, data))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilUrlEquals | No such element");
                    ExtentReporter.NodeError("WaitUntilUrlEquals | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }
            }
            catch (Exception e)
            {
                Log.Error("WaitUntilUrlEquals | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilUrlEquals | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilUrlStartsWith(String obj, String data)
        {
            Log.Info("WaitUntilUrlStartsWith .." + data);
            ExtentReporter.NodeInfo("WaitUntilUrlStartsWith ..");
            try
            {

                if (Waits.WaitUntilUrlStartsWith(Drivers.driver, data))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilUrlStartsWith | No such element");
                    ExtentReporter.NodeError("WaitUntilUrlStartsWith | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }
            }
            catch (Exception e)
            {
                Log.Error("WaitUntilUrlStartsWith | Exception: " + e.Message);
                ExtentReporter.NodeError("WaitUntilUrlStartsWith | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilUrlEndsWith(String obj, String data)
        {
            Log.Info("WaitUntilUrlEndsWith .." + data);
            ExtentReporter.NodeInfo("WaitUntilUrlEndsWith ..");
            try
            {

                if (Waits.WaitUntilUrlEndsWith(Drivers.driver, data))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilUrlEndsWith | No such element");
                    ExtentReporter.NodeError("WaitUntilUrlEndsWith");
                    DriverScript.outcome = (int) Outcome.Error;
                }
            }
            catch (Exception e)
            {
                Log.Error("Not able to WaitUntilUrlEndsWith | Exception: " + e.Message);
                ExtentReporter.NodeError("Not able to WaitUntilUrlEndsWith | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitUntilUrlMatchesPattern(String obj, String data)
        {
            Log.Info("WaitUntilUrlMatchesPattern .." + data);
            ExtentReporter.NodeInfo("WaitUntilUrlMatchesPattern ..");
            try
            {

                if (Waits.WaitUntilUrlMatchesPattern(Drivers.driver, data))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    Log.Error("WaitUntilUrlMatchesPattern | No such element");
                    ExtentReporter.NodeError("WaitUntilUrlMatchesPattern | No such element");
                    DriverScript.outcome = (int) Outcome.Error;
                }
            }
            catch (Exception e)
            {
                Log.Error("Not able to WaitUntilUrlMatchesPattern | Exception: " + e.Message);
                ExtentReporter.NodeError("Not able to WaitUntilUrlMatchesPattern | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        public static void WaitSeconds(String obj, String data)
        {
            Log.Info($"Waiting \"{data}\" seconds");
            ExtentReporter.NodeInfo($"Waiting \"{data}\" seconds");

            try
            {
                Waits.WaitSeconds(Convert.ToInt32(data));

                DriverScript.outcome = (int) Outcome.Pass;
            }
            catch (Exception e)
            {
                Log.Error($"WaitSeconds | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"WaitSeconds | Exception: {e.Message}");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        #endregion
    }
}
