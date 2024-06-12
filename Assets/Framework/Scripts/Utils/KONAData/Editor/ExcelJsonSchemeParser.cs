using System.Collections.Generic;
using UnityEngine;

using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace KONA.Data.Editor
{
    public class ExcelJsonSchemeParser
    {
        private const int ILLEGAL_ROW_NUM = -1;
        private const int COMMENT_ROW_NUM = 0;
        private const string SCHEME_END = "$scheme_end";

        private readonly ISheet sheet;
        private readonly IRow schemeStartRow;
        private readonly short firstCellNum;
        private readonly short lastCellNum;
        private int schemeEndRowNum;
        private ExcelJsonSchemeNode parent = null;

        public ExcelJsonSchemeParser(ISheet sheet)
        {
            this.sheet = sheet;
            this.schemeEndRowNum = ILLEGAL_ROW_NUM;

            var rows = sheet.GetRowEnumerator();

            while (rows.MoveNext())
            {
                IRow row = (IRow)rows.Current;
                if (row.RowNum != 0 && ContainsEndMarker(row))
                {
                    schemeEndRowNum = row.RowNum;
                }
            }

            if (schemeEndRowNum == ILLEGAL_ROW_NUM)
            {
                Debug.LogError($"[{this.sheet.SheetName}] Scheme end row marker not found");
            }

            this.schemeStartRow = this.sheet.GetRow(1);
            this.firstCellNum = this.schemeStartRow.FirstCellNum;
            this.lastCellNum = this.schemeStartRow.LastCellNum;
        }

        private List<CellRangeAddress> GetMergedRegionsInRow(int row)
        {
            List<CellRangeAddress> regions = new List<CellRangeAddress>();
            for (int i = 0; i < this.sheet.NumMergedRegions; i++)
            {
                CellRangeAddress range = this.sheet.GetMergedRegion(i);
                if (range.FirstRow <= row && range.LastRow >= row)
                    regions.Add(range);
            }
            return regions;
        }

        public ExcelJsonScheme ParseAndGetScheme()
        {
            ExcelJsonSchemeNode rootNode = Parse(this.parent, this.schemeStartRow.RowNum, this.firstCellNum, this.lastCellNum - 1);
            return new ExcelJsonScheme(this.sheet, rootNode, this.schemeEndRowNum + 1, this.sheet.LastRowNum);
        }

        private ExcelJsonSchemeNode Parse(ExcelJsonSchemeNode parent, int rowNum, int startCellNum, int endCellNum)
        {
            for (int cellNum = startCellNum; cellNum <= endCellNum; cellNum++)
            {
                ICell cell = this.sheet.GetRow(rowNum).GetCell(cellNum);
                if (cell != null)
                {
                    cell.SetCellType(CellType.String);
                    string value = cell.StringCellValue;
                    if (value != null && !string.IsNullOrEmpty(value) && !value.Equals("^", System.StringComparison.OrdinalIgnoreCase))
                    {
                        ExcelJsonSchemeNode child = new ExcelJsonSchemeNode(this.sheet, rowNum, cellNum, value);
                        if (parent == null)
                            parent = child;
                        else
                        {
                            parent.AddChild(child);
                            if (child.Type.Equals(ExcelJsonSchemeNode.ECellType.KEY))
                            {
                                cellNum++;
                                Parse(child, rowNum, cellNum, cellNum);
                                continue;
                            }
                        }

                        List<CellRangeAddress> mergedRegionsInRow = GetMergedRegionsInRow(rowNum);
                        if (child.IsContainer)
                        {
                            int firstCellNum = cellNum;
                            int lastCellNum = cellNum;
                            foreach (CellRangeAddress region in mergedRegionsInRow)
                            {
                                if (region.IsInRange(rowNum, cellNum))
                                {
                                    firstCellNum = region.FirstColumn;
                                    lastCellNum = region.LastColumn;
                                }
                            }
                            Parse(child, rowNum + 1, firstCellNum, lastCellNum);
                            cellNum = lastCellNum;
                        }
                    }
                }
            }
            return parent;
        }

        private bool ContainsEndMarker(IRow row)
        {
            ICell cell = row.GetCell(0);
            return (cell != null) && cell.CellType.Equals(CellType.String)
                && cell.StringCellValue.Equals(SCHEME_END, System.StringComparison.OrdinalIgnoreCase);
        }

    }
}