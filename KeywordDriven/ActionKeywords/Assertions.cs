using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using KeywordDriven.Utils;
using KeywordDriven.Execution;

namespace KeywordDriven.ActionKeywords
{

    internal static class Assertions
    {
        internal static string GetElementTextByTypeDriver(By by, String type, IWebDriver driver)
        {
            String txt = "";
            switch (type)
            {
                case "text":
                    txt = driver.FindElement(by).Text;
                    break;
                case "textContent":
                    txt = driver.FindElement(by).GetAttribute("textContent");
                    break;
                case "value":
                    txt = driver.FindElement(by).GetAttribute("value");
                    break;
                default:
                    break;
            }
            return txt;
        }

        internal static string GetTextByDriver(By by, IWebDriver driver)
        {
            string elementTxt = "";
            string[] types = { "text", "textContent", "value" };
            for (int i = 0; i < types.Length; i++)
            {
                if (!GetElementTextByTypeDriver(by, types[i], driver).Trim().Equals(""))
                {
                    elementTxt = GetElementTextByTypeDriver(by, types[i], driver).Trim();
                    break;
                }
            }
            Console.WriteLine(elementTxt);
            return elementTxt;
        }
    }

    public partial class Keywords
    {
        #region Keywords methods

        public static void AssertElementExists(String obj, String data)
        {

        }
        
        public static void AssertElementNotExists(String obj, String data)
        {

        }
        
        public static void AssertElementVisible(String obj, String data)
        {

        }
        
        public static void AssertElementNotVisible(String obj, String data)
        {

        }
        
        public static void AssertElementEditable(String obj, String data)
        {

        }
        
        public static void AssertElementNotEditable(String obj, String data)
        {

        }
        
        public static void AssertElementContainsText(String obj, String data)
        {
            Log.Info($"AssertTextPresent \"{data}\", Element \"{obj}\"");
            ExtentReporter.NodeInfo($"AssertTextPresent \"{data}\", Element \"{obj}\"");
            try
            {
                By by = Locators.GetLocator(obj);

                Waits.WaitUntilVisibleByDriver(by, Drivers.driver);

                bool result = Assertions.GetTextByDriver(by, Drivers.driver).Contains(data);

                Assert.AreEqual(result, true);

                DriverScript.outcome = (int)Outcome.Pass;
                WaitSeconds("", "2");

            }
            catch (AssertFailedException e)
            {
                Log.Info($"AssertTextPresent Fail| Exception: {e.Message}");
                ExtentReporter.NodeFail("AssertTextPresent Fail| Exception" + e.Message.Replace('<', '\"').Replace('>', '\"'));
                DriverScript.outcome = (int)Outcome.Fail;
            }
            catch (Exception e)
            {
                Log.Error($"Not able to AssertTextPresent | Exception: {e.Message}");
                ExtentReporter.NodeError($"Not able to AssertTextPresent | Exception: {e.Message}");
                DriverScript.outcome = (int)Outcome.Error;
            }
        }
        
        public static void AssertElementHaveAttribute(String obj, String data)
        {

        }
        
        public static void AssertElementNotContainsText(String obj, String data)
        {
            Log.Info($"AssertTextNotPresent \"{data}\", Element \"{obj}\"");
            ExtentReporter.NodeInfo($"AssertTextNotPresent \"{data}\", Element \"{obj}\"");
            try
            {
                By by = Locators.GetLocator(obj);

                Waits.WaitUntilVisibleByDriver(by, Drivers.driver);

                bool result = Assertions.GetTextByDriver(by, Drivers.driver).Contains(data);

                Assert.AreEqual(result, true);

                DriverScript.outcome = (int) Outcome.Pass;
                WaitSeconds("", "2");

            }
            catch (AssertFailedException e)
            {
                Log.Info($"AssertTextNotPresent Fail| Exception: {e.Message}");
                ExtentReporter.NodeFail("AssertTextNotPresent Fail| Exception" + e.Message.Replace('<', '\"').Replace('>', '\"'));
                DriverScript.outcome = (int)Outcome.Fail;
            }
            catch (Exception e)
            {
                Log.Error($"Not able to AssertTextNotPresent | Exception: {e.Message}");
                ExtentReporter.NodeError($"Not able to AssertTextNotPresent | Exception: {e.Message}");
                DriverScript.outcome = (int)Outcome.Error;
            }
        }
        
        public static void AssertElementHaveText(String obj, String data)
        {

        }
        
        public static void AssertElementNotHaveText(String obj, String data)
        {

        }
        
        public static void AssertSelectedText(String obj, String data)
        {

        }
        
        public static void AssertSelectedValue(String obj, String data)
        {

        }
        
        public static void AssertSelectedIndex(String obj, String data)
        {

        }
        
        public static void AssertCheckboxChecked(String obj, String data)
        {

        }
        
        public static void AssertCheckboxNotChecked(String obj, String data)
        {

        }
        
        public static void AssertRadioSeleced(String obj, String data)
        {

        }
        
        public static void AssertRadioNotSeleced(String obj, String data)
        {

        }
        
        public static void AssertPageContainsTitle(String obj, String data)
        {

        }
        
        public static void AssertPageNotContainsTitle(String obj, String data)
        {

        }
        
        public static void AssertPageHaveTitle(String obj, String data)
        {

        }
        
        public static void AssertPageNotHaveTitle(String obj, String data)
        {

        }
        
        public static void AssertPageContainsUrl(String obj, String data)
        {

        }
        
        public static void AssertPageHaveUrl(String obj, String data)
        {

        }        
        
        #endregion
    }
}
