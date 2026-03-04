using AngleSharp.Text;
using KeywordDriven.Config;
using KeywordDriven.Execution;
using KeywordDriven.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace KeywordDriven.ActionKeywords
{
    internal static class Drivers
    {
        internal static IWebDriver driver;

        /// <summary>
        /// Launches Google Chrome with optional headless mode
        /// </summary>
        internal static IWebDriver OpenChrome()
        {
            try
            {
                // Auto-manages ChromeDriver version
                new DriverManager().SetUpDriver(new ChromeConfig());

                ChromeOptions options = new ChromeOptions();

                if (DriverSetting.Headless)
                {
                    options.AddArgument("--headless=new");  // New headless mode (Chrome 112+)
                    options.AddArgument("--window-size=1920,1080");
                }

                // Common Chrome options
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-popup-blocking");
                options.AddArgument("--ignore-certificate-errors");
                options.AddExcludedArgument("enable-automation");  // Hide "Chrome is controlled by automation"
                options.AddUserProfilePreference("credentials_enable_service", false); // Disable save password prompt
                options.AddUserProfilePreference("profile.password_manager_enabled", false);

                options.AddUserProfilePreference("download.prompt_for_download",false);
                options.AddUserProfilePreference("profile.default_content_settings.popups", 0);

                IWebDriver driver = new ChromeDriver(options);
                Console.WriteLine("Chrome browser launched successfully.");

                return driver;
            }
            catch (Exception ex)
            {
                throw new Exception($"OpenChrome failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Launches Mozilla Firefox with optional headless mode
        /// </summary>
        internal static IWebDriver OpenFirefox()
        {
            try
            {
                // Auto-manages GeckoDriver version
                new DriverManager().SetUpDriver(new FirefoxConfig());

                FirefoxOptions options = new FirefoxOptions();

                if (DriverSetting.Headless)
                {
                    options.AddArgument("--headless");
                    options.AddArgument("--width=1920");
                    options.AddArgument("--height=1080");
                }

                // Common Firefox options
                options.SetPreference("dom.webnotifications.enabled", false);  // Disable notifications
                options.SetPreference("media.volume_scale", "0.0");            // Mute audio
                options.SetPreference("browser.download.folderList", 2);
                options.SetPreference("browser.helperApps.neverAsk.saveToDisk",
                                      "application/pdf,application/octet-stream");
                options.AcceptInsecureCertificates = true;                

                IWebDriver driver = new FirefoxDriver(options);
                Console.WriteLine("Firefox browser launched successfully.");
                return driver;
            }
            catch (Exception ex)
            {
                throw new Exception($"OpenFirefox failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Launches Microsoft Edge (Chromium) with optional headless mode
        /// </summary>
        internal static IWebDriver OpenEdge()
        {
            try
            {
                // Auto-manages EdgeDriver version
                new DriverManager().SetUpDriver(new EdgeConfig());

                EdgeOptions options = new EdgeOptions();

                if (DriverSetting.Headless)
                {
                    options.AddArgument("--headless=new");
                    options.AddArgument("--window-size=1920,1080");
                }

                // Common Edge options
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("--inprivate");  // Launch in InPrivate mode
                options.AddUserProfilePreference("credentials_enable_service", false);
                options.AddUserProfilePreference("profile.password_manager_enabled", false);

                IWebDriver driver = new EdgeDriver(options);
                Console.WriteLine("Edge browser launched successfully.");
                return driver;
            }
            catch (Exception ex)
            {
                throw new Exception($"OpenEdge failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Generic OpenBrowser keyword — routes to specific browser method
        /// </summary>
        /// <param name="browserType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static IWebDriver OpenBrowser(string browserType = "chrome")
        {
            try
            {
                driver = browserType.Trim().ToLower() switch
                {
                    "chrome" => OpenChrome(),
                    "firefox" => OpenFirefox(),
                    "edge" => OpenEdge(),
                    _ => throw new ArgumentException(
                            $"Unsupported browser: '{browserType}'. Valid options: chrome, firefox, edge")
                };

                driver.Manage().Window.Maximize();
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(DriverSetting.DefaultTimeoutSeconds);

                Console.WriteLine($"OpenBrowser launched successfully | Browser: {browserType} | Headless: {DriverSetting.Headless}");
                Log.Info($"OpenBrowser launched successfully | Browser: {browserType} | Headless: {DriverSetting.Headless}");
                ExtentReporter.NodeInfo($"OpenBrowser launched successfully | Browser: {browserType} | Headless: {DriverSetting.Headless}");
                return driver;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"OpenBrowser - {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"OpenBrowser failed [{browserType}]: {ex.Message}");
            }
        }

        /// <summary>
        /// Maximizes the current browser window to full screen.
        /// </summary>
        internal static void MaximizeBrowser()
        {
            try
            {
                if (driver == null)
                    throw new InvalidOperationException(
                        "Browser is not open. Call OpenBrowser before MaximizeBrowser.");

                driver.Manage().Window.Maximize();

                Console.WriteLine($"MaximizeBrowser | " +
                                  $"Size: {driver.Manage().Window.Size.Width}x{driver.Manage().Window.Size.Height}");
                Log.Info($"MaximizeBrowser | " +
                                  $"Size: {driver.Manage().Window.Size.Width}x{driver.Manage().Window.Size.Height}");
                ExtentReporter.NodeInfo($"MaximizeBrowser | " +
                                  $"Size: {driver.Manage().Window.Size.Width}x{driver.Manage().Window.Size.Height}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"MaximizeBrowser | {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"MaximizeBrowser failed | {ex.Message}");
            }
        }

        /// <summary>
        /// Navigates the current browser session to the specified URL.
        /// Must be called after OpenBrowser.
        /// </summary>
        /// <param name="url">Full URL to navigate to (e.g. https://example.com)</param>
        internal static void NavigateToUrl(string url)
        {
            try
            {
                if (driver == null)
                    throw new InvalidOperationException(
                        "Browser is not open. Call OpenBrowser before NavigateToUrl.");

                if (string.IsNullOrWhiteSpace(url))
                    throw new ArgumentException("URL cannot be null or empty.");

                // Prepend https:// if no scheme is provided
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                    url = "https://" + url;

                driver.Navigate().GoToUrl(url);

                // Wait until page is fully loaded
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DriverSetting.PageLoadTimeoutSeconds));
                wait.Until(driver => ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState").ToString() == "complete");

                Console.WriteLine($"NavigateToUrl | URL: {url} | Title: {driver.Title}");
                Log.Info($"NavigateToUrl | URL: {url} | Title: {driver.Title}");
                ExtentReporter.NodeInfo($"NavigateToUrl | URL: {url} | Title: {driver.Title}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"NavigateToUrl - {ex.Message}");
                throw;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"NavigateToUrl - {ex.Message}");
                throw;
            }
            catch (WebDriverException ex)
            {
                throw new Exception($"NavigateToUrl failed [{url}]: {ex.Message}");
            }
        }

        /// <summary>
        /// Refreshes the current browser page and waits for full reload.
        /// </summary>
        internal static void RefreshPage()
        {
            try
            {
                if (driver == null)
                    throw new InvalidOperationException(
                        "Browser is not open. Call OpenBrowser before RefreshPage.");

                string urlBefore = driver.Url;
                string titleBefore = driver.Title;

                driver.Navigate().Refresh();

                // Wait for page to fully reload
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(driver => ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState").ToString() == "complete");

                Console.WriteLine($"RefreshPage | URL: {urlBefore} | Title Before: {titleBefore} | Title After: {driver.Title}");
                Log.Info($"RefreshPage | URL: {urlBefore} | Title Before: {titleBefore} | Title After: {driver.Title}");
                ExtentReporter.NodeInfo($"RefreshPage | URL: {urlBefore} | Title Before: {titleBefore} | Title After: {driver.Title}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"RefreshPage | {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"RefreshPage failed | {ex.Message}");
            }
        }
        /// <summary>
        /// Navigates the browser back to the previous page in history.
        /// Waits for the page to fully load after navigating.
        /// </summary>
        internal static void NavigateBack()
        {
            try
            {
                if (driver == null)
                    throw new InvalidOperationException(
                        "Browser is not open. Call OpenBrowser before NavigateBack.");

                string urlBefore = driver.Url;

                driver.Navigate().Back();

                // Wait for page to fully load
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(driver => ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState").ToString() == "complete");

                Console.WriteLine($"NavigateBack | From: {urlBefore} | To: {driver.Url}");
                Log.Info($"NavigateBack | From: {urlBefore} | To: {driver.Url}");
                ExtentReporter.NodeInfo($"NavigateBack | From: {urlBefore} | To: {driver.Url}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"NavigateBack | {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"NavigateBack failed | {ex.Message}");
            }
        }

        /// <summary>
        /// Navigates the browser forward to the next page in history.
        /// Waits for the page to fully load after navigating.
        /// </summary>
        internal static void NavigateForward()
        {
            try
            {
                if (driver == null)
                    throw new InvalidOperationException(
                        "Browser is not open. Call OpenBrowser before NavigateForward.");

                string urlBefore = driver.Url;

                driver.Navigate().Forward();

                // Wait for page to fully load
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(driver => ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState").ToString() == "complete");

                Console.WriteLine($"NavigateForward | From: {urlBefore} | To: {driver.Url}");
                Log.Info($"NavigateForward | From: {urlBefore} | To: {driver.Url}");
                ExtentReporter.NodeInfo($"NavigateForward | From: {urlBefore} | To: {driver.Url}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"NavigateForward | {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"NavigateForward failed | {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the browser window to a specific width and height in pixels.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <exception cref="Exception"></exception>
        internal static void SetWindowSize(int width, int height)
        {
            try
            {
                if (driver == null)
                    throw new InvalidOperationException(
                        "Browser is not open. Call OpenBrowser before SetWindowSize.");

                if (width <= 0 || height <= 0)
                    throw new ArgumentException(
                        $"Invalid dimensions: {width}x{height}. Width and Height must be greater than 0.");

                driver.Manage().Window.Size = new System.Drawing.Size(width, height);

                // Verify actual applied size (browser may enforce a minimum)
                var actualSize = driver.Manage().Window.Size;

                Console.WriteLine($"SetWindowSize | " +
                                  $"Requested: {width}x{height} | " +
                                  $"Applied: {actualSize.Width}x{actualSize.Height}");
                Log.Info($"SetWindowSize | " +
                                  $"Requested: {width}x{height} | " +
                                  $"Applied: {actualSize.Width}x{actualSize.Height}");
                ExtentReporter.NodeInfo($"SetWindowSize | " +
                                  $"Requested: {width}x{height} | " +
                                  $"Applied: {actualSize.Width}x{actualSize.Height}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"SetWindowSize | {ex.Message}");
                throw;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"SetWindowSize | {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"SetWindowSize failed [{width}x{height}] | {ex.Message}");
            }
        }

    }
    
    public partial class Keywords
    {
        #region Keywords methods

        public static void OpenBrowser(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name} \"{data}\"");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} \"{data}\"");
            try
            {
                Drivers.driver = Drivers.OpenBrowser(data);

                DriverScript.outcome = 1;
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeError($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        
        public static void CloseBrowser(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name}");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name}");

            try
            {
                Drivers.driver?.Close();
                DriverScript.outcome = 1;

                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Window closed.");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Window closed.");
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: " + e.Message);
                ExtentReporter.NodeError($"{MethodBase.GetCurrentMethod().Name} | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        
        public static void CloseAllBrowser(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name}");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name}");

            try
            {
                Drivers.driver?.Quit();
                Drivers.driver = null;

                Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} | Driver terminated.");
                Log.Info($"{MethodBase.GetCurrentMethod().Name} | Driver terminated.");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Driver terminated.");
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: " + e.Message);
                ExtentReporter.NodeError($"{MethodBase.GetCurrentMethod().Name} | Exception: " + e.Message);
                DriverScript.outcome = (int)Outcome.Error;
            }
        }     
        
        public static void MaximizeBrowser(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name}");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name}");
            try
            {
                Drivers.MaximizeBrowser();

                DriverScript.outcome = 1;
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                DriverScript.outcome = (int)Outcome.Error;
            }
        }
        
        public static void NavigateToURL(String obj, String data)
        {
            try
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name} \"{data}\"");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} \"{data}\"");

                Drivers.NavigateToUrl(data);

                DriverScript.outcome = 1;

            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: " + e.Message);
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void RefreshPage(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name}");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name}");
            try
            {
                Drivers.RefreshPage();

                DriverScript.outcome = 1;
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                DriverScript.outcome = (int)Outcome.Error;
            }
        }
        
        public static void NavigateBack(String obj, String data)
        {
            try
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name}");

                Drivers.NavigateBack();

                DriverScript.outcome = 1;

            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }
        
        public static void NavigateForward(String obj, String data)
        {
            try
            {
                Log.Info($"{MethodBase.GetCurrentMethod().Name}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name}");

                Drivers.NavigateForward();

                DriverScript.outcome = 1;

            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name} | Exception: {e.Message}");
                DriverScript.outcome = (int)Outcome.Error;
            }
        }        
        
        public static void SetWindowSize(String obj, String data)
        {
            Log.Info($"{MethodBase.GetCurrentMethod().Name}");
            ExtentReporter.NodeInfo($"{MethodBase.GetCurrentMethod().Name}");
            try
            {
                string[] size = data.Split(',');

                int width =Convert.ToInt32(size[0]);
                int height = Convert.ToInt32(size[1]);

                Drivers.SetWindowSize(width, height);

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
