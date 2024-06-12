using UnityEngine;
using UnityEditor;
using System.IO;

namespace KONA.Data.Editor
{
    public class ExcelConverterWindow : EditorWindow
    {
        public static string kExcelToJsonConverterInputPathPrefsName = "ExcelJsonConverter.InputPath";
        public static string kExcelToJsonConverterInputFolderPathPrefsName = "ExcelJsonConverter.InputFolderPath";
        public static string kExcelToJsonConverterOutputFolderPathPrefsName = "ExcelJsonConverter.OutputFolderPath";

        private string inputFilePath;
        private string inputFolderPath;
        private string outputFolderPath;

        [MenuItem("Tools/Excel to Json Converter")]
        public static void ShowWindow()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(ExcelConverterWindow), true, "Excel to Json Converter", true);
            window.maxSize = new Vector2(700f, 200f);
        }

        public void OnEnable()
        {
            inputFilePath = EditorPrefs.GetString(kExcelToJsonConverterInputPathPrefsName, Application.dataPath);
            inputFolderPath = EditorPrefs.GetString(kExcelToJsonConverterInputFolderPathPrefsName, Application.dataPath);
            outputFolderPath = EditorPrefs.GetString(kExcelToJsonConverterOutputFolderPathPrefsName, Application.dataPath);
        }

        public void OnDisable()
        {
            EditorPrefs.SetString(kExcelToJsonConverterInputPathPrefsName, inputFilePath);
            EditorPrefs.SetString(kExcelToJsonConverterInputFolderPathPrefsName, inputFolderPath);
            EditorPrefs.SetString(kExcelToJsonConverterOutputFolderPathPrefsName, outputFolderPath);
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUIContent outputFolderContent = new GUIContent("Output Folder", "Select the folder where the converted json files should be saved.");
            EditorGUIUtility.labelWidth = 80.0f;
            EditorGUILayout.TextField(outputFolderContent, outputFolderPath, GUILayout.MinWidth(120), GUILayout.MaxWidth(500));
            if (GUILayout.Button(new GUIContent("Select Folder"), GUILayout.MinWidth(80), GUILayout.MaxWidth(100)))
            {
                outputFolderPath = EditorUtility.OpenFolderPanel("Select Folder to save json files", outputFolderPath, Application.dataPath);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUIContent inputFileContent = new GUIContent("Input File", "Select the excel file.");
            EditorGUIUtility.labelWidth = 80.0f;
            EditorGUILayout.TextField(inputFileContent, inputFilePath, GUILayout.MinWidth(120), GUILayout.MaxWidth(500));
            if (GUILayout.Button(new GUIContent("Select File"), GUILayout.MinWidth(80), GUILayout.MaxWidth(100)))
            {

                inputFilePath = EditorUtility.OpenFilePanel("Seleect excel file", string.IsNullOrEmpty(inputFilePath) ? inputFilePath : Path.GetDirectoryName(inputFilePath), "xlsx,xls,csv");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginArea(new Rect(Screen.width - (100 + 10), 55, 100, 25));
            if (GUILayout.Button("Convert"))
            {
				new Converter.Json.ExcelToJsonConverter(outputFolderPath).ConvertWorkbook(inputFilePath);
            }
            GUILayout.EndArea();

            GUILayout.Space(30);

            GUILayout.BeginHorizontal();
            GUIContent inputFolderContent = new GUIContent("Input Folder", "Select the folder where the excel files to be processed are located.");
            EditorGUIUtility.labelWidth = 80.0f;
            EditorGUILayout.TextField(inputFolderContent, inputFolderPath, GUILayout.MinWidth(120), GUILayout.MaxWidth(500));
            if (GUILayout.Button(new GUIContent("Select Folder"), GUILayout.MinWidth(80), GUILayout.MaxWidth(100)))
            {
                inputFolderPath = EditorUtility.OpenFolderPanel("Select Folder with Excel Files", inputFolderPath, Application.dataPath);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginArea(new Rect(Screen.width - (100 + 10), 105, 100, 25));
            if (GUILayout.Button("Convert"))
            {
				new Converter.Json.ExcelToJsonConverter(outputFolderPath).ConvertAllWorkbooks(inputFolderPath);
            }
            GUILayout.EndArea();

            GUI.enabled = true;
        }
    }
}

