using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using KeywordDriven.Utils;
using KeywordDriven.Config;
using KeywordDriven.Execution;

namespace KeywordDriven.ActionKeywords
{
    internal partial class Keywords
    {
        private static void WaitUntil(By by, IWebDriver driver)
        {
            try
            {
                Log.Info("WaitUntil ..");
                ExtentReporter.NodeInfo("WaitUntil ..");

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DriverSetting._timeout));
                wait.Until(d => d.FindElement(by));
            }
            catch (Exception e)
            {
                Log.Error("Failed WaitUntil | Exception: " + e.Message);
            }
        }
        
        private static void WaitFluentUntil(By by, IWebDriver driver)
        {
            try
            {
                Log.Info("WaitFluentUntil ..");
                ExtentReporter.NodeInfo("WaitFluentUntil ..");

                DefaultWait<IWebDriver> fluentWait = new DefaultWait<IWebDriver>(driver);
                fluentWait.Timeout = TimeSpan.FromSeconds(DriverSetting._timeout);
                fluentWait.PollingInterval = TimeSpan.FromSeconds(1);
                fluentWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                IWebElement searchResult = fluentWait.Until(x => x.FindElement(by));
            }
            catch (Exception e)
            {
                Log.Error("Failed WaitFluentUntil | Exception: " + e.Message);
                ExtentReporter.NodeError("Failed WaitFluentUntil | Exception: " + e.Message);
            }
        }
        
        private static void WaitUntilClickable(By by, IWebDriver driver)
        {
            try
            {
                Log.Info("WaitUntilClickable ..");
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DriverSetting._timeout));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
            }
            catch (Exception e)
            {
                Log.Error("Failed WaitUntilClickable | Exception: " + e.Message);
                ExtentReporter.NodeInfo("Failed WaitUntilClickable | Exception: " + e.Message);
            }
        }

        private static void WaitUntilExists(By by, IWebDriver driver)
        {
            try
            {
                Log.Info("WaitUntilExists ..");
                ExtentReporter.NodeInfo("WaitUntilExists ..");
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DriverSetting._timeout));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));
            }
            catch (Exception e)
            {
                Log.Error("Failed WaitUntilExists | Exception: " + e.Message);
                ExtentReporter.NodeInfo("Failed WaitUntilExists | Exception: " + e.Message);
            }
        }

        private static void WaitUntilVisible(By by, IWebDriver driver)
        {
            try
            {
                Log.Info("WaitUntilVisible ..");
                ExtentReporter.NodeInfo("WaitUntilVisible ..");

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DriverSetting._timeout));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
            }
            catch (Exception e)
            {
                Log.Error("Failed WaitUntilVisible | Exception: " + e.Message);
                ExtentReporter.NodeError("Failed WaitUntilVisible | Exception: " + e.Message);
            }
        }

        private static void WaitUntilUrlContains(String data, IWebDriver driver)
        {
            try
            {
                Log.Info("WaitUntilUrlContains .." + data);
                ExtentReporter.NodeInfo("WaitUntilUrlContains ..");
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DriverSetting._navigationtimeout));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.UrlContains(data));
            }
            catch (Exception e)
            {
                Log.Error("Failed WaitUntilUrlContains | Exception: " + e.Message);
                ExtentReporter.NodeError("Failed WaitUntilUrlContains | Exception: " + e.Message);
            }
        }

        private static void WaitUntilInvisibilityElement(By by, IWebDriver driver)
        {
            try
            {
                Log.Info("WaitUntilInvisibilityElement ..");
                ExtentReporter.NodeInfo("WaitUntilInvisibilityElement ..");
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DriverSetting._navigationtimeout));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(by));
            }
            catch (Exception e)
            {
                Log.Error("Failed WaitUntilInvisibilityElement | Exception: " + e.Message);
                ExtentReporter.NodeError("Failed WaitUntilInvisibilityElement | Exception: " + e.Message);
            }
        }

        #region Public methods
        public static void WaitSeconds(String obj, String data)
        {
            try
            {
                int millisec = Convert.ToInt32(data) * 1000;
                Log.Info($"Waiting \"{data}\" seconds");
                ExtentReporter.NodeInfo($"Waiting \"{data}\" seconds");

                Thread.Sleep(millisec);

                DriverScript.iOutcome = 1;
            }
            catch (Exception e)
            {
                Log.Error($"Failed WaitSeconds | Exception: {e.Message}");
                ExtentReporter.NodeInfo($"Failed WaitSeconds | Exception: {e.Message}");
                DriverScript.iOutcome = 3;
            }
        }
        #endregion
    }
}
