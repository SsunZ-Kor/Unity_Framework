using BubbleFighter.Network.Protocol;
using SuperScrollView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class ScrollItem_RoomMain : LoopListViewItem2
    {
        [SerializeField]
        private Text _uiTxt_RoomName = null;
        [SerializeField]
        private Text _uiTxt_UserCount = null;
        [SerializeField]
        private ButtonEx _btn_JoinRoom = null;

        private ST_GameRoom data = null;

        private void Awake()
        {
            if (_btn_JoinRoom != null)
                _btn_JoinRoom.onClick.Subscribe(OnClick_JoinRoom);
        }

        public void SetData(ST_GameRoom roomInfo)
        {
            if(_uiTxt_RoomName != null)
                _uiTxt_RoomName.text = roomInfo.RoomName;
            if(_uiTxt_UserCount != null)
                _uiTxt_UserCount.text = roomInfo.UserCount.ToString();

            data = roomInfo;
        }

        public void OnClick_JoinRoom()
        {
            NetProcess.Request_JoinGameRoom(data.RoomId);
        }
    }
}