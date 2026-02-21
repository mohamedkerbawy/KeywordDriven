using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using KeywordDriven.Utils;
using KeywordDriven.Execution;

namespace KeywordDriven.ActionKeywords
{
    internal partial class Keywords
    {
        private static string GetElementTextByTypeDriver(By by, String type)
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

        private static string GetElementTextByTypeappiumDriver(By by, String type)
        {
            String txt = "";
            switch (type)
            {
                case "text":
                    txt = appiumdriver.FindElement(by).Text;
                    break;
                case "textContent":
                    txt = appiumdriver.FindElement(by).GetAttribute("textContent");
                    break;
                case "value":
                    txt = appiumdriver.FindElement(by).GetAttribute("value");
                    break;
                default:
                    break;
            }
            return txt;
        }

        private static string GetTextByDriver(By by)
        {
            string elementTxt = "";
            string[] types = { "text", "textContent", "value" };
            for (int i = 0; i < types.Length; i++)
            {
                if (!GetElementTextByTypeDriver(by, types[i]).Trim().Equals(""))
                {
                    elementTxt = GetElementTextByTypeDriver(by, types[i]).Trim();
                    break;
                }
            }
            Console.WriteLine(elementTxt);
            return elementTxt;
        }

        private static string GetTextByappiumDriver(By by)
        {
            string elementTxt = "";
            string[] types = { "text", "textContent", "value" };
            for (int i = 0; i < types.Length; i++)
            {
                if (!GetElementTextByTypeappiumDriver(by, types[i]).Trim().Equals(""))
                {
                    elementTxt = GetElementTextByTypeappiumDriver(by, types[i]).Trim();
                    break;
                }
            }
            Console.WriteLine(elementTxt);
            return elementTxt;
        }

        #region Public methods
        public static void AssertTextPresent(String obj, String data)
        {
            Log.Info($"AssertTextPresent \"{data}\", Element \"{obj}\"");
            ExtentReporter.NodeInfo($"AssertTextPresent \"{data}\", Element \"{obj}\"");
            try
            {
                string[] locator = obj.Split('_');
                By by = LocateValue(locator[1], GetKey(obj));

                if (locator[0] == "Mobile")
                {
                    WaitUntilVisible(by, appiumdriver);

                    bool result = GetTextByappiumDriver(by).Contains(data);

                    Assert.AreEqual(result, true);

                    DriverScript.iOutcome = 1;
                    WaitSeconds("", "2");
                }
                else
                {
                    WaitUntilVisible(by, driver);

                    bool result = GetTextByDriver(by).Contains(data);

                    Assert.AreEqual(result, true);

                    DriverScript.iOutcome = 1;
                    WaitSeconds("", "2");
                }
            }
            catch (AssertFailedException e)
            {
                Log.Info($"AssertTextPresent Fail| Exception: {e.Message}");
                ExtentReporter.NodeFail("AssertTextPresent Fail| Exception" + e.Message.Replace('<','\"').Replace('>', '\"'));
                DriverScript.iOutcome = 2;
                DriverScript.bOutcomeFail = true;
            }
            catch (Exception e)
            {
                Log.Error($"Failed AssertTextPresent | Exception: {e.Message}");
                ExtentReporter.NodeError($"Failed AssertTextPresent | Exception: {e.Message}");
                DriverScript.iOutcome = 3;
                DriverScript.bOutcomeError = true;
            }
        }

        public static void AssertTextNotPresent(String obj, String data)
        {
            Log.Info($"AssertTextNotPresent \"{data}\", Element \"{obj}\"");
            ExtentReporter.NodeInfo($"AssertTextNotPresent \"{data}\", Element \"{obj}\"");
            try
            {
                string[] locator = obj.Split('_');
                By by = LocateValue(locator[1], GetKey(obj));

                if (locator[0] == "Mobile")
                {
                    WaitUntilVisible(by, appiumdriver);

                    bool result = GetTextByappiumDriver(by).Contains(data);

                    Assert.AreNotEqual(result, true);

                    DriverScript.iOutcome = 1;
                    WaitSeconds("", "2");
                }
                else
                {
                    WaitUntilVisible(by, driver);

                    bool result = GetTextByDriver(by).Contains(data);

                    Assert.AreEqual(result, true);

                    DriverScript.iOutcome = 1;
                    WaitSeconds("", "2");
                }
            }
            catch (AssertFailedException e)
            {
                Log.Info($"AssertTextNotPresent Fail| Exception: {e.Message}");
                ExtentReporter.NodeFail("AssertTextNotPresent Fail| Exception" + e.Message.Replace('<', '\"').Replace('>', '\"'));
                DriverScript.iOutcome = 2;
                DriverScript.bOutcomeFail = true;
            }
            catch (Exception e)
            {
                Log.Error($"Failed AssertTextNotPresent | Exception: {e.Message}");
                ExtentReporter.NodeError($"Failed AssertTextNotPresent | Exception: {e.Message}");
                DriverScript.iOutcome = 3;
                DriverScript.bOutcomeError = true;
            }
        }

        public static void AssertValue(String obj, String data)
        {

        }

        public static void AssertNotValue(String obj, String data)
        {

        }

        public static void AssetElementPresent(String obj, String data)
        {

        }
        
        public static void AssetElementNotPresent(String obj, String data)
        {

        }

        public static void AssetChecked(String obj, String data)
        {

        }

        public static void AssetNotChecked(String obj, String data)
        {

        }

        public static void AssertSelectedOption(String obj, String data)
        { }
        
        public static void AssertNotSelectedOption(String obj, String data)
        { 

        }

        public static void AssertSelectedValue(String obj, String data)
        { 

        }

        public static void AssertNotSelectedValue(String obj, String data)
        { 

        }

        public static void AssertSelectedIndex(String obj, String data)
        {

        }

        public static void AssertNotSelectedIndex(String obj, String data)
        { 

        }

        public static void AssetEditable(String obj, String data)
        {

        }

        public static void AssetNotEditable(String obj, String data)
        {

        }

        public static void AssertURLPresent(String obj, String data)
        {

        }

        public static void AssertURLContain(String obj, String data)
        {

        }

        public static void AssetFilePresent(String obj, String data)
        {
            
        }
        
        #endregion
    }
}
