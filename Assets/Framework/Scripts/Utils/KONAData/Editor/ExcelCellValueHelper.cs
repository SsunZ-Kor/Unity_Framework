using System.Collections.Generic;
using UnityEngine;

using NPOI.SS.UserModel;

namespace KONA.Data.Editor
{
    public static class ExcelCellValueHelper
    {
        private static Dictionary<IWorkbook, IFormulaEvaluator> formulaevaluatorMap = new Dictionary<IWorkbook, IFormulaEvaluator>();

        public static System.Object GetCellValue(ICell cell)
        {
            if (cell == null) return null;

            if (cell.CellType.Equals(CellType.Formula))
            {
                //Debug.Log($"Fomula [{cell.Row.Sheet.SheetName}] C{cell.ColumnIndex} R{cell.RowIndex}");
                IWorkbook wb = cell.Row.Sheet.Workbook;

                IFormulaEvaluator evaluator = null;
                if (formulaevaluatorMap.ContainsKey(wb))
                {
                    evaluator = formulaevaluatorMap[wb];
                }
                else
                {
                    evaluator = cell.Row.Sheet.Workbook.GetCreationHelper().CreateFormulaEvaluator();
                    formulaevaluatorMap.Add(wb, evaluator);
                }
                evaluator.EvaluateInCell(cell);
            }

            System.Object obj = null;

            switch (cell.CellType)
            {
                case CellType.Boolean:
                    {
                        obj = cell.BooleanCellValue;
                    }
                    break;
                case CellType.Numeric:
                //{
                //    obj = (Decimal)cell.NumericCellValue;
                //}
                //break;                

                case CellType.String:
                    {
                        cell.SetCellType(CellType.String);
                        string stringCellValue = cell.StringCellValue;
                        if (!string.IsNullOrEmpty(stringCellValue))
                        {
                            long longValue;
                            double doubleValue;
                            if (long.TryParse(stringCellValue, out longValue))
                            {
                                obj = longValue;
                            }
                            else if (double.TryParse(stringCellValue, out doubleValue))
                            {
                                obj = doubleValue;
                            }
                            else
                                obj = stringCellValue;
                        }
                    }
                    break;
                case CellType.Blank:
                    obj = "";
                    break;
                case CellType.Error:
                    {
                        Debug.LogError($"cell represent error({cell.Sheet.SheetName} - {cell})");
                    }
                    break;
                case CellType.Formula:
                    {
                        Debug.LogError("formula not supported");
                    }
                    break;
            }

            return obj;
        }
    }
}