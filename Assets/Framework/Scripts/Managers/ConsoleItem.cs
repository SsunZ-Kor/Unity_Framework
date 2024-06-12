using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SuperScrollView;

namespace Game
{
    public class ConsoleItem : LoopListViewItem2, IPointerClickHandler
    {

        [SerializeField]
        private Text _uiTxt_Contents = null;

        [SerializeField]
        private Text _uiTxt_CollapseCount = null;

        public int Count { get; private set; }

        private ConsoleWindow _consoleWnd = null;

        public void Init(ConsoleWindow consoleWnd)
        {
        }

        public void SetLog(ConsoleWindow consoleWnd, string log, int count)
        {
            _consoleWnd = consoleWnd;
            if (_uiTxt_Contents != null)
                _uiTxt_Contents.text = log;

            if (_uiTxt_CollapseCount != null)
                _uiTxt_CollapseCount.text = count < 1000 ? count.ToString() : "+999";
        }

        public string GetLog()
        {
            return _uiTxt_Contents?.text;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _consoleWnd.SelectLogList(this);
        }
    }
}