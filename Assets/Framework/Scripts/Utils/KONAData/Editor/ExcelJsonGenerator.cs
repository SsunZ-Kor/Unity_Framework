using System.Collections;
using System.Collections.Generic;

using System;
using System.Text;
using System.IO;
using NPOI.SS.UserModel;

using Newtonsoft.Json;
using System.Linq.Expressions;

namespace KONA.Data.Editor
{
    public class ExcelJsonGenerator
    {
        private StringBuilder sb = new StringBuilder();
        private StringWriter sw = new StringWriter();

        private ExcelJsonScheme scheme;
        private ISheet sheet;
        private int currentRowNum;

        private enum EJsonType
        {
            Object,
            Array,
        }

        private Stack<EJsonType> stack = new Stack<EJsonType>();

        public ExcelJsonGenerator(ExcelJsonScheme scheme)
        {
            this.scheme = scheme;
            this.sheet = this.scheme.Sheet;
            this.currentRowNum = this.scheme.ContentStartRowNum;
        }

        private void Push(EJsonType jsonType, JsonWriter writer)
        {
            this.stack.Push(jsonType);

            switch (jsonType)
            {
                case EJsonType.Object:
                    writer.WriteStartObject();
                    break;
                case EJsonType.Array:
                    writer.WriteStartArray();
                    break;
            }
        }

        private EJsonType Pop(JsonWriter writer)
        {
            EJsonType jsonType = stack.Pop();

            switch (jsonType)
            {
                case EJsonType.Object:
                    writer.WriteEndObject();
                    break;
                case EJsonType.Array:
                    writer.WriteEnd();
                    break;
            }

            return jsonType;
        }

        public string Generate()
        {
            string result = "";
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                ExcelJsonSchemeNode rootNode = scheme.Root;

                List<ExcelJsonSchemeNode> nodeList = scheme.GetNodeList();

                for (int rowNum = this.currentRowNum; rowNum <= this.scheme.ContentEndRowNum; rowNum++)
                {
                    IEnumerator enumerator = nodeList.GetEnumerator();

                    if (stack.Count > 0)
                    {
                        enumerator.MoveNext();
                        enumerator.MoveNext();
                    }

                    IRow row = sheet.GetRow(rowNum);
                    if (row != null)
                    {
                        while (enumerator.MoveNext())
                        {
                            ExcelJsonSchemeNode schemeNode = (ExcelJsonSchemeNode)enumerator.Current;

                            int stackSize = stack.Count;
                            if (schemeNode.RowNum - 1 < stackSize)
                            {
                                int back = stackSize - schemeNode.RowNum;
                                for (int j = 0; j <= back; j++)
                                {
                                    Pop(writer);
                                }
                            }

                            if (schemeNode.IsContainer)
                            {
                                string key = schemeNode.GetKey(row);
                                if (!string.IsNullOrEmpty(key))
                                {
                                    writer.WritePropertyName(key);
                                }

                                if (schemeNode.Type.Equals(ExcelJsonSchemeNode.ECellType.MAP))
                                {
                                    Push(EJsonType.Object, writer);
                                }
                                else if (schemeNode.Type.Equals(ExcelJsonSchemeNode.ECellType.ARRAY))
                                {
                                    Push(EJsonType.Array, writer);
                                }
                            }
                            else if (schemeNode.IsKeyProvidable)
                            {
                                if (schemeNode.Type.Equals(ExcelJsonSchemeNode.ECellType.PROPERTY))
                                {
                                    string key = schemeNode.GetKey(row);
                                    if (string.IsNullOrEmpty(key)) break;

                                    writer.WritePropertyName(key);
                                    writer.WriteValue(schemeNode.GetValue(row));
                                }
                            }
                            else if (schemeNode.Type.Equals(ExcelJsonSchemeNode.ECellType.VALUE))
                            {
                                Object obj = schemeNode.GetValue(row);

                                if (obj.GetType() == typeof(string))
                                {
                                    if (!string.IsNullOrEmpty((string)obj))
                                        writer.WriteValue(schemeNode.GetValue(row));
                                }
                                else
                                {
                                    writer.WriteValue(schemeNode.GetValue(row));
                                }
                            }
                        }
                    }
                }

                while (stack.Count > 0)
                {
                    Pop(writer);
                }

                result = sb.ToString();
                writer.Close();
            }

            return result;
        }
    }
}