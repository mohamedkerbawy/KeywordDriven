using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using KeywordDriven.Utils;
using KeywordDriven.Config;
using KeywordDriven.Execution;

namespace KeywordDriven.ActionKeywords
{
    internal partial class Keywords
    {       
        private static By LocateValue(string locatortype, string value)
        {
            By by;
            switch (locatortype)
            {
                case "xpath":
                    by = By.XPath(value);
                    break;
                case "id":
                    by = By.Id(value);
                    break;
                case "csslocator":
                    by = By.CssSelector(value);
                    break;
                case "classname":
                    by = By.ClassName(value);
                    break;
                case "linktext":
                    by = By.LinkText(value);
                    break;
                case "name":
                    by = By.Name(value);
                    break;
                case "partiallinktext":
                    by = By.PartialLinkText(value);
                    break;
                default:
                    by = null;
                    break;
            }
            return by;
        }

        private static string GetKey(String obj)
        {
           return ExcelManager.GetKeyValue(obj, ExcelSetting.Col_Locators_PageObject, ExcelSetting.Sheet_Locators);
        }

        private static bool IsElementPresent(By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException e)
            {
                Log.Info($"Not able to find element by IsElementPresent | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to find element by IsElementPresent | Exception: {e.Message}");
                return false;
            }
        }

        private static bool ClickByDriver(By by)
        {
            try
            {
                Log.Info("ClickByDriver ..");
                ExtentReporter.NodeInfo("ClickByDriver ..");

                driver.FindElement(by).Click();
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to ClickByDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to ClickByDriver | Exception: {e.Message}");
                return false;
            }
        }

        private static bool ClickByJavascript(By by)
        {
            try
            {
                Log.Info("ClickByJavascript ..");
                ExtentReporter.NodeInfo("ClickByJavascript ..");

                IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
                jse.ExecuteScript("arguments[arguments.length - 1].click();", driver.FindElement(by));
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to ClickByJavascript | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to ClickByJavascript | Exception: {e.Message}");
                return false;
            }
        }

        private static bool ClickByappiumDriver(By by)
        {
            try
            {
                Log.Info("ClickByappiumDriver ..");
                ExtentReporter.NodeInfo("ClickByappiumDriver ..");

                appiumdriver.FindElement(by).Click();
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to ClickByappiumDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to ClickByappiumDriver | Exception: {e.Message}");
                return false;
            }
        }

        private static bool InputByDriver(By by, String data)
        {
            try
            {
                Log.Info("InputByDriver ..");
                ExtentReporter.NodeInfo("InputByDriver ..");

                driver.FindElement(by).SendKeys(data);
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to InputByDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to InputByDriver | Exception: {e.Message}");
                return false;
            }
        }

        private static bool InputByJavascript(By by, String data)
        {
            try
            {
                Log.Info("InputByJavascript ..");
                ExtentReporter.NodeInfo("InputByJavascript ..");

                IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
                jse.ExecuteScript("arguments[0].value='" + data + "';", driver.FindElement(by));
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to InputByJavascript | Exception: { e.Message}");
                ExtentReporter.NodeInfo($"Not able to InputByJavascript | Exception: {e.Message}");
                return false;
            }
        }

        private static bool InputByappiumDriver(By by, String data)
        {
            try
            {
                Log.Info("InputByappiumDriver ..");
                ExtentReporter.NodeInfo("InputByappiumDriver ..");

                appiumdriver.FindElement(by).SendKeys(data);
                return true;

            }
            catch (Exception e)
            {
                Log.Info($"Not able to InputByappiumDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to InputByappiumDriver | Exception: {e.Message}");
                return false;
            }
        }
        
        private static bool SelectTextByDriver(By by, string data)
        {
            try
            {
                Log.Info("SelectTextByDriver ..");
                ExtentReporter.NodeInfo("SelectTextByDriver ..");

                new SelectElement(driver.FindElement(by)).SelectByText(data);
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to SelectTextByDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to SelectTextByDriver | Exception: {e.Message}");
                return false;
            }
        }

        private static bool SelectValueByDriver(By by, string data)
        {
            try
            {
                Log.Info("SelectValueByDriver ..");
                ExtentReporter.NodeInfo("SelectValueByDriver ..");

                new SelectElement(driver.FindElement(by)).SelectByValue(data);
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to SelectValueByDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to SelectValueByDriver | Exception: {e.Message}");
                return false;
            }
        }

        private static bool SelectTextByappiumDriver(By by, string data)
        {
            try
            {
                Log.Info("SelectTextByappiumDriver ..");
                ExtentReporter.NodeInfo("SelectTextByappiumDriver ..");

                new SelectElement(appiumdriver.FindElement(by)).SelectByText(data);
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to SelectTextByappiumDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to SelectTextByappiumDriver | Exception: {e.Message}");
                return false;
            }
        }

        private static bool SelectValueByappiumDriver(By by, string data)
        {
            try
            {
                Log.Info("SelectValueByappiumDriver ..");
                ExtentReporter.NodeInfo("SelectValueByappiumDriver ..");

                new SelectElement(appiumdriver.FindElement(by)).SelectByValue(data);
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"Not able to SelectValueByappiumDriver | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to SelectValueByappiumDriver | Exception: {e.Message}");
                return false;
            }
        }

        #region Public methods
        public static void Click(String obj, String data)
        {
            Log.Info($"Clicking on Element \"{obj}\"");
            ExtentReporter.NodeInfo($"Clicking on Element \"{obj}\"");

            try
            {
                string[] locator = obj.Split('_');
                By by = LocateValue(locator[1], GetKey(obj));

                if (locator[0] == "Mobile")
                {
                    WaitUntilVisible(by, appiumdriver);
                    WaitSeconds("", "2");

                    if (!ClickByappiumDriver(by))
                    {
                        Log.Error("Failed ClickByappiumDriver");
                        ExtentReporter.NodeError("Failed ClickByappiumDriver");
                        DriverScript.iOutcome = 3;
                    }
                    else
                    {
                        DriverScript.iOutcome = 1;
                    }    
                }
                else
                {
                    WaitUntilClickable(by, driver);
                    WaitSeconds("", "2");

                    if (!ClickByDriver(by))
                    {
                        if (!ClickByJavascript(by))
                        {
                            Log.Error("Failed ClickByDriver and ClickByJavascript");
                            ExtentReporter.NodeError("Failed ClickByDriver and ClickByJavascript");
                            DriverScript.iOutcome = 3;
                        }
                        else
                        {
                            DriverScript.iOutcome = 1;
                        }
                    }
                    else
                    {
                        DriverScript.iOutcome = 1;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed Click | Exception: {e.Message}");
                ExtentReporter.NodeError($"Failed Click | Exception: {e.Message}");
                ExtentReporter.AddScreenShot("");
                DriverScript.iOutcome = 3;
            }
        }

        public static void Input(String obj, String data)
        {
            Log.Info($"Typing \"{data}\" in Element \"{obj}\"");
            ExtentReporter.NodeInfo($"Typing \"{data}\" in Element \"{obj}\"");
            try
            {
                string[] locator = obj.Split('_');
                By by = LocateValue(locator[1], GetKey(obj));

                if (locator[0] == "Mobile")
                {
                    WaitUntilVisible(by, appiumdriver);
                    WaitSeconds("", "2");

                    if (!InputByappiumDriver(by, data))
                    {
                        Log.Error("Failed InputByappiumDriver");
                        ExtentReporter.NodeError("Failed InputByappiumDriver");
                        DriverScript.iOutcome = 3;
                    }
                    else
                    {
                        DriverScript.iOutcome = 1;
                    }
                }
                else
                {
                    WaitUntilClickable(by, driver);
                    WaitSeconds("", "2");

                    if (!InputByDriver(by, data))
                    {
                        if (!InputByJavascript(by, data))
                        {
                            Log.Error("Failed InputByDriver and InputByJavascript");
                            ExtentReporter.NodeError("Failed InputByDriver and InputByJavascript");
                            DriverScript.iOutcome = 3;
                        }
                        else
                        {
                            DriverScript.iOutcome = 1;
                        }
                    }
                    else
                    {
                        DriverScript.iOutcome = 1;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed Input | Exception: {e.Message}");
                ExtentReporter.NodeError($"Failed Input | Exception: {e.Message}");
                DriverScript.iOutcome = 3;
            }
        }

        public static void Select(String obj, String data)
        {
            Log.Info($"Selecting from dropdown Element \"{obj}\"");
            ExtentReporter.NodeInfo($"Selecting from dropdown Element \"{obj}\"");

            try
            {
                string[] locator = obj.Split('_');
                By by = LocateValue(locator[1], GetKey(obj));

                if (locator[0] == "Mobile")
                {
                    WaitUntilExists(by, appiumdriver);
                    if (!SelectTextByappiumDriver(by, data))
                    {
                        if (!SelectValueByappiumDriver(by, data))
                        {
                            Log.Error("Failed SelectTextByappiumDriver and SelectValueByappiumDriver");
                            ExtentReporter.NodeError("Failed SelectTextByappiumDriver and SelectValueByappiumDriver");
                            DriverScript.iOutcome = 3;
                        }
                        else
                        {
                            DriverScript.iOutcome = 1;
                        }
                    }
                    else
                    {
                        DriverScript.iOutcome = 1;
                    }
                }
                else
                {
                    WaitUntilExists(by, driver);
                    if (!SelectTextByDriver(by, data))
                    {
                        if (!SelectValueByDriver(by, data))
                        {
                            Log.Error("Failed SelectTextByDriver and SelectValueByDriver");
                            ExtentReporter.NodeError("Failed SelectTextByDriver and SelectValueByDriver");
                            DriverScript.iOutcome = 3;
                        }
                        else
                        {
                            DriverScript.iOutcome = 1;
                        }
                    }
                    else
                    {
                        DriverScript.iOutcome = 1;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed Select | Exception: {e.Message}");
                ExtentReporter.NodeError($"Failed Select | Exception: {e.Message}");
                DriverScript.iOutcome = 3;
            }
        }

        public static void DragDrop(String obj, String data)
        {
            Log.Info("Draging Webelement " + obj);
            ExtentReporter.NodeInfo("Draging Webelement " + obj);
            try
            {
                string[] locator1 = obj.Split('_');
                string[] locator2value = data.Split('_');

                By byDragElement = LocateValue(locator1[1], GetKey(obj));
                By byDropValue = LocateValue(locator2value[1], GetKey(data));

                IWebElement source;
                IWebElement target;

                WaitUntilExists(byDragElement, driver);
                if (IsElementPresent(byDragElement))
                {
                    source = driver.FindElement(byDragElement);
                }
                else
                {
                    source = driver.FindElement(byDragElement);
                }

                WaitUntilExists(byDropValue, driver);
                if (IsElementPresent(byDragElement))
                {
                    target = driver.FindElement(byDropValue);
                }
                else
                {
                    target = driver.FindElement(byDropValue);
                }

                IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
                WaitSeconds("", "3");

                Actions actions = new Actions(driver);
                actions.ClickAndHold(source).Perform();
                actions.MoveByOffset(target.Location.X - source.Location.X, target.Location.Y - source.Location.Y).Perform();
                actions.Release(target).Perform();

                DriverScript.iOutcome = 1;
                WaitSeconds("", "3");
            }
            catch (Exception e)
            {
                Log.Error("Not able to drag and drop | Exception: " + e.Message);
                ExtentReporter.NodeError("Not able to drag and drop | Exception: " + e.Message);
                DriverScript.iOutcome = 3;
            }
        }

        public static void KeyPress(String obj, String data)
        {
            Log.Info($"KeyPress \"{data}\" on \"{obj}\"");
            ExtentReporter.NodeInfo($"KeyPress \"{data}\" on \"{obj}\"");
            try
            {
                string[] locator = obj.Split('_');
                switch (data.ToLower().Trim())
                {
                    case "enter":
                        driver.FindElement(LocateValue(locator[1], GetKey(obj))).SendKeys(Keys.Enter);
                        DriverScript.iOutcome = 1;
                        break;
                    case "return":
                        driver.FindElement(LocateValue(locator[1], GetKey(obj))).SendKeys(Keys.Return);
                        DriverScript.iOutcome = 1;
                        break;
                    case "tab":
                        driver.FindElement(LocateValue(locator[1], GetKey(obj))).SendKeys(Keys.Tab);
                        DriverScript.iOutcome = 1;
                        break;
                    default:
                        Log.Error("Not a key");
                        ExtentReporter.NodeError("Not a key");
                        DriverScript.iOutcome = 3;
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error("Not able to KeyPress " + data + " | Exception: " + e.Message);
                ExtentReporter.NodeError("Not able to KeyPress " + data + " | Exception: " + e.Message);
                DriverScript.iOutcome = 3;
            }
        }

        public static void KeyPressEnter(String obj, String data)
        {
            Actions builder = new Actions(driver);
            builder.SendKeys(Keys.Enter);
        }

        public static void DoubleClick(String obj, String data)
        {

        }
        
        public static void Clear(String obj, String data)
        {

        }      

        #endregion
    }
}
