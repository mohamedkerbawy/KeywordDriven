using ClosedXML.Excel;
using KeywordDriven.Config;
using System;
using System.IO;

namespace KeywordDriven.Utils
{
    public class ExcelManager
    {
        private static XLWorkbook excelWorkbook;
        private static string workbookPath;

        public static void SetExcel(string path, string fileName)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Excel directory cannot be empty.", nameof(path));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Excel file name cannot be empty.", nameof(fileName));

            string normalizedFileName = Path.HasExtension(fileName)
                ? fileName
                : $"{fileName}.xlsx";
            string fullPath = Path.GetFullPath(Path.Combine(path, normalizedFileName));

            if (excelWorkbook != null
                && string.Equals(workbookPath, fullPath, StringComparison.OrdinalIgnoreCase))
                return;

            if (excelWorkbook != null)
                SaveCloseExcel();

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Excel workbook was not found: {fullPath}", fullPath);

            excelWorkbook = new XLWorkbook(fullPath);
            workbookPath = fullPath;
        }

        public static void SaveCloseExcel()
        {
            if (excelWorkbook == null)
                return;

            try
            {
                excelWorkbook.Save();
            }
            finally
            {
                CloseExcel();
            }
        }

        public static void CloseExcel()
        {
            excelWorkbook?.Dispose();
            excelWorkbook = null;
            workbookPath = null;
        }

        internal static string GetCellData(int rowNum, int colNum, string sheetName)
        {
            IXLWorksheet worksheet = GetWorksheet(sheetName);
            return worksheet.Cell(rowNum + 1, colNum + 1).GetFormattedString();
        }

        internal static int GetRowCount(string sheetName)
        {
            return GetWorksheet(sheetName).LastRowUsed()?.RowNumber() ?? 0;
        }

        internal static int GetRowContains(string testCaseName, int colNum, string sheetName)
        {
            int rowCount = GetRowCount(sheetName);
            for (int rowNum = 0; rowNum < rowCount; rowNum++)
            {
                if (string.Equals(
                    GetCellData(rowNum, colNum, sheetName),
                    testCaseName,
                    StringComparison.Ordinal))
                    return rowNum;
            }

            return rowCount;
        }

        internal static string GetKeyValue(string keyName, int colNum, string sheetName)
        {
            int rowCount = GetRowCount(sheetName);
            for (int rowNum = 0; rowNum < rowCount; rowNum++)
            {
                if (string.Equals(
                    GetCellData(rowNum, colNum, sheetName),
                    keyName,
                    StringComparison.Ordinal))
                    return GetCellData(rowNum, colNum + 1, sheetName);
            }

            return string.Empty;
        }

        internal static int GetTestStepsCount(string sheetName, string testCaseID, int testCaseStart)
        {
            int rowCount = GetRowCount(sheetName);
            for (int rowNum = testCaseStart; rowNum <= rowCount; rowNum++)
            {
                if (!string.Equals(
                    testCaseID,
                    GetCellData(rowNum, ExcelSetting.Col_TestSteps_TestCaseID, sheetName),
                    StringComparison.Ordinal))
                    return rowNum;
            }

            return rowCount;
        }

        internal static void SetCellData(string result, int rowNum, int colNum, string sheetName)
        {
            GetWorksheet(sheetName).Cell(rowNum + 1, colNum + 1).Value = result ?? string.Empty;
        }

        private static IXLWorksheet GetWorksheet(string sheetName)
        {
            if (excelWorkbook == null)
                throw new InvalidOperationException("No Excel workbook is open. Call SetExcel first.");
            if (string.IsNullOrWhiteSpace(sheetName))
                throw new ArgumentException("Worksheet name cannot be empty.", nameof(sheetName));

            return excelWorkbook.Worksheet(sheetName);
        }
    }
}
