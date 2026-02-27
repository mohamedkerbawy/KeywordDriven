using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using KeywordDriven.Config;
using KeywordDriven.Utils;
using KeywordDriven.Execution;   /////SHOULD REMOVED/////

namespace KeywordDriven.ActionKeywords
{
    internal partial class Locators
    {
        internal static By GetLocator(string obj)
        {
            string[] locator = obj.Split('_');

            string locatortype = locator[1];
            string locatorvalue = GetLocatorValue(obj);

            By by;
            switch (locatortype)
            {
                case "xpath":
                    by = By.XPath(locatorvalue);
                    break;
                case "id":
                    by = By.Id(locatorvalue);
                    break;
                case "csslocator":
                    by = By.CssSelector(locatorvalue);
                    break;
                case "classname":
                    by = By.ClassName(locatorvalue);
                    break;
                case "linktext":
                    by = By.LinkText(locatorvalue);
                    break;
                case "name":
                    by = By.Name(locatorvalue);
                    break;
                case "partiallinktext":
                    by = By.PartialLinkText(locatorvalue);
                    break;
                default:
                    by = null;
                    break;
            }
            return by;
        }
        
        internal static string GetLocatorValue(String obj)
        {
            return ExcelManager.GetKeyValue(obj, ExcelSetting.Col_Locators_PageObject, ExcelSetting.Sheet_Locators);
        }

        internal static bool ClickByDriver(By by)
        {
            try
            {
                Log.Info("ClickByDriver ..");
                ExtentReporter.NodeInfo("ClickByDriver ..");

                Drivers.driver.FindElement(by).Click();
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to ClickByDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to ClickByDriver | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool ClickByJavascript(By by)
        {
            try
            {
                Log.Info("ClickByJavascript ..");
                ExtentReporter.NodeInfo("ClickByJavascript ..");

                IJavaScriptExecutor jse = (IJavaScriptExecutor)Drivers.driver;
                jse.ExecuteScript("arguments[arguments.length - 1].click();", Drivers.driver.FindElement(by));
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to ClickByJavascript | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to ClickByJavascript | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool InputByDriver(By by, String data)
        {
            try
            {
                Log.Info("InputByDriver ..");
                ExtentReporter.NodeInfo("InputByDriver ..");

                Drivers.driver.FindElement(by).SendKeys(data);
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to InputByDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to InputByDriver | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool InputByJavascript(By by, String data)
        {
            try
            {
                Log.Info("InputByJavascript ..");
                ExtentReporter.NodeInfo("InputByJavascript ..");

                IJavaScriptExecutor jse = (IJavaScriptExecutor)Drivers.driver;
                jse.ExecuteScript("arguments[0].value='" + data + "';", Drivers.driver.FindElement(by));
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to InputByJavascript | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to InputByJavascript | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool SelectTextByDriver(By by, string data)
        {
            try
            {
                Log.Info("SelectTextByDriver ..");
                ExtentReporter.NodeInfo("SelectTextByDriver ..");

                new SelectElement(Drivers.driver.FindElement(by)).SelectByText(data);
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to SelectTextByDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to SelectTextByDriver | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool SelectValueByDriver(By by, string data)
        {
            try
            {
                Log.Info("SelectValueByDriver ..");
                ExtentReporter.NodeInfo("SelectValueByDriver ..");

                new SelectElement(Drivers.driver.FindElement(by)).SelectByValue(data);
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to SelectValueByDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to SelectValueByDriver | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool ClearByDriver(By by)
        {
            try
            {
                Log.Info("ClearByDriver ..");
                ExtentReporter.NodeInfo("ClearByDriver ..");

                Drivers.driver.FindElement(by).Clear();
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to ClearByDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to ClearByDriver | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool ClearByJavascript(By by)
        {
            try
            {
                Log.Info("ClearByJavascript ..");
                ExtentReporter.NodeInfo("ClearByJavascript ..");

                IJavaScriptExecutor jse = (IJavaScriptExecutor)Drivers.driver;
                jse.ExecuteScript("arguments[0].value = '';", Drivers.driver.FindElement(by));
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to ClearByJavascript | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to ClearByJavascript | Exception: {e.Message}");
                return false;
            }
        }

    }
    
    public partial class Keywords
    {

        #region Keywords methods
        public static void Click(String obj, String data)
        {
            Log.Info($"Clicking on Element \"{obj}\"");
            ExtentReporter.NodeInfo($"Clicking on Element \"{obj}\"");

            try
            {
                By by = Locators.GetLocator(obj);

                Waits.WaitUntilClickable(by, Drivers.driver);
                WaitSeconds("", "2");

                if (!Locators.ClickByDriver(by))
                {
                    if (!Locators.ClickByJavascript(by))
                    {
                        Log.Error("Not able to ClickByDriver or ClickByJavascript");
                        ExtentReporter.NodeError("Not able to ClickByDriver or ClickByJavascript");
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
                Log.Error($"Not able to Click | Exception: {e.Message}");
                ExtentReporter.NodeError($"Not able to Click | Exception: {e.Message}");
                ExtentReporter.AddScreenShot("");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void Input(String obj, String data)
        {
            Log.Info($"Typing \"{data}\" in Element \"{obj}\"");
            ExtentReporter.NodeInfo($"Typing \"{data}\" in Element \"{obj}\"");
            try
            {
                By by = Locators.GetLocator(obj);

                Waits.WaitUntilClickable(by, Drivers.driver);
                WaitSeconds("", "2");

                if (!Locators.InputByDriver(by, data))
                {
                    if (!Locators.InputByJavascript(by, data))
                    {
                        Log.Error("Not able to InputByDriver or InputByJavascript");
                        ExtentReporter.NodeError("Not able to InputByDriver or InputByJavascript");
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
                Log.Error($"Not able to Input | Exception: {e.Message}");
                ExtentReporter.NodeError($"Not able to Input | Exception: {e.Message}");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void Select(String obj, String data)
        {
            Log.Info($"Selecting from dropdown Element \"{obj}\"");
            ExtentReporter.NodeInfo($"Selecting from dropdown Element \"{obj}\"");

            try
            {
                By by = Locators.GetLocator(obj);

                Waits.WaitUntilExists(by, Drivers.driver);
                if (!Locators.SelectTextByDriver(by, data))
                {
                    if (!Locators.SelectValueByDriver(by, data))
                    {
                        Log.Error("Not able to SelectTextByDriver or SelectValueByDriver");
                        ExtentReporter.NodeError("Not able to SelectTextByDriver or SelectValueByDriver");
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
                Log.Error($"Not able to Select | Exception: {e.Message}");
                ExtentReporter.NodeError($"Not able to Select | Exception: {e.Message}");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void KeyPress(String obj, String data)
        {
            Log.Info($"KeyPress \"{data}\" on \"{obj}\"");
            ExtentReporter.NodeInfo($"KeyPress \"{data}\" on \"{obj}\"");
            try
            {
                By by = Locators.GetLocator(obj);
                switch (data.ToLower().Trim())
                {
                    case "enter":
                        Drivers.driver.FindElement(by).SendKeys(Keys.Enter);
                        DriverScript.outcome = (int) Outcome.Pass;
                        break;
                    case "return":
                        Drivers.driver.FindElement(by).SendKeys(Keys.Return);
                        DriverScript.outcome = (int) Outcome.Pass;
                        break;
                    case "tab":
                        Drivers.driver.FindElement(by).SendKeys(Keys.Tab);
                        DriverScript.outcome = (int) Outcome.Pass;
                        break;
                    default:
                        Log.Error("Not a key");
                        ExtentReporter.NodeError("Not a key");
                        DriverScript.outcome = (int) Outcome.Error;
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error("Not able to KeyPress " + data + " | Exception: " + e.Message);
                ExtentReporter.NodeError("Not able to KeyPress " + data + " | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        
        public static void Clear(String obj, String data)
        {
            Log.Info($"Clearing an Element \"{obj}\"");
            ExtentReporter.NodeInfo($"Clearing an Element \"{obj}\"");

            try
            {
                By by = Locators.GetLocator(obj);

                Waits.WaitUntilClickable(by, Drivers.driver);
                WaitSeconds("", "2");

                if (!Locators.ClearByDriver(by))
                {
                    if (!Locators.ClearByJavascript(by))
                    {
                        Log.Error("Not able to ClearByDriver or ClearByJavascript");
                        ExtentReporter.NodeError("Not able to ClearByDriver or ClearByJavascript");
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
                Log.Error($"Not able to Clear| Exception: {e.Message}");
                ExtentReporter.NodeError($"Not able to Clear | Exception: {e.Message}");
                ExtentReporter.AddScreenShot("");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }      

        #endregion
    }
}
