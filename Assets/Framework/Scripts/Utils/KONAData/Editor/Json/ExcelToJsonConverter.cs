using System.IO;
using System.Linq;

using UnityEditor;

using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

namespace KONA.Data.Converter.Json
{
	using KONA.Data.Editor;
	using UnityEngine;

	public class ExcelToJsonConverter : ExcelConverter<string>
	{
		public string OutputPath { get; private set; }
		public bool OnlyModifiedFiles { get; private set; }

		public ExcelToJsonConverter(string outputPath, bool onlyModifiedFiles = false)
		{
			OutputPath = outputPath;
			OnlyModifiedFiles = onlyModifiedFiles;
		}

		protected override bool IsConvertible(string filePath, string sheetName)
		{
			if (!OnlyModifiedFiles || IsModifiedFile(filePath, sheetName, OutputPath))
				return base.IsConvertible(filePath, sheetName);
			return false;
		}


		protected override bool ConvertSheet(ISheet sheet, out string jsonText)
		{
			ExcelJsonScheme scheme = new ExcelJsonSchemeParser(sheet).ParseAndGetScheme();
			ExcelJsonGenerator generator = new ExcelJsonGenerator(scheme);
			jsonText = generator.Generate();

			return !string.IsNullOrEmpty(jsonText);
		}

		protected override void WriteSheet(string jsonText, string sheetName)
		{
			var outputJsonFilePath = Path.Combine(OutputPath, $"{sheetName}.json");
			File.WriteAllText(outputJsonFilePath, jsonText);
			
			var assetIdx = outputJsonFilePath.IndexOf("Assets");
			if (assetIdx >= 0)
			{
				AssetDatabase.ImportAsset(outputJsonFilePath.Substring(assetIdx));
			}
		}

		bool IsModifiedFile(string excelFile, string sheetName, string outputDirectory)
		{
			string outputFile = $"{outputDirectory}/{sheetName}.json";
			if (!File.Exists(outputFile))
				return true;
			if (File.GetLastWriteTimeUtc(excelFile) > File.GetLastWriteTimeUtc(outputFile))
				return true;

			return false;
		}

		//[MenuItem("Assets/BFGame/Convert Excel To Json")]
		public static void ConvertToLiteDB()
		{
			var excelFilePaths = Selection.assetGUIDs.Select(AssetDatabase.GUIDToAssetPath)
				.Select(Path.GetFullPath).ToList();
			foreach (var filePath in excelFilePaths)
			{
				var outputPath = Path.GetDirectoryName(filePath);
				new ExcelToJsonConverter(outputPath).ConvertWorkbook(filePath);

				Debug.Log(filePath);
				Debug.Log(outputPath);
			}
		}

		[MenuItem("Assets/BFGame/Convert Excel To Json", true)]
		public static bool ConvertToLiteDBMenuValidation()
		{
			return Selection.assetGUIDs.Select(AssetDatabase.GUIDToAssetPath).All(path => path.EndsWith(".xlsx"));
		}

		[MenuItem("BFGame/Export Data")]
		public static void ExportData()
		{
			var filePath = Application.dataPath.Replace("Assets", "GameDataExcel");
			filePath = EditorUtility.OpenFilePanel("Export Data", filePath, "xlsx");
			if (string.IsNullOrWhiteSpace(filePath))
				return;

			var outputPath = Application.dataPath + "/GameContents/Resources/Data/GameDatas";
			new ExcelToJsonConverter(outputPath).ConvertWorkbook(filePath);
		}

		[MenuItem("BFGame/Export All Data")]
		public static void ExportAllData()
		{
			var filePaths = Directory.GetFiles(Application.dataPath.Replace("Assets", "GameDataExcel"));
			foreach (var filePath in filePaths)
			{
				var outputPath = Application.dataPath + "/GameContents/Resources/Data/GameDatas";
				new ExcelToJsonConverter(outputPath).ConvertWorkbook(filePath);
			}
		}
	}
}