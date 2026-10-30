using KeywordDriven.Config;
using KeywordDriven.Execution;
using KeywordDriven.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace KeywordDriven.ActionKeywords
{
    internal class Locators
    {
        /// <summary>
        /// Maps locator type with locator value and return locator By object.
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static By GetLocator(string obj)
        {
            if (string.IsNullOrWhiteSpace(obj))
                throw new ArgumentException("Page object name cannot be null or empty.", nameof(obj));

            string[] pageObject = obj.Split('_', 3);
            if (pageObject.Length < 2 || string.IsNullOrWhiteSpace(pageObject[1]))
                throw new ArgumentException(
                    $"Page object '{obj}' must include a locator type after the first underscore.",
                    nameof(obj));

            string locatorType = pageObject[1].Trim();
            string locatorvalue = ExcelManager.GetKeyValue(obj, ExcelSetting.Col_Locators_PageObject, ExcelSetting.Sheet_Locators);

            if (string.IsNullOrWhiteSpace(locatorvalue))
                throw new InvalidOperationException($"No locator value is configured for page object '{obj}'.");

            return locatorType.ToLowerInvariant() switch
            {
                "xpath" => By.XPath(locatorvalue),
                "id" => By.Id(locatorvalue),
                "css" or "cssselector" or "csslocator" => By.CssSelector(locatorvalue),
                "class" or "classname" => By.ClassName(locatorvalue),
                "tag" or "tagname" => By.TagName(locatorvalue),
                "linktext" => By.LinkText(locatorvalue),
                "name" => By.Name(locatorvalue),
                "partiallinktext" => By.PartialLinkText(locatorvalue),
                "testid" or "datatestid" => By.XPath($"//*[@data-testid={ToXPathLiteral(locatorvalue)}]"),
                "arialabel" => By.XPath($"//*[@aria-label={ToXPathLiteral(locatorvalue)}]"),
                "role" => By.XPath($"//*[@role={ToXPathLiteral(locatorvalue)}]"),
                "text" => By.XPath($"//*[normalize-space(.)={ToXPathLiteral(locatorvalue)}]"),
                "partialtext" => By.XPath($"//*[contains(normalize-space(.), {ToXPathLiteral(locatorvalue)})]"),
                _ => throw new ArgumentException(
                    $"Unsupported locator type '{locatorType}'. Valid types: id, name, xpath, css, classname, tagname, linktext, partiallinktext, testid, arialabel, role, text, partialtext.",
                    nameof(obj)),
            };
        }

        private static string ToXPathLiteral(string value)
        {
            if (!value.Contains('\''))
                return $"'{value}'";
            if (!value.Contains('"'))
                return $"\"{value}\"";

            string[] parts = value.Split('\'');
            return $"concat({string.Join(", \"'\", ", parts.Select(part => $"'{part}'"))})";
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
                js.ExecuteScript("arguments[0].click();", driver.FindElement(locator));
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
            new Actions(driver)
                .MoveToElement(driver.FindElement(locator))
                .Click()
                .Perform();
        }

        /// <summary>
        /// Double clicks element using Actions.
        /// Use for elements that require double click to trigger.
        /// </summary>        
        internal static void DoubleClick(IWebDriver driver, By locator)
        {
            new Actions(driver)
                .DoubleClick(driver.FindElement(locator))
                .Perform();
        }

        /// <summary>
        /// Right clicks element using Actions.
        /// Use to open context menus.
        /// </summary>
        internal static void RightClick(IWebDriver driver, By locator)
        {
            new Actions(driver)
                .ContextClick(driver.FindElement(locator))
                .Perform();
        }

        /// <summary>
        /// Inputs text into element using standard SendKeys method. 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool InputByDriver(IWebDriver driver, By locator, string data)
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
        internal static bool InputByJavascript(IWebDriver driver, By locator, string data, bool append = false)
        {
            try
            {
                IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
                jse.ExecuteScript(
                    "const element = arguments[0];" +
                    "const text = arguments[1];" +
                    "const append = arguments[2];" +
                    "if ('value' in element) {" +
                    "const value = append ? element.value + text : text;" +
                    "const setter = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(element), 'value')?.set;" +
                    "if (setter) setter.call(element, value); else element.value = value;" +
                    "} else if (element.isContentEditable) {" +
                    "element.textContent = append ? element.textContent + text : text;" +
                    "} else { throw new Error('Element is not an editable text control.'); }" +
                    "element.dispatchEvent(new Event('input', { bubbles: true }));" +
                    "element.dispatchEvent(new Event('change', { bubbles: true }));",
                    driver.FindElement(locator),
                    data,
                    append);
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
        /// Selects option by visible text.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static void SelectByText(IWebDriver driver, By locator, string text)
        {
            new SelectElement(driver.FindElement(locator)).SelectByText(text);
        }

        /// <summary>
        /// Selects option by value attribute.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static void SelectByValue(IWebDriver driver, By locator, string value)
        {
            new SelectElement(driver.FindElement(locator)).SelectByValue(value);
        }

        /// <summary>
        /// Selects option by its index position (zero-based).
        /// e.g. index 0 = first option, index 1 = second option
        /// </summary>
        internal static void SelectByIndex(IWebDriver driver, By locator, int index)
        {
            new SelectElement(driver.FindElement(locator)).SelectByIndex(index);
        }

        /// <summary>
        /// Selects the first option in the dropdown.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        internal static void SelectFirstOption(IWebDriver driver, By locator)
        {
            new SelectElement(driver.FindElement(locator)).SelectByIndex(0);
        }

        /// <summary>
        /// Selects the last option in the dropdown.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        internal static void SelectLastOption(IWebDriver driver, By locator)
        {
            var select = new SelectElement(driver.FindElement(locator));
            select.SelectByIndex(select.Options.Count - 1);
        }

        /// <summary>
        /// Selects multiple options by visible text (for multi-select dropdowns).
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="texts"></param>
        internal static void SelectMultipleByText(IWebDriver driver, By locator, IEnumerable<string> texts)
        {
            ArgumentNullException.ThrowIfNull(texts);
            var select = new SelectElement(driver.FindElement(locator));
            if (!select.IsMultiple)
                throw new InvalidOperationException("Multiple options can only be selected on a multi-select element.");

            foreach (var text in texts)
                select.SelectByText(text);
        }

        /// <summary>
        /// Deselects all selected options (for multi-select dropdowns).
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        internal static void DeselectAll(IWebDriver driver, By locator)
        {
            new SelectElement(driver.FindElement(locator)).DeselectAll();
        }

        /// <summary>
        /// Checks the checkbox if it is not already checked.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        internal static void CheckCheckbox(IWebDriver driver, By locator)
        {
            var element = driver.FindElement(locator);
            if (!GetCheckableState(element, "checkbox"))
                element.Click();
        }

        /// <summary>
        /// Unchecks the checkbox if it is not already unchecked.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        internal static void UnCheckCheckbox(IWebDriver driver, By locator)
        {
            var element = driver.FindElement(locator);
            if (GetCheckableState(element, "checkbox"))
                element.Click();
        }

        /// <summary>
        /// Selects a radio button if it is not already selected.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        internal static void SelectRadioButton(IWebDriver driver, By locator)
        {
            var element = driver.FindElement(locator);
            if (!GetCheckableState(element, "radio"))
                element.Click();
        }

        private static bool IsRadioButton(IWebElement element)
        {
            return (element.TagName.Equals("input", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(element.GetAttribute("type"), "radio", StringComparison.OrdinalIgnoreCase))
                || string.Equals(element.GetAttribute("role"), "radio", StringComparison.OrdinalIgnoreCase);
        }

        private static bool GetCheckableState(IWebElement element, string expectedType)
        {
            bool isNativeInput = element.TagName.Equals("input", StringComparison.OrdinalIgnoreCase)
                && string.Equals(element.GetAttribute("type"), expectedType, StringComparison.OrdinalIgnoreCase);
            bool isAriaControl = string.Equals(
                element.GetAttribute("role"),
                expectedType,
                StringComparison.OrdinalIgnoreCase);

            if (!isNativeInput && !isAriaControl)
                throw new InvalidOperationException($"Element is not a {expectedType} control.");

            if (isNativeInput)
                return element.Selected;

            return element.GetAttribute("aria-checked") switch
            {
                "true" => true,
                "false" or "mixed" => false,
                _ => throw new InvalidOperationException(
                    $"ARIA {expectedType} control must have aria-checked='true' or 'false'.")
            };
        }

        /// <summary>
        /// Selects a radio button from a group by its visible label text.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="groupLocator"></param>
        /// <param name="labelText"></param>
        /// <exception cref="NoSuchElementException"></exception>
        internal static void SelectRadioButtonByText(IWebDriver driver, By groupLocator, string labelText)
        {
            var radios = driver.FindElements(groupLocator).Where(IsRadioButton).ToList();

            var match = radios.FirstOrDefault(r =>
                string.Equals(r.GetAttribute("value"), labelText, StringComparison.OrdinalIgnoreCase)
                || string.Equals(r.Text.Trim(), labelText, StringComparison.OrdinalIgnoreCase)
                || GetRadioLabelText(driver, r).Equals(labelText, StringComparison.OrdinalIgnoreCase)
            );

            if (match == null)
                throw new NoSuchElementException(
                    $"Radio button with text '{labelText}' not found. " +
                    $"Available: [{string.Join(", ", radios.Select(r => r.GetAttribute("value")))}]"
                );

            if (!GetCheckableState(match, "radio"))
                match.Click();
        }

        private static string GetRadioLabelText(IWebDriver driver, IWebElement radio)
        {
            string id = radio.GetAttribute("id");
            if (!string.IsNullOrEmpty(id))
            {
                string labelText = driver.FindElements(By.XPath($"//label[@for={ToXPathLiteral(id)}]"))
                    .FirstOrDefault()?.Text;
                if (!string.IsNullOrWhiteSpace(labelText))
                    return labelText.Trim();
            }

            return radio.FindElements(By.XPath("ancestor::label[1]"))
                .FirstOrDefault()?.Text.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Selects a radio button from a group by its value attribute.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="groupLocator"></param>
        /// <param name="value"></param>
        /// <exception cref="NoSuchElementException"></exception>
        internal static void SelectRadioButtonByValue(IWebDriver driver, By groupLocator, string value)
        {
            var radios = driver.FindElements(groupLocator).Where(IsRadioButton).ToList();

            var match = radios.FirstOrDefault(r =>
                r.GetAttribute("value")?.Equals(value, StringComparison.OrdinalIgnoreCase) == true
            );

            if (match == null)
                throw new NoSuchElementException(
                    $"Radio button with value '{value}' not found. " +
                    $"Available values: [{string.Join(", ", radios.Select(r => r.GetAttribute("value")))}]"
                );

            if (!GetCheckableState(match, "radio"))
                match.Click();
        }

        /// <summary>
        /// Selects a radio button from a group by its index position (zero-based).
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="groupLocator"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static void SelectRadioButtonByIndex(IWebDriver driver, By groupLocator, int index)
        {
            var radios = driver.FindElements(groupLocator).Where(IsRadioButton).ToList();

            if (index < 0 || index >= radios.Count)
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    $"Group has {radios.Count} radio buttons (valid indexes: 0 to {radios.Count - 1})."
                );

            var target = radios.ElementAt(index);

            if (!GetCheckableState(target, "radio"))
                target.Click();
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
                jse.ExecuteScript(
                    "const element = arguments[0];" +
                    "if ('value' in element) {" +
                    "const setter = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(element), 'value')?.set;" +
                    "if (setter) setter.call(element, ''); else element.value = '';" +
                    "} else if (element.isContentEditable) {" +
                    "element.textContent = '';" +
                    "} else { throw new Error('Element is not an editable text control.'); }" +
                    "element.dispatchEvent(new Event('input', { bubbles: true }));" +
                    "element.dispatchEvent(new Event('change', { bubbles: true }));",
                    driver.FindElement(locator));
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
        internal static void PressKey(IWebDriver driver, By locator, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            string keyName = key.Trim().ToLowerInvariant();
            string keyToSend = keyName switch
            {
                "enter" => Keys.Enter,
                "tab" => Keys.Tab,
                "escape" => Keys.Escape,
                "space" => Keys.Space,
                "backspace" => Keys.Backspace,
                "delete" => Keys.Delete,
                "arrowup" => Keys.ArrowUp,
                "arrowdown" => Keys.ArrowDown,
                "arrowleft" => Keys.ArrowLeft,
                "arrowright" => Keys.ArrowRight,
                "home" => Keys.Home,
                "end" => Keys.End,
                "pageup" => Keys.PageUp,
                "pagedown" => Keys.PageDown,
                "control" or "ctrl" => Keys.Control,
                "shift" => Keys.Shift,
                "alt" => Keys.Alt,
                _ => key
            };

            driver.FindElement(locator).SendKeys(keyToSend);
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
        internal static void UploadMultipleFiles(IWebDriver driver, By locator, string[] filePaths)
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

        internal static void ExecuteLocatorKeyword(string keyword, string obj, string data, Action<By, string> operation)
        {
            Log.Info($"{keyword} \"{data}\" on \"{obj}\"");
            ExtentReporter.NodeInfo($"{keyword} \"{data}\" on \"{obj}\"");

            try
            {
                operation(Locators.GetLocator(obj), data);
                DriverScript.outcome = (int)Outcome.Pass;
            }
            catch (Exception exception)
            {
                Log.Error($"{keyword} | Exception: {exception.Message}");
                ExtentReporter.NodeError($"{keyword} | Exception: {exception.Message}");
                DriverScript.outcome = (int)Outcome.Error;
            }
        }

    }
    
    public partial class Keywords
    {

        #region Keywords methods

        public static void Click(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(Click), obj, data, (locator, _) =>
            {
                Waits.WaitUntilClickable(Drivers.driver, locator);
                IWebElement element = Drivers.driver.FindElement(locator);
                ((IJavaScriptExecutor)Drivers.driver).ExecuteScript(
                    "arguments[0].scrollIntoView({block: 'center'});",
                    element);

                if (!Locators.ClickByDriver(Drivers.driver, locator)
                    && !Locators.ClickByJavascript(Drivers.driver, locator))
                    throw new InvalidOperationException($"Could not click page object '{obj}'.");
            });

        public static void TypeText(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(TypeText), obj, data, (locator, text) =>
            {
                Waits.WaitUntilEditable(Drivers.driver, locator);
                if (!Locators.InputByDriver(Drivers.driver, locator, text)
                    && !Locators.InputByJavascript(Drivers.driver, locator, text))
                    throw new InvalidOperationException($"Could not type text into '{obj}'.");
            });

        public static void AppendText(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(AppendText), obj, data, (locator, text) =>
            {
                Waits.WaitUntilEditable(Drivers.driver, locator);
                if (!Locators.InputByDriver(Drivers.driver, locator, text)
                    && !Locators.InputByJavascript(Drivers.driver, locator, text, append: true))
                    throw new InvalidOperationException($"Could not append text to '{obj}'.");
            });

        public static void ClearText(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(ClearText), obj, data, (locator, _) =>
            {
                Waits.WaitUntilEditable(Drivers.driver, locator);
                if (!Locators.ClearByDriver(Drivers.driver, locator)
                    && !Locators.ClearByJavascript(Drivers.driver, locator))
                    throw new InvalidOperationException($"Could not clear text from '{obj}'.");
            });
        
        public static void SelectByText(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(SelectByText), obj, data, (locator, text) =>
            {
                Waits.WaitUntilExists(Drivers.driver, locator);
                Locators.SelectByText(Drivers.driver, locator, text);
            });

        public static void SelectByValue(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(SelectByValue), obj, data, (locator, value) =>
            {
                Waits.WaitUntilExists(Drivers.driver, locator);
                Locators.SelectByValue(Drivers.driver, locator, value);
            });

        public static void SelectByIndex(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(SelectByIndex), obj, data, (locator, value) =>
            {
                if (!int.TryParse(value, out int index) || index < 0)
                    throw new ArgumentException("SelectByIndex requires a non-negative integer index.", nameof(data));

                Waits.WaitUntilExists(Drivers.driver, locator);
                Locators.SelectByIndex(Drivers.driver, locator, index);
            });

        public static void CheckCheckbox(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(CheckCheckbox), obj, data, (locator, _) =>
            {
                Waits.WaitUntilClickable(Drivers.driver, locator);
                Locators.CheckCheckbox(Drivers.driver, locator);
            });

        public static void SelectRadioButton(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(SelectRadioButton), obj, data, (locator, _) =>
            {
                Waits.WaitUntilClickable(Drivers.driver, locator);
                Locators.SelectRadioButton(Drivers.driver, locator);
            });

        public static void SelectRadioButtonByText(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(SelectRadioButtonByText), obj, data, (locator, text) =>
                Locators.SelectRadioButtonByText(Drivers.driver, locator, text));

        public static void SelectRadioButtonByValue(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(SelectRadioButtonByValue), obj, data, (locator, value) =>
                Locators.SelectRadioButtonByValue(Drivers.driver, locator, value));

        public static void SelectRadioButtonByIndex(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(SelectRadioButtonByIndex), obj, data, (locator, value) =>
            {
                if (!int.TryParse(value, out int index) || index < 0)
                    throw new ArgumentException("SelectRadioButtonByIndex requires a non-negative integer index.", nameof(data));

                Locators.SelectRadioButtonByIndex(Drivers.driver, locator, index);
            });

        public static void UnCheckCheckbox(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(UnCheckCheckbox), obj, data, (locator, _) =>
                Locators.UnCheckCheckbox(Drivers.driver, locator));

        public static void UncheckCheckbox(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(UncheckCheckbox), obj, data, (locator, _) =>
                Locators.UnCheckCheckbox(Drivers.driver, locator));

        public static void SelectFirstOption(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(SelectFirstOption), obj, data, (locator, _) =>
                Locators.SelectFirstOption(Drivers.driver, locator));

        public static void SelectLastOption(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(SelectLastOption), obj, data, (locator, _) =>
                Locators.SelectLastOption(Drivers.driver, locator));

        public static void SelectMultipleByText(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(SelectMultipleByText), obj, data, (locator, optionData) =>
            {
                string[] optionTexts = optionData.Split(
                    '|',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (optionTexts.Length == 0)
                    throw new ArgumentException("SelectMultipleByText requires pipe-separated option texts.", nameof(data));

                Locators.SelectMultipleByText(Drivers.driver, locator, optionTexts);
            });

        public static void DeselectAll(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(DeselectAll), obj, data, (locator, _) =>
                Locators.DeselectAll(Drivers.driver, locator));

        public static void ClickByActions(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(ClickByActions), obj, data, (locator, _) =>
                Locators.ClickByActions(Drivers.driver, locator));

        public static void DoubleClick(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(DoubleClick), obj, data, (locator, _) =>
                Locators.DoubleClick(Drivers.driver, locator));

        public static void RightClick(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(RightClick), obj, data, (locator, _) =>
                Locators.RightClick(Drivers.driver, locator));

        public static void PressKey(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(PressKey), obj, data, (locator, key) =>
                Locators.PressKey(Drivers.driver, locator, key));

        public static void UploadFile(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(UploadFile), obj, data, (locator, filePath) =>
            {
                Waits.WaitUntilExists(Drivers.driver, locator);
                Locators.UploadFile(Drivers.driver, locator, filePath);
            });

        public static void UploadMultipleFiles(String obj, String data) =>
            Locators.ExecuteLocatorKeyword(nameof(UploadMultipleFiles), obj, data, (locator, paths) =>
            {
                char separator = paths.Contains('|') ? '|' : ',';
                string[] filePaths = paths.Split(
                    separator,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (filePaths.Length == 0)
                    throw new ArgumentException("UploadMultipleFiles requires pipe- or comma-separated file paths.", nameof(data));

                Waits.WaitUntilExists(Drivers.driver, locator);
                Locators.UploadMultipleFiles(Drivers.driver, locator, filePaths);
            });

        #endregion
    }
}
