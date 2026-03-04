using KeywordDriven.Config;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace KeywordDriven.Utils
{
    public class ExcelManager
    {
        private static Excel.Application ExcelApp;
        private static Excel.Workbook ExcelWBook;
        private static Excel.Worksheet ExcelWSheet;

        public static void SetExcel(string path,string fileName)
        {
            Excel.Application excelApp = null;
            Excel.Workbook excelWBook = null;

            try
            {
                if (ExcelApp == null)
                    ExcelApp = new Excel.Application { Visible = false };

                // Check if the specific workbook is already open
                bool isAlreadyOpen = false;
                if (excelApp.Workbooks.Count > 0)
                {
                    foreach (Excel.Workbook wb in excelApp.Workbooks)
                    {
                        // Compare full paths to ensure it's the exact same file
                        if (string.Equals(wb.FullName, Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase))
                        {
                            excelWBook = wb;
                            isAlreadyOpen = true;
                            Console.WriteLine("Workbook is already open. Using existing instance.");
                            break;
                        }
                    }
                }

                // If not open, open it now
                if (!isAlreadyOpen)
                {
                    excelWBook = excelApp.Workbooks.Open(Path.Combine(path, fileName + ".xlsx"));
                }

                ExcelApp = excelApp;
                ExcelWBook = excelWBook;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error handling Excel file: {ex.Message}");
            }
        }

        public static void SaveCloseExcel()
        {
            ExcelWBook.Save();
            CloseExcel();
        }

        public static void CloseExcel()
        {
            if (ExcelWBook != null)
            {
                ExcelWBook.Close(false);
            }
            if (ExcelApp != null)
            {
                ExcelApp.Quit();
            }

            // Force Garbage Collection to ensure the process actually exits
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        internal static string GetCellData(int rowNum, int colNum, String sheetName)
        {
            ExcelWSheet = ExcelWBook.Sheets[sheetName] as Excel.Worksheet;
            string cellValue = (ExcelWSheet.Cells[rowNum + 1, colNum + 1] as Excel.Range).Text as string;
            return cellValue;
        }

        internal static int GetRowCount(String sheetName)
        {
            int number;
            ExcelWSheet = ExcelWBook.Sheets[sheetName] as Excel.Worksheet;
            number = ExcelWSheet.UsedRange.Rows.Count + 1;
            return number;
        }

        internal static int GetRowContains(String testCaseName, int colNum, String sheetName)
        {
            int rowNum = 0;
            ExcelWSheet = ExcelWBook.Sheets[sheetName] as Excel.Worksheet;
            int rowCount = GetRowCount(sheetName);

            for (; rowNum < rowCount; rowNum++)
            {
                if (GetCellData(rowNum, colNum, sheetName).Equals(testCaseName))
                {
                    break;
                }
            }
            return rowNum;
        }

        internal static string GetKeyValue(String KeyName, int colNum, String sheetName)
        {
            int rowNum = 0;
            ExcelWSheet = ExcelWBook.Sheets[sheetName] as Excel.Worksheet;
            int rowCount = GetRowCount(sheetName);

            for (; rowNum < rowCount; rowNum++)
            {
                if (GetCellData(rowNum, colNum, sheetName).Equals(KeyName))
                {
                    break;
                }
            }
            return GetCellData(rowNum, colNum + 1, sheetName);
        }

        internal static int GetTestStepsCount(String sheetName, String testCaseID, int testCaseStart)
        {
            for (int i = testCaseStart; i <= ExcelManager.GetRowCount(sheetName); i++)
            {
                if (!testCaseID.Equals(ExcelManager.GetCellData(i, ExcelSetting.Col_TestSteps_TestCaseID, sheetName)))
                {
                    int number = i;
                    return number;
                }
            }
            ExcelWSheet = ExcelWBook.Sheets[sheetName] as Excel.Worksheet;
            int number1 = ExcelWSheet.UsedRange.Rows.Count + 1;
            return number1;
        }

        internal static void SetCellData(String Result, int rowNum, int colNum, String sheetName)
        {
            ExcelWSheet = ExcelWBook.Sheets[sheetName] as Excel.Worksheet;
            (ExcelWSheet.Cells[rowNum + 1, colNum + 1] as Excel.Range).Value = Result;
        }
    }
}
