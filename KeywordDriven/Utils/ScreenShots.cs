using KeywordDriven.Config;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeywordDriven.Utils
{
    public class ScreenShots
    {
        /// <summary>
        /// Takes a screenshot of the entire browser viewport and saves it to disk.
        /// Auto-generates filename with timestamp if no name provided.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="fileName">Custom file name without extension (default: auto timestamp)</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static string TakeScreenshot(IWebDriver driver, string fileName=null)
        {
            try
            {
                if (driver == null)
                    throw new InvalidOperationException(
                        "Browser is not open. Call OpenBrowser before TakeElementScreenshot.");

                // Create folder if it doesn't exist
                if (!Directory.Exists(PathSetting.Path_ScreenShotsDir))
                {
                    Directory.CreateDirectory((PathSetting.Path_ScreenShotsDir));
                    Console.WriteLine($"Created screenshot folder | {PathSetting.Path_ScreenShotsDir}");
                }

                // Auto-generate filename if not provided
                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss_fff}";

                // Sanitize filename — remove invalid characters
                foreach (char c in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(c, '_');

                string fullPath = Path.Combine(PathSetting.Path_ScreenShotsDir, $"{fileName}.png");

                // Cast the Driver to ITakesScreenshot
                ITakesScreenshot screenshotDriver = (ITakesScreenshot)driver;

                // Capture the screenshot
                Screenshot screenshot = screenshotDriver.GetScreenshot();

                // Save to disk
                screenshot.SaveAsFile(fullPath);

                Console.WriteLine($"Screenshot saved to | {fullPath}");

                // Return path
                return fullPath;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"TakeScreenshot | {ex.Message}");
                throw;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"TakeScreenshot | {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"TakeScreenshot failed | {ex.Message}");
            }
        }

        /// <summary>
        /// Takes a screenshot of a specific web element only (crops from full page).
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static string TakeElementScreenshot(IWebDriver driver, By locator, string fileName=null)
        {
            try
            {
                if (driver == null)
                    throw new InvalidOperationException(
                        "Browser is not open. Call OpenBrowser before TakeElementScreenshot.");

                // Create folder if it doesn't exist
                if (!Directory.Exists(PathSetting.Path_ScreenShotsDir))
                {
                    Directory.CreateDirectory(PathSetting.Path_ScreenShotsDir);
                    Console.WriteLine($"Created screenshot folder | {PathSetting.Path_ScreenShotsDir}");
                }

                // Auto-generate filename if not provided
                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss_fff}";

                // Sanitize filename
                foreach (char c in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(c, '_');

                string fullPath = Path.Combine(PathSetting.Path_ScreenShotsDir, $"{fileName}.png");

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
}
