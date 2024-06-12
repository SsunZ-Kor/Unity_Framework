using System.Collections.Generic;
using System;

namespace Game
{
    public partial class GameDataManager
    {
        public T GetData<T>(int nID) where T : GameDataBase
        {
            var typeName = typeof(T).Name;
            if(!_dic_Datas.ContainsKey(typeName))
                LoadData<T>();
            
            var table = _dic_Datas[typeName] as TableContainer<T>;
            if (table == null)
                return null;

            return table.dic_Table.GetOrNull(nID);
        }

        public Dictionary<int, T> GetDicData<T>() where T : GameDataBase
        {
            var typeName = typeof(T).Name;
            if (!_dic_Datas.ContainsKey(typeName))
                LoadData<T>();

            var table = _dic_Datas[typeName] as TableContainer<T>;
            if (table == null)
                return null;

            return table.dic_Table;
        }
    }
}