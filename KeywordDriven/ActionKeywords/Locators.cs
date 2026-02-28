using KeywordDriven.Config;
using KeywordDriven.Execution;
using KeywordDriven.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Modules.BrowsingContext;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Reflection;
using System.Xml.Linq;

namespace KeywordDriven.ActionKeywords
{
    internal partial class Locators
    {
        // <summary>
        /// Maps locator type with locator value and return locator By object.
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static By GetLocator(string obj)
        {
            string[] pageObject = obj.Split('_');

            string locatortype = pageObject[1];
            string locatorvalue = ExcelManager.GetKeyValue(obj, ExcelSetting.Col_Locators_PageObject, ExcelSetting.Sheet_Locators);
            
            By locator = locatortype.Trim().ToLower() switch
            {
                "xpath" => By.XPath(locatorvalue),
                "id" => By.Id(locatorvalue),
                "csslocator" => By.CssSelector(locatorvalue),
                "classname" => By.ClassName(locatorvalue),
                "tagname" => By.TagName(locatorvalue),
                "linktext" => By.LinkText(locatorvalue),
                "name" => By.Name(locatorvalue),
                "partiallinktext" => By.PartialLinkText(locatorvalue),
                _ => throw new Exception($"Unsupported locator type:{locatortype.Trim().ToLower()}, Valid types: id, name, xpath, css, classname, tagname, linktext, partiallinktext"),
            };
            return locator;
        }

        /// <summary>
        /// Standard click Element.
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        internal static bool ClickByDriver(By locator)
        {
            try
            {
                Drivers.driver.FindElement(locator).Click();
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name}| Exception: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clicks element using JavaScript.
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        internal static bool ClickByJavascript(By locator)
        {
            try
            {
                var js = (IJavaScriptExecutor)Drivers.driver;
                js.ExecuteScript("arguments[0].click();", Drivers.driver.FindElement(locator));
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clicks element using Actions (move to element then click).
        /// Use for custom dropdowns, menus, tooltips, or canvas elements.
        /// </summary>
        /// <param name="by"></param>
        internal static void ClickByActions(By locator)
        {
            try
            {
                new Actions(Drivers.driver)
                    .MoveToElement(Drivers.driver.FindElement(locator))
                    .Click()
                    .Perform();
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
            }
        }

        internal static void ClickByOffset(By locator)
        {

        }
        /// <summary>
        /// Double clicks element using Actions.
        /// Use for elements that require double click to trigger.
        /// </summary>
        internal static void DoubleClick(By locator)
        {
            try
            {
                new Actions(Drivers.driver)
                    .DoubleClick(Drivers.driver.FindElement(locator))
                    .Perform();

            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
            }
        }

        /// <summary>
        /// Right clicks element using Actions.
        /// Use to open context menus.
        /// </summary>
        internal static void RightClick(By locator)
        {
            try
            {
                new Actions(Drivers.driver)
                    .ContextClick(Drivers.driver.FindElement(locator))
                    .Perform();
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
            }
        }

        internal static bool InputByDriver(By locator, String data)
        {
            try
            {
                Drivers.driver.FindElement(locator).SendKeys(data);
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool InputByJavascript(By locator, String data)
        {
            try
            {
                IJavaScriptExecutor jse = (IJavaScriptExecutor)Drivers.driver;
                jse.ExecuteScript("arguments[0].value='" + data + "';", Drivers.driver.FindElement(locator));
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool SelectTextByDriver(By locator, string data)
        {
            try
            {
                new SelectElement(Drivers.driver.FindElement(locator)).SelectByText(data);
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool SelectValueByDriver(By locator, string data)
        {
            try
            {
                new SelectElement(Drivers.driver.FindElement(locator)).SelectByValue(data);
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool ClearByDriver(By locator)
        {
            try
            {
                Drivers.driver.FindElement(locator).Clear();
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool ClearByJavascript(By locator)
        {
            try
            {
                IJavaScriptExecutor jse = (IJavaScriptExecutor)Drivers.driver;
                jse.ExecuteScript("arguments[0].value = '';", Drivers.driver.FindElement(locator));
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                return false;
            }
        }

    }
    
    public partial class Keywords
    {

        #region Keywords methods

        public static void Click(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name} \"{obj}\"");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} \"{obj}\"");

            try
            {
                By locator = Locators.GetLocator(obj);

                Waits.WaitUntilClickable(locator, Drivers.driver);

                ((IJavaScriptExecutor)Drivers.driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", locator);

                if (Locators.ClickByDriver(locator))
                {
                    DriverScript.outcome = (int)Outcome.Pass;
                }
                else if (Locators.ClickByJavascript(locator))
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                else
                {
                    DriverScript.outcome = (int)Outcome.Error;
                }
                
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeError($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.AddScreenShot("");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void TypeText(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name} \"{data}\" to \"{obj}\"");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} \"{data}\" to \"{obj}\"");
            try
            {
                By locator = Locators.GetLocator(obj);

                Waits.WaitUntilClickable(locator, Drivers.driver);

                if (!Locators.InputByDriver(locator, data))
                {
                    if (!Locators.InputByJavascript(locator, data))
                    {
                        DriverScript.outcome = (int) Outcome.Error;
                    }
                    else
                    {
                        DriverScript.outcome = (int) Outcome.Pass;
                    }
                }
                else
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
                
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeError($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void AppendText(String obj, String data)
        { }
        
        public static void ClearText(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name} \"{obj}\"");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} \"{obj}\"");

            try
            {
                By locator = Locators.GetLocator(obj);

                Waits.WaitUntilClickable(locator, Drivers.driver);
                WaitSeconds("", "2");

                if (!Locators.ClearByDriver(locator))
                {
                    if (!Locators.ClearByJavascript(locator))
                    {
                        DriverScript.outcome = (int)Outcome.Error;
                    }
                    else
                    {
                        DriverScript.outcome = (int)Outcome.Pass;
                    }
                }
                else
                {
                    DriverScript.outcome = (int)Outcome.Pass;
                }

            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeError($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.AddScreenShot("");
                DriverScript.outcome = (int)Outcome.Error;
            }
        }
        
        public static void SelectByText(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name} from \"{obj}\"");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} from \"{obj}\"");

            try
            {
                By locator = Locators.GetLocator(obj);

                Waits.WaitUntilExists(locator, Drivers.driver);

                if (!Locators.SelectTextByDriver(locator, data))
                {
                    if (!Locators.SelectValueByDriver(locator, data))
                    {
                        DriverScript.outcome = (int) Outcome.Error;
                    }
                    else
                    {
                        DriverScript.outcome = (int) Outcome.Pass;
                    }
                }
                else
                {
                    DriverScript.outcome = (int) Outcome.Pass;
                }
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeError($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void SelectByValue(String obj, String data)
        { }

        public static void SelectByIndex(String obj, String data)
        { }

        public static void CheckCheckbox(String obj, String data)
        { }

        public static void SelectRadioButton(String obj, String data)
        { }

        public static void PressKey(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name} \"{data}\" in \"{obj}\"");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} \"{data}\" in \"{obj}\"");
            try
            {
                By locator = Locators.GetLocator(obj);
                switch (data.ToLower().Trim())
                {
                    case "enter":
                        Drivers.driver.FindElement(locator).SendKeys(Keys.Enter);
                        DriverScript.outcome = (int) Outcome.Pass;
                        break;
                    case "return":
                        Drivers.driver.FindElement(locator).SendKeys(Keys.Return);
                        DriverScript.outcome = (int) Outcome.Pass;
                        break;
                    case "tab":
                        Drivers.driver.FindElement(locator).SendKeys(Keys.Tab);
                        DriverScript.outcome = (int) Outcome.Pass;
                        break;
                    default:
                        Drivers.driver.FindElement(locator).SendKeys(Keys.Enter);
                        DriverScript.outcome = (int)Outcome.Pass;
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeError($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void UploadFiles(String obj, String data)
        { }

        #endregion
    }
}
