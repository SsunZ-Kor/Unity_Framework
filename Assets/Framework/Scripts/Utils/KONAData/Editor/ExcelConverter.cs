using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text.RegularExpressions;

using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

namespace KONA.Data.Converter
{
	public enum ExcelConvertTarget
	{
		None,
		Json,
		LiteDB,
	}

    public abstract class ExcelConverter<TResult>
    {
        protected ExcelConverter()
        {

		}

		protected virtual bool IsConvertible(string filePath, string sheetName)
		{
			return true;
		}
		protected abstract bool ConvertSheet(ISheet sheet, out TResult data);
		protected abstract void WriteSheet(TResult data, string sheetName);

        public bool ConvertWorkbook(string filePath)
        {
			var sheets = GetSheets(CreateWorkbook(filePath), sheet => sheet.SheetName.StartsWith("!"))
				.GroupBy(sheet => sheet.SheetName).ToList();
			if (sheets.Any(group => group.Count() > 1))
				throw new DuplicatedSheetException(sheets.Where(g => g.Count() > 1).Select(g => g.Key));

			foreach (var sheet in sheets.SelectMany(g => g))
			{
				var sheetName = sheet.SheetName.Replace("!", string.Empty);
				if (IsConvertible(filePath, sheetName))
				{
					if (ConvertSheet(sheet, out var data))
					{
						WriteSheet(data, sheetName);
					}
				}
			}

            return true;
        }

		public bool ConvertAllWorkbooks(string inputPath)
		{
			List<string> excelFiles = GetExcelFileNamesInDirectory(inputPath);

			bool succeded = true;

			foreach (var excelFile in excelFiles)
			{
				if (!ConvertWorkbook(excelFile))
				{
					succeded = false;
					break;
				}
			}

			return succeded;
		}

		private List<string> GetExcelFileNamesInDirectory(string directory)
		{
			string[] directoryFiles = Directory.GetFiles(directory);
			List<string> excelFiles = new List<string>();

			// Regular expression to match against 2 excel file types (xls & xlsx), ignoring
			// files with extension .meta and starting with ~$ (temp file created by excel when fie
			Regex excelRegex = ExcelConverterUtil.ExcelFileNameRegex;

			for (int i = 0; i < directoryFiles.Length; i++)
			{
				string fileName = Path.GetFileName(directoryFiles[i]);
				if (excelRegex.IsMatch(fileName))
				{
					excelFiles.Add(directoryFiles[i]);
				}
			}

			return excelFiles;
		}

		protected IWorkbook CreateWorkbook(FileStream fileStream)
		{
			var fileExt = Path.GetExtension(fileStream.Name);
			if (fileExt.EndsWith("xls"))
			{
				return new HSSFWorkbook(fileStream);
			}

			if (fileExt.EndsWith("xlsx"))
			{
#if UNITY_EDITOR_OSX
				throw new FileLoadException("xlsx is not supported on OSX");
#else
				return new XSSFWorkbook(fileStream);
#endif
			}

			throw new FileLoadException($"Unknown workbook file type: {fileStream.Name}");
		}

		protected IWorkbook CreateWorkbook(string filePath)
		{
			using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				return CreateWorkbook(fileStream);
			}
		}

		protected IEnumerable<ISheet> GetSheets(IWorkbook workbook, Predicate<ISheet> predicate = null)
		{
			for (int i = 0; i < workbook.NumberOfSheets; ++i)
			{
				var sheet = workbook.GetSheetAt(i);
				if (sheet != null && (predicate?.Invoke(sheet) ?? true))
					yield return sheet;
			}
		}
	}
}