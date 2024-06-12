using BubbleFighter.Network.Protocol;
using SuperScrollView;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Window_RoomList : WindowBase
    {
        [SerializeField]
        private ButtonEx _btn_CreateRoom = null;
        [SerializeField]
        private ButtonEx _btn_RefreshRoomList = null;

        [SerializeField]
        private LoopListView2 _scroll_RoomList = null;

        private List<ST_GameRoom> _list_Room;

        protected override void Awake()
        {
            base.Awake();

            if (_btn_CreateRoom != null)
                _btn_CreateRoom.onClick.Subscribe(OnClick_CreateRoom);

            if (_btn_RefreshRoomList != null)
                _btn_RefreshRoomList.onClick.Subscribe(OnClick_RefreshRoom);

            if (_scroll_RoomList != null)
                _scroll_RoomList.InitListView(10, OnGetItemByIndex);
        }

        private void OnEnable()
        {
            _scroll_RoomList.ResetListView();
        }

        private void OnClick_CreateRoom()
        {
            Managers.UI.OpenWindow(WindowID.Window_RoomCreate_Popup);
        }

        private void OnClick_RefreshRoom()
        {
            NetProcess.Request_GameRoomList();
        }

        public void SetRoomList(List<ST_GameRoom> list_GameRoom)
        {
            _list_Room = list_GameRoom;

            _scroll_RoomList.SetListItemCount(_list_Room?.Count ?? 0, true);
            _scroll_RoomList.RefreshAllShownItem();
        }

        private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
        {
            if (_list_Room == null || !_list_Room.CheckIndex(index))
                return null;

            ScrollItem_RoomMain item = listView.NewListViewItem("ScrollItem_RoomMain") as ScrollItem_RoomMain;

            if (item == null)
                return null;

            item.SetData(_list_Room[index]);

            return item;
        }
    }
}