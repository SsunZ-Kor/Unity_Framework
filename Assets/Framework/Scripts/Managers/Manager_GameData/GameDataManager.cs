using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace Game
{
    public interface ITableContainer
    {
        void SetData(TextAsset ta);
    }

    [Serializable]
    public class DataListContainder<U> where U : GameDataBase
    {
        public List<U> dataList = new List<U>();
    }

    public class TableContainer<T> : ITableContainer where T : GameDataBase
    {

        public Dictionary<int, T> dic_Table = new Dictionary<int, T>();
        
        public void SetData(TextAsset ta)
        {
            // 자료 정리
            var dataList = JsonConvert.DeserializeObject<DataListContainder<T>>(ta.text);
            for (int i = 0; i < dataList.dataList.Count; ++i)
            {
                var data = dataList.dataList[i];
                dic_Table.Add(data.Id, data);
            }
        }
    }


    public partial class GameDataManager : ManagerBase
    {
        private Dictionary<string, ITableContainer> _dic_Datas;

        public override IEnumerator Init_Async()
        {
            if (_dic_Datas == null)
                _dic_Datas = new Dictionary<string, ITableContainer>();
            else
                _dic_Datas.Clear();

            yield break;
        }

        public void LoadData<T>() where T : GameDataBase
        {
            var dataName = typeof(T).Name;
            
            // 이미 로드 되어있다면 패스
            if (_dic_Datas.ContainsKey(dataName))
                return;

            // Json 로드
            var ta_Data = Resources.Load<TextAsset>($"Data/GameDatas/{dataName}");
            if (ta_Data == null)
            {
                Debug.LogError("GameDataManager->LoadData is Faild :: " + dataName);
                return;
            }

            var newTableContainer = new TableContainer<T>();
            newTableContainer.SetData(ta_Data);

            _dic_Datas.Add(dataName, newTableContainer);
        }
    }
}
