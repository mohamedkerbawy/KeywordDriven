using System;
using System.IO;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using KeywordDriven.Config;
using KeywordDriven.Utils;
using KeywordDriven.Execution; /////SHOULD REMOVED/////

namespace KeywordDriven.ActionKeywords
{
    internal static class Drivers
    {
        internal static IWebDriver driver;

        internal static FirefoxDriver BuildFirefoxDriver()
        {
            var options = new FirefoxOptions();
            if (DriverSetting.Headless)
            {
                options.AddArgument("-headless");
            }
            options.AddArgument("-foreground");
            return new FirefoxDriver(FirefoxDriverService.CreateDefaultService(), options, TimeSpan.FromSeconds(DriverSetting.PageLoadTimeoutSeconds));
        }

        internal static ChromeDriver BuildChromeDriver()
        {
            var options = new ChromeOptionsWithPrefs();
            options.AddArguments("--start-maximized");
            options.AddArgument("--enable-automation");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-browser-side-navigation");
            options.AddArgument("--disable-gpu");

            options.AddArgument("--ignore-ssl-errors=yes");
            options.AddArgument("--ignore-certificate-errors");

            if (DriverSetting.Headless)
            {
                options.AddArguments("--headless");
            }

            options.prefs = new Dictionary<string, object>
                    {
                        { "profile.default_content_settings.popups", 0 },
                        { "download.prompt_for_download","false" }
                    };

            return new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromSeconds(DriverSetting.PageLoadTimeoutSeconds));
        }

        internal static RemoteWebDriver BuildRemoteDriver(string browser)
        {
            var DOCKER_GRID_HUB_URI = new Uri("http://localhost:4444/wd/hub");

            RemoteWebDriver driver;

            switch (browser)
            {
                case "chrome":
                    var chromeOptions = new ChromeOptions
                    {
                        BrowserVersion = "",
                        PlatformName = "LINUX",
                    };

                    chromeOptions.AddArgument("--start-maximized");

                    driver = new RemoteWebDriver(DOCKER_GRID_HUB_URI, chromeOptions.ToCapabilities());
                    break;

                case "firefox":
                    var firefoxOptions = new FirefoxOptions
                    {
                        BrowserVersion = "",
                        PlatformName = "LINUX",
                    };

                    driver = new RemoteWebDriver(DOCKER_GRID_HUB_URI, firefoxOptions.ToCapabilities());
                    break;

                default:
                    throw new ArgumentException($"{browser} is not supported remotely.");
            }

            return driver;
        }

        internal static IWebDriver BuildDriver(string type, string browser)
        {
            if (type == "local")
            {
                switch (browser)
                {
                    case "Chrome":
                        return BuildChromeDriver();
                    case "Firefox":
                        return BuildFirefoxDriver();
                    default:
                        throw new ArgumentException($"{browser} is not supported locally.");
                }
            }
            else if (type == "remote")
            {
                return BuildRemoteDriver(browser);
            }
            else
            {
                throw new ArgumentException($"{DriverSetting.DriverType} is invalid. Choose 'local' or 'remote'.");
            }

        }

        internal class ChromeOptionsWithPrefs : ChromeOptions
        {
            public Dictionary<string, object> prefs { get; set; }
        }

        internal static void TakeScreenshot(string directory, string imgName)
        {
            var ss = ((ITakesScreenshot)driver).GetScreenshot();
            var ssFileName = Path.Combine(directory, imgName);
            ss.SaveAsFile($"{ssFileName}.png");
        }

