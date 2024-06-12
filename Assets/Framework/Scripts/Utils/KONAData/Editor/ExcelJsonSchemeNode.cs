using System;
using System.Collections.Generic;

using NPOI.SS.UserModel;

namespace KONA.Data.Editor
{
    public class ExcelJsonSchemeNode
    {
        private ISheet sheet;
        private int rowNum;
        private int cellNum;

        public enum ECellType
        {
            PROPERTY, KEY, VALUE, MAP, ARRAY, IGNORE
        }

        private string key = "";
        private ECellType type = ECellType.PROPERTY;

        public ECellType Type { get => type; }
        public bool IsContainer { get => (type.Equals(ECellType.ARRAY)) || (type.Equals(ECellType.MAP)); }
        public bool IsKeyProvidable { get => (type.Equals(ECellType.KEY) || type.Equals(ECellType.PROPERTY) || !string.IsNullOrEmpty(key)) && !type.Equals(ECellType.IGNORE); }

        private ExcelJsonSchemeNode parent = null;
        public ExcelJsonSchemeNode Parent { get => parent; set => parent = value; }

        private List<ExcelJsonSchemeNode> children = new List<ExcelJsonSchemeNode>();

        public bool IsRoot { get => this.parent == null; }
        public List<ExcelJsonSchemeNode> Children { get => children; }
        public int CellNum { get => cellNum; }
        public int RowNum { get => rowNum; }

        public ExcelJsonSchemeNode(ISheet sheet, int rowNum, int cellNum, string schemeName)
        {
            this.sheet = sheet;
            this.rowNum = rowNum;
            this.cellNum = cellNum;

            string splitter = "$";

            if (!schemeName.Contains(splitter))
            {
                this.key = schemeName;
                this.type = ECellType.PROPERTY;
            }
            else
            {
                string[] splitted = schemeName.Split(splitter.ToCharArray());
                this.key = splitted[0];
                string typeString = splitted[(splitted.Length - 1)];
                switch (typeString)
                {
                    case "{}":
                        this.type = ECellType.MAP;
                        break;
                    case "[]":
                        this.type = ECellType.ARRAY;
                        break;
                    case "key":
                        this.type = ECellType.KEY;
                        break;
                    case "value":
                        this.type = ECellType.VALUE;
                        break;
                    case "^":
                        this.type = ECellType.IGNORE;
                        break;
                    default:
                        break;
                }
            }
        }

        public void AddChild(ExcelJsonSchemeNode child)
        {
            switch (this.type)
            {
                case ECellType.KEY:
                    if (child.Type.Equals(ECellType.KEY) || child.Type.Equals(ECellType.PROPERTY))
                    {
                        throw new Exception("key and property are impossible to be child of key");
                    }
                    break;
                case ECellType.PROPERTY:
                    if (child.Type.Equals(ECellType.KEY) || child.Type.Equals(ECellType.PROPERTY))
                    {
                        throw new Exception("key and property are impossible to be child of key");
                    }
                    break;
                case ECellType.ARRAY:
                    if (child.Type.Equals(ECellType.PROPERTY) || child.Type.Equals(ECellType.KEY))
                    {
                        throw new Exception("array's child must be unnamed one");
                    }
                    break;
                case ECellType.MAP:
                    if (child.Type.Equals(ECellType.VALUE))
                    {
                        throw new Exception("value object is impossible to be child of map");
                    }
                    break;
                case ECellType.VALUE:
                    throw new System.Exception("value object cannot have a child");
            }

            child.Parent = this;
            this.children.Add(child);
        }

        public List<ExcelJsonSchemeNode> Linear()
        {
            List<ExcelJsonSchemeNode> list = new List<ExcelJsonSchemeNode>();

            list.Add(this);
            foreach (var child in children)
            {
                list.AddRange(child.Linear());
            }

            return list;
        }

        public string GetKey(IRow row)
        {
            if (!string.IsNullOrEmpty(key))
                return key;

            if (type.Equals(ECellType.KEY))
            {
                NPOI.SS.UserModel.ICell cell = row.GetCell(cellNum);
                if (null == cell)
                    return null;

                return ExcelCellValueHelper.GetCellValue(cell).ToString();
            }

            //if( parent != null && parent.IsKeyProvidable )
            //{
            //    if(string.IsNullOrEmpty(parent.key))
            //    {
            //        var cell = row.GetCell(parent.CellNum);
            //        if(cell == null)
            //        {
            //            return null;
            //        }
            //        //if (cell.RowIndex == 20 && cell.ColumnIndex == 3)
            //        //    Debug.Log("test");
            //        System.Object cellValue = ExcelCellValueHelper.GetCellValue(cell);
            //        if (cellValue == null)
            //            return null;

            //        return ExcelCellValueHelper.GetCellValue(cell).ToString();
            //    }
            //    return parent.GetKey(row);
            //}

            return null;
        }

        public System.Object GetValue(IRow row)
        {
            ICell cell = row.GetCell(cellNum);
            return ExcelCellValueHelper.GetCellValue(cell);
        }

        public override string ToString()
        {
            string str = "";
            for (int dep = 1; dep < rowNum; dep++)
                str += "-";

            str += $"ExcelJsonSchemeNode [type={type.ToString()}]";
            return str;
        }
    }
}

