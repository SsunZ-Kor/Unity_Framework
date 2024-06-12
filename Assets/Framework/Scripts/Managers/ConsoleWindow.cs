using SuperScrollView;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public partial class ConsoleWindow : MonoBehaviour
    {
        public class LogCountPair
        {
            public string log;
            public int count;
        }


        public static ConsoleWindow instance;

        [SerializeField]
        private Text _uiTxt_LogDetail = null;
        [SerializeField]
        private Text _uiTxtInput_AdminCmd = null;

        [SerializeField]
        private LoopListView2 _loopListView = null;

        [SerializeField]
        private TabEx _uiTab = null;
        [SerializeField]
        private GameObject _root_Log = null;
        [SerializeField]
        private GameObject _root_Cheat = null;

        List<LogCountPair> _list_log = new List<LogCountPair>(2000);

        private void Awake()
        {
            if (instance != null)
            {
                GameObject.Destroy(this.gameObject);
                return;
            }

            OnAwake_Cheat();

            instance = this;
            GameObject.DontDestroyOnLoad(this.gameObject);
            Application.logMessageReceived += ReciveLog;
            this.gameObject.SetActive(false);
            _loopListView.InitListView(10, OnGetItemByIndex);
            _loopListView.MovePanelToItemIndex(0, 0f);

            _uiTab.SetCallback(OnTab);
            _uiTab.ChangeIndex(0);
        }

        private void OnEnable()
        {
            _loopListView.SetListItemCount(_list_log?.Count ?? 0, false);
            _loopListView.RefreshAllShownItem();
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= ReciveLog;
        }

        private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
        {
            if (!_list_log.CheckIndex(index))
                return null;

            ConsoleItem item = listView.NewListViewItem("Item_Console") as ConsoleItem;

            if (item == null)
                return null;

            item.SetLog(this, _list_log[index].log, _list_log[index].count);

            return item;
        }

        public void ReciveLog(string condition, string stackTrace, LogType type)
        {
            string color = "#ffffffff";
            switch (type)
            {
                case LogType.Warning: color = "#ffff00ff"; break;
                case LogType.Error:
                case LogType.Exception: color = "#ff0000ff"; break;
            }

            var log = $"{DateTime.Now} :: <color={color}>{type}</color> :: {condition}\n\n{stackTrace}";

            // 마지막 로그와 비교
            if (_list_log.Count > 0)
            {
                var lastLog = _list_log[_list_log.Count - 1].log;
                // 같다면 카운트만 증가 시키고 호출 스택 종료
                if (lastLog.CompareTo(log) == 0)
                {
                    _list_log[_list_log.Count - 1].count++;
                    return;
                }
            }

            if (_list_log.Count >= 2000)
            {
                _list_log.RemoveAt(_list_log.Count - 1);
                _list_log.Insert(0, new LogCountPair() { log = log, count = 1 });

                if (this.gameObject.activeSelf)
                {
                    _loopListView.RefreshAllShownItem();
                }
            }
            else
            {
                _list_log.Insert(0, new LogCountPair() { log = log, count = 1 });
                if (this.gameObject.activeSelf)
                {
                    _loopListView.SetListItemCount(_list_log?.Count ?? 0, false);
                    _loopListView.RefreshAllShownItem();
                }
            }

        }

        public void SelectLogList(ConsoleItem item)
        {
            if (item == null || _uiTxt_LogDetail == null)
                return;

            _uiTxt_LogDetail.text = item.GetLog();
        }

        public void CopyLogToClipboard()
        {
            UniClipboard.SetText(_uiTxt_LogDetail?.text);
            Debug.Log("Copy Success");
        }

        public void Close()
        {
            this.gameObject.SetActive(false);
        }

        public void ClearInputFeild()
        {
            _uiTxtInput_AdminCmd.text = null;
        }

        public void OnSubmitInputFeild(string value)
        {
            if(string.IsNullOrEmpty(value) || value == "Admin Command")
                return;

#if DevClient
            ReciveLog($"AdminCmd :: {value}", "ConsoleWindow->OnSubmitInputFeild", LogType.Log);
#else
            ReciveLog($"AdminCmd :: Not supported in real build", "ConsoleWindow->OnSubmitInputFeild", LogType.Log);
#endif

        }

        private void OnTab(int idx)
        {
            switch (idx)
            {
                case 0:
                    _root_Log.SetActive(true);
                    _root_Cheat.SetActive(false);
                    break;
                case 1:
                    _root_Log.SetActive(false);
                    _root_Cheat.SetActive(true);
                    break;
            }
        }
    }
}