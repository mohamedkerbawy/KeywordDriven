using KeywordDriven.Config;
using KeywordDriven.Execution;
using KeywordDriven.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Modules.BrowsingContext;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        internal static bool ClickByDriver(IWebDriver driver, By locator)
        {
            try
            {
                driver.FindElement(locator).Click();
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
        internal static bool ClickByJavascript(IWebDriver driver, By locator)
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;
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
        internal static void ClickByActions(IWebDriver driver, By locator)
        {
            try
            {
                new Actions(driver)
                    .MoveToElement(driver.FindElement(locator))
                    .Click()
                    .Perform();
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
            }
        }

        internal static void ClickByOffset(IWebDriver driver, By locator)
        {

        }

        /// <summary>
        /// Double clicks element using Actions.
        /// Use for elements that require double click to trigger.
        /// </summary>
        
        internal static void DoubleClick(IWebDriver driver, By locator)
        {
            try
            {
                new Actions(driver)
                    .DoubleClick(driver.FindElement(locator))
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
        internal static void RightClick(IWebDriver driver, By locator)
        {
            try
            {
                new Actions(driver)
                    .ContextClick(driver.FindElement(locator))
                    .Perform();
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
            }
        }

        /// <summary>
        /// Inputs text into element using standard SendKeys method. 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool InputByDriver(IWebDriver driver, By locator, String data)
        {
            try
            {
                driver.FindElement(locator).SendKeys(data);
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
        /// Inputs text into element using JavaScript. Use when SendKeys doesn't work (e.g. due to overlays, custom controls, or performance issues).
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool InputByJavascript(IWebDriver driver, By locator, String data)
        {
            try
            {
                IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
                jse.ExecuteScript("arguments[0].value='" + data + "';", driver.FindElement(locator));
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool SelectTextByDriver(IWebDriver driver, By locator, string data)
        {
            try
            {
                new SelectElement(driver.FindElement(locator)).SelectByText(data);
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                return false;
            }
        }

        internal static bool SelectValueByDriver(IWebDriver driver, By locator, string data)
        {
            try
            {
                new SelectElement(driver.FindElement(locator)).SelectByValue(data);
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
        /// Clears text from input field using standard Clear() method.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <returns></returns>
        internal static bool ClearByDriver(IWebDriver driver, By locator)
        {
            try
            {
                driver.FindElement(locator).Clear();
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
        /// Clears text from input field using JavaScript.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <returns></returns>
        internal static bool ClearByJavascript(IWebDriver driver, By locator)
        {
            try
            {
                IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
                jse.ExecuteScript("arguments[0].value = '';", driver.FindElement(locator));
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
        /// Presses specified key on the element. Supports special keys like Enter, Tab, Escape, etc.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="key"></param>
        /// <exception cref="Exception"></exception>
        internal static void PressKey(IWebDriver driver,By locator, string key)
        {
            try
            {
                string keyToSend = key.Trim().ToLower() switch
                {
                    "enter" => Keys.Enter,
                    "tab" => Keys.Tab,
                    "escape" => Keys.Escape,
                    "space" => Keys.Space,
                    "backspace" => Keys.Backspace,
                    "delete" => Keys.Delete,
                    "arrowup" => Keys.ArrowUp,
                    "arrowdown" => Keys.ArrowDown,
                    _ => key
                };

                driver.FindElement(locator).SendKeys(keyToSend);
            }
            catch (Exception ex)
            {
                throw new Exception($"PressKey [{locator}]: {ex.Message}");
            }
        }

        /// <summary>
        /// Uploads a file by sending the file path to an input[type='file'] element.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="filePath"></param>
        /// <exception cref="Exception"></exception>
        internal static void UploadFile(IWebDriver driver, By locator, string filePath)
        {
            try
            {
                // Validate file exists
                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException(
                        "File path cannot be null or empty.");

                if (!File.Exists(filePath))
                    throw new FileNotFoundException(
                        $"File not found: \"{filePath}\"");

                // Validate file size (warn if > 10MB) 
                FileInfo fileInfo = new FileInfo(filePath);
                double fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);

                if (fileSizeMB > 10)
                    Console.WriteLine($"UploadFile | Large file detected: {fileSizeMB:F2} MB | " +
                                      $"File: {fileInfo.Name}");

                // Locate file input element
                // Make hidden file inputs visible for upload

                // If element is hidden, make it visible via JS
                if (!driver.FindElement(locator).Displayed)
                {
                    ((IJavaScriptExecutor)driver)
                        .ExecuteScript("arguments[0].style.display='block'; " +
                                       "arguments[0].style.visibility='visible';", driver.FindElement(locator));

                    Console.WriteLine($"UploadFile | Hidden input made visible via JS | " +
                                      $"Locator: {locator}");
                }

                // Send file path to input 
                driver.FindElement(locator).SendKeys(filePath);

                // Wait for file name to appear in input ─
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(driver =>
                {
                    string value = driver.FindElement(locator).GetAttribute("value");
                    return !string.IsNullOrEmpty(value);
                });

                string uploadedValue = driver.FindElement(locator).GetAttribute("value");

                Console.WriteLine($"UploadFile" +
                                  $"\n  Locator  : {locator}" +
                                  $"\n  File     : {fileInfo.Name}" +
                                  $"\n  Size     : {fileSizeMB:F2} MB" +
                                  $"\n  Path     : {filePath}" +
                                  $"\n  Input Val: {uploadedValue}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"UploadFile | {ex.Message}");
                throw;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"UploadFile | {ex.Message}");
                throw;
            }
            catch (WebDriverTimeoutException)
            {
                throw new Exception(
                    $"UploadFile | File input not ready within timeout | Locator: {locator}");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"UploadFile | Locator: {locator} | Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Uploads multiple files to a multi-file input element.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="filePaths"></param>
        /// <exception cref="Exception"></exception>
        internal static void UploadMultipleFiles(IWebDriver driver,By locator, string[] filePaths)
        {
            try
            {
                if (filePaths == null || filePaths.Length == 0)
                    throw new ArgumentException("File paths array cannot be null or empty.");

                // Validate all files exist first
                List<string> missingFiles = new List<string>();
                foreach (string path in filePaths)
                {
                    if (!File.Exists(path))
                        missingFiles.Add(path);
                }

                if (missingFiles.Count > 0)
                    throw new FileNotFoundException(
                        $"Files not found:\n  {string.Join("\n  ", missingFiles)}");

                if (!driver.FindElement(locator).Displayed)
                {
                    ((IJavaScriptExecutor)driver)
                        .ExecuteScript("arguments[0].style.display='block'; " +
                                       "arguments[0].style.visibility='visible';", driver.FindElement(locator));
                }

                // ── Send all file paths joined by newline ─
                string combinedPaths = string.Join("\n", filePaths);
                driver.FindElement(locator).SendKeys(combinedPaths);

                Console.WriteLine($"[PASS] UploadMultipleFiles" +
                                  $"\n  Locator    : {locator}" +
                                  $"\n  File Count : {filePaths.Length}" +
                                  $"\n  Files      : {string.Join(", ", filePaths.Select(Path.GetFileName))}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"UploadMultipleFiles - {ex.Message}");
                throw;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"UploadMultipleFiles - {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"UploadMultipleFiles | Locator: {locator} | Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Takes a screenshot of a specific web element only (crops from full page).
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="screenshotFolder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static string TakeElementScreenshot(IWebDriver driver, By locator, string screenshotFolder, string fileName)
        {
            try
            {
                if (driver == null)
                    throw new InvalidOperationException(
                        "Browser is not open. Call OpenBrowser before TakeElementScreenshot.");

                // Create folder if it doesn't exist
                if (!Directory.Exists(screenshotFolder))
                {
                    Directory.CreateDirectory(screenshotFolder);
                    Console.WriteLine($"Created screenshot folder | {screenshotFolder}");
                }

                // Auto-generate filename if not provided
                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss_fff}";

                // Sanitize filename
                foreach (char c in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(c, '_');

                string fullPath = Path.Combine(screenshotFolder, $"{fileName}.png");

                // Locate the element
                IWebElement element = driver.FindElement(locator);

                // Cast the element to ITakesScreenshot
                Screenshot elementScreenshot = ((ITakesScreenshot)element).GetScreenshot();

                // Save to disk
                elementScreenshot.SaveAsFile(fullPath);

                Console.WriteLine($"Screenshot saved to | {fullPath}");

                return fullPath;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"TakeElementScreenshot | {ex.Message}");
                throw;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"TakeElementScreenshot | {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"TakeElementScreenshot failed | {ex.Message}");
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

                Waits.WaitUntilClickable(Drivers.driver, locator);

                ((IJavaScriptExecutor)Drivers.driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", locator);

                if (Locators.ClickByDriver(Drivers.driver,locator))
                {
                    DriverScript.outcome = (int)Outcome.Pass;
                }
                else if (Locators.ClickByJavascript(Drivers.driver,locator))
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

                Waits.WaitUntilClickable(Drivers.driver, locator);

                if (!Locators.InputByDriver(Drivers.driver, locator, data))
                {
                    if (!Locators.InputByJavascript(Drivers.driver, locator, data))
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

                Waits.WaitUntilClickable(Drivers.driver, locator);
                WaitSeconds("", "2");

                if (!Locators.ClearByDriver(Drivers.driver, locator))
                {
                    if (!Locators.ClearByJavascript(Drivers.driver, locator))
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

                Waits.WaitUntilExists(Drivers.driver, locator);

                if (!Locators.SelectTextByDriver(Drivers.driver, locator, data))
                {
                    if (!Locators.SelectValueByDriver(Drivers.driver, locator, data))
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

                Locators.PressKey(Drivers.driver, locator,data);

                DriverScript.outcome = (int)Outcome.Pass;


            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeError($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void UploadFile(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name} \"{data}\"");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} \"{data}\"");
            try
            {
                By locator = Locators.GetLocator(obj);

                Waits.WaitUntilExists(Drivers.driver, locator);

                Locators.UploadFile(Drivers.driver, locator, data);

                DriverScript.outcome = (int)Outcome.Pass;

            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeError($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                DriverScript.outcome = (int)Outcome.Error;
            }
        }

        public static void UploadMultipleFiles(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name} \"{data}\"");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} \"{data}\"");
            try
            {
                By locator = Locators.GetLocator(obj);

                Waits.WaitUntilExists(Drivers.driver, locator);

                string[] filepaths = data.Split(',');

                Locators.UploadMultipleFiles(Drivers.driver, locator, filepaths);

                DriverScript.outcome = (int)Outcome.Pass;

            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeError($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                DriverScript.outcome = (int)Outcome.Error;
            }
        }
        
        public static void TakeElementScreenshot(String obj, String data)
        {
            try
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name}");

                By locator = Locators.GetLocator(obj);

                Waits.WaitUntilExists(Drivers.driver, locator);

                Locators.TakeElementScreenshot(Drivers.driver, locator, "", data);

                DriverScript.outcome = 1;

            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                DriverScript.outcome = (int)Outcome.Error;
            }
        }

        #endregion
    }
}
