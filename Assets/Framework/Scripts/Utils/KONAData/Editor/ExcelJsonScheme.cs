using NPOI.SS.UserModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KONA.Data.Editor
{
    public class ExcelJsonScheme
    {
        private ISheet sheet;
        private ExcelJsonSchemeNode root;
        private int contentStartRowNum;
        private int contentEndRowNum;

        public ISheet Sheet { get => sheet; }
        public ExcelJsonSchemeNode Root { get => root; }
        public int ContentStartRowNum { get => contentStartRowNum; }
        public int ContentEndRowNum { get => contentEndRowNum; }

        public ExcelJsonScheme(ISheet sheet, ExcelJsonSchemeNode root, int contentStartRowNum, int contentEndRowNum)
        {
            this.sheet = sheet;
            this.root = root;
            this.contentStartRowNum = contentStartRowNum;
            this.contentEndRowNum = contentEndRowNum;
        }

        public List<ExcelJsonSchemeNode> GetNodeList()
        {
            return root.Linear();
        }
    }
}