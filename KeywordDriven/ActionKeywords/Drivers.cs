using KeywordDriven.Config;
using KeywordDriven.Execution;
using KeywordDriven.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
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
            if (string.IsNullOrWhiteSpace(browserType))
                throw new ArgumentException("Browser type cannot be null or empty.", nameof(browserType));
            if (driver != null)
                throw new InvalidOperationException("A browser session is already open. Close it before opening another.");
            if (DriverSetting.DefaultTimeoutSeconds <= 0 || DriverSetting.PageLoadTimeoutSeconds <= 0)
                throw new InvalidOperationException("Browser timeout settings must be greater than zero.");

            IWebDriver newDriver = browserType.Trim().ToLowerInvariant() switch
            {
                "chrome" => OpenChrome(),
                "firefox" => OpenFirefox(),
                "edge" => OpenEdge(),
                _ => throw new ArgumentException(
                    $"Unsupported browser: '{browserType}'. Valid options: chrome, firefox, edge.",
                    nameof(browserType))
            };

            try
            {
                var timeouts = newDriver.Manage().Timeouts();
                timeouts.ImplicitWait = TimeSpan.Zero;
                timeouts.PageLoad = TimeSpan.FromSeconds(DriverSetting.PageLoadTimeoutSeconds);
                timeouts.AsynchronousJavaScript = TimeSpan.FromSeconds(DriverSetting.DefaultTimeoutSeconds);
                if (!DriverSetting.Headless)
                    newDriver.Manage().Window.Maximize();

                driver = newDriver;
                string message = $"OpenBrowser launched successfully | Browser: {browserType} | Headless: {DriverSetting.Headless}";
                Console.WriteLine(message);
                Log.Info(message);
                ExtentReporter.NodeInfo(message);
                return driver;
            }
            catch
            {
                newDriver.Quit();
                throw;
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

                url = url.Trim();
                if (!url.Contains("://", StringComparison.Ordinal))
                    url = "https://" + url;

                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri targetUri)
                    || (targetUri.Scheme != Uri.UriSchemeHttp && targetUri.Scheme != Uri.UriSchemeHttps))
                    throw new ArgumentException("URL must be a valid HTTP or HTTPS address.", nameof(url));

                driver.Navigate().GoToUrl(targetUri);

                // Wait until page is fully loaded
                WaitForDocumentReady();

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
                WaitForDocumentReady();

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
                WaitForDocumentReady();

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
                WaitForDocumentReady();

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

        private static void WaitForDocumentReady()
        {
            var wait = new WebDriverWait(
                driver,
                TimeSpan.FromSeconds(DriverSetting.PageLoadTimeoutSeconds));
            wait.Until(currentDriver =>
                string.Equals(
                    ((IJavaScriptExecutor)currentDriver).ExecuteScript("return document.readyState")?.ToString(),
                    "complete",
                    StringComparison.Ordinal));
        }

        internal static void MinimizeBrowser()
        {
            EnsureBrowserOpen(nameof(MinimizeBrowser));
            driver.Manage().Window.Minimize();
        }

        internal static void FullScreenBrowser()
        {
            EnsureBrowserOpen(nameof(FullScreenBrowser));
            driver.Manage().Window.FullScreen();
        }

        internal static void SwitchToWindow(string windowIdentifier)
        {
            EnsureBrowserOpen(nameof(SwitchToWindow));
            if (string.IsNullOrWhiteSpace(windowIdentifier))
                throw new ArgumentException("Window handle or title cannot be empty.", nameof(windowIdentifier));

            var handles = driver.WindowHandles;
            string matchingHandle = handles.FirstOrDefault(handle =>
                string.Equals(handle, windowIdentifier, StringComparison.Ordinal));

            if (matchingHandle != null)
            {
                driver.SwitchTo().Window(matchingHandle);
                return;
            }

            string originalHandle = driver.CurrentWindowHandle;
            foreach (string handle in handles)
            {
                driver.SwitchTo().Window(handle);
                if (string.Equals(driver.Title, windowIdentifier, StringComparison.Ordinal))
                    return;
            }

            driver.SwitchTo().Window(originalHandle);
            throw new NoSuchWindowException($"No open window has handle or title '{windowIdentifier}'.");
        }

        internal static void SwitchToFrame(By frameLocator)
        {
            EnsureBrowserOpen(nameof(SwitchToFrame));
            driver.SwitchTo().Frame(driver.FindElement(frameLocator));
        }

        internal static void SwitchToDefaultContent()
        {
            EnsureBrowserOpen(nameof(SwitchToDefaultContent));
            driver.SwitchTo().DefaultContent();
        }

        internal static void CloseBrowserWindow()
        {
            EnsureBrowserOpen(nameof(CloseBrowserWindow));
            if (driver.WindowHandles.Count <= 1)
            {
                CloseAllBrowsers();
                return;
            }

            driver.Close();
            driver.SwitchTo().Window(driver.WindowHandles.First());
        }

        internal static void CloseAllBrowsers()
        {
            if (driver == null)
                return;

            IWebDriver currentDriver = driver;
            driver = null;
            currentDriver.Quit();
        }

        private static void EnsureBrowserOpen(string operation)
        {
            if (driver == null)
                throw new InvalidOperationException($"Browser is not open. Call OpenBrowser before {operation}.");
        }

        internal static void ExecuteDriverKeyword(string keyword, string details, Action action, bool preserveExistingOutcome = false)
        {
            string message = string.IsNullOrWhiteSpace(details) ? keyword : $"{keyword} | {details}";
            Log.Info(message);
            ExtentReporter.NodeInfo(message);

            try
            {
                action();
                if (!preserveExistingOutcome || DriverScript.outcome == (int)Outcome.Pass)
                    DriverScript.outcome = (int)Outcome.Pass;
            }
            catch (Exception exception)
            {
                string errorMessage = $"{keyword} failed | {exception.Message}";
                Log.Error(errorMessage);
                ExtentReporter.NodeError(errorMessage);
                DriverScript.outcome = (int)Outcome.Error;
            }
        }

    }

    public partial class Keywords
    {
        #region Keywords methods
       
        public static void OpenBrowser(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(OpenBrowser), $"Browser: {data}", () => Drivers.OpenBrowser(data));
        }

        public static void CloseBrowser(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(
                nameof(CloseBrowser),
                "Close current browser window",
                Drivers.CloseBrowserWindow,
                preserveExistingOutcome: true);
        }

        public static void CloseAllBrowser(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(CloseAllBrowser), "Close all browser windows", Drivers.CloseAllBrowsers);
        }

        public static void MaximizeBrowser(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(MaximizeBrowser), string.Empty, Drivers.MaximizeBrowser);
        }

        public static void MinimizeBrowser(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(MinimizeBrowser), string.Empty, Drivers.MinimizeBrowser);
        }

        public static void FullScreenBrowser(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(FullScreenBrowser), string.Empty, Drivers.FullScreenBrowser);
        }

        public static void NavigateToURL(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(NavigateToURL), $"URL: {data}", () => Drivers.NavigateToUrl(data));
        }

        public static void RefreshPage(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(RefreshPage), string.Empty, Drivers.RefreshPage);
        }

        public static void NavigateBack(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(NavigateBack), string.Empty, Drivers.NavigateBack);
        }

        public static void NavigateForward(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(NavigateForward), string.Empty, Drivers.NavigateForward);
        }

        public static void SetWindowSize(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(SetWindowSize), $"Size: {data}", () =>
            {
                string[] dimensions = data?.Split(',');
                if (dimensions == null || dimensions.Length != 2
                    || !int.TryParse(dimensions[0].Trim(), out int width)
                    || !int.TryParse(dimensions[1].Trim(), out int height))
                    throw new ArgumentException("Window size must be provided as width,height (for example 1280,720).", nameof(data));

                Drivers.SetWindowSize(width, height);
            });
        }

        public static void SwitchToWindow(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(SwitchToWindow), $"Window: {data}", () => Drivers.SwitchToWindow(data));
        }

        public static void SwitchToFrame(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(SwitchToFrame), $"Frame locator: {obj}", () => Drivers.SwitchToFrame(Locators.GetLocator(obj)));
        }

        public static void SwitchToDefaultContent(string obj, string data)
        {
            Drivers.ExecuteDriverKeyword(nameof(SwitchToDefaultContent), string.Empty, Drivers.SwitchToDefaultContent);
        }

        #endregion
    }
}