        internal static void TakeScreenshot(string imgName)
        {
            var ss = ((ITakesScreenshot)driver).GetScreenshot();
            var ssFileName = Path.Combine("", imgName);
            ss.SaveAsFile($"{ssFileName}.png");
        }
    }
    
    public partial class Keywords
    {
        #region Keywords methods
        public static void OpenBrowser(String obj, String data)
        {
            Log.Info($"Opening Browser \"{data}\"");
            ExtentReporter.NodeInfo($"Opening Browser \"{data}\"");
            try
            {
                Drivers.driver = Drivers.BuildDriver(DriverSetting.DriverType, data);

                DriverScript.outcome = 1;

                Log.Info($"Browser {data} Opened");
                ExtentReporter.NodeInfo($"Browser {data} Opened");
            }
            catch (Exception e)
            {
                Log.Error($"Not able to OpenBrowser | Exception: {e.Message}");
                ExtentReporter.NodeError($"Not able to OpenBrowser | Exception: {e.Message}");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void CloseBrowser(String obj, String data)
        {
            try
            {
                if (Drivers.driver != null)
                {
                    Log.Info("Closing Browser ");
                    ExtentReporter.NodeInfo("Closing Browser ");
                    //driver.Quit();
                    Drivers.driver.Close();

                    DriverScript.outcome = 1;

                    Log.Info("Browser Closed");
                    ExtentReporter.NodeInfo("Browser Closed");
                }
            }
            catch (Exception e)
            {
                Log.Error("Not able to CloseBrowser | Exception: " + e.Message);
                ExtentReporter.NodeError("Not able to CloseBrowser | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void RefreshBrowser(String obj, String data)
        {
            try
            {
                Log.Info($"Refreshing Browser");
                ExtentReporter.NodeInfo($"Refreshing Browser");

                Drivers.driver.Navigate().Refresh();

                DriverScript.outcome = 1;
            }
            catch (Exception e)
            {
                Log.Error($"Not able to RefreshBrowser | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Not able to RefreshBrowser | Exception: {e.Message}");
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void NavigateToURL(String obj, String data)
        {
            try
            {
                Log.Info($"Navigating to URL \"{data}\"");
                ExtentReporter.NodeInfo($"Navigating to URL \"{data}\"");

                ((IJavaScriptExecutor)Drivers.driver).ExecuteScript("return window.stop;");

                string currentURL = Drivers.driver.Url;
                if (!currentURL.Equals(data))
                {
                    Drivers.driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(DriverSetting.PageLoadTimeoutSeconds);
                    //driver.Url = data;
                    Drivers.driver.Navigate().GoToUrl(data);

                    DriverScript.outcome = 1;
                }
                else
                {
                    Drivers.driver.Navigate().Refresh();

                    DriverScript.outcome = 1;
                }

            }
            catch (Exception e)
            {
                Log.Error("Not able to NavigateToURL | Exception: " + e.Message);
                ExtentReporter.NodeInfo("Not able to NavigateToURL | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void NavigateBack(String obj, String data)
        {
            try
            {
                Log.Info($"Navigating Back ");
                ExtentReporter.NodeInfo($"Navigating Back ");

                ((IJavaScriptExecutor)Drivers.driver).ExecuteScript("return window.stop;");

                Drivers.driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(DriverSetting.PageLoadTimeoutSeconds);
                Drivers.driver.Navigate().Back();

                DriverScript.outcome = 1;

            }
            catch (Exception e)
            {
                Log.Error("Not able to NavigateBack | Exception: " + e.Message);
                ExtentReporter.NodeInfo("Not able to NavigateBack | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        public static void NavigateForward(String obj, String data)
        {
            try
            {
                Log.Info($"Navigating Forward ");
                ExtentReporter.NodeInfo($"Navigating Forward ");

                ((IJavaScriptExecutor)Drivers.driver).ExecuteScript("return window.stop;");

                Drivers.driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(DriverSetting.PageLoadTimeoutSeconds);
                Drivers.driver.Navigate().Forward();

                DriverScript.outcome = 1;

            }
            catch (Exception e)
            {
                Log.Error("Not able to NavigateForward | Exception: " + e.Message);
                ExtentReporter.NodeInfo("Not able to NavigateForward | Exception: " + e.Message);
                DriverScript.outcome = (int) Outcome.Error;
            }
        }

        #endregion
    }
}
