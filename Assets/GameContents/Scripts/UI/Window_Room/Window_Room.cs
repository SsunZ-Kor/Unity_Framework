using BubbleFighter.Network.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Window_Room : WindowBase
    {
        [System.Serializable]
        public class RoomItems
        { 
            public List<Item_Room> list_RoomItem = new List<Item_Room>();
        }

        [SerializeField]
        private List<RoomItems> list_RoomItems = new List<RoomItems>();
        [SerializeField]
        private ButtonEx _btn_Ready = null;
        [SerializeField]
        private ButtonEx _btn_Start = null;

        private bool bLock = true;

        protected override void Awake()
        {
            base.Awake();

            _btn_Ready.onClick.Subscribe(OnClick_Ready);
            _btn_Start.onClick.Subscribe(OnClick_Start);
        }

        public override bool CloseSelf()
        {
            if (bLock)
            {
                Managers.UI.EnqueuePopup(
                    "알림",
                    "정말로 방을 떠나시겠습니까?",
                    () => NetProcess.Request_LeaveGameRoom(() => { bLock = false; this.CloseSelf(); }),
                    null
                    );
                return false;
            }

            return base.CloseSelf();
        }

        public override void OnEvent_AfterOpen()
        {
            base.OnEvent_AfterOpen();
            bLock = true;
        }

        public void RefreshGameRoom()
        {
            var gameRoom = UserData.RoomInfo.GameRoom;
            if (gameRoom == null)
            {
                Debug.LogError("Window_Room->SetRoomInfo :: Room Info is Null");
                return;
            }

            var dic_TeamMemberIndex = new Dictionary<int, int>();
            for (int i = 0; i < list_RoomItems.Count; ++i)
                dic_TeamMemberIndex.Add(i + 1, 0);

            // Item_Room 세팅
            for (int i = 0; i < gameRoom.Users.Count; ++i)
            {
                var userInfo = gameRoom.Users[i];
                var bIsMaster = userInfo.UserNo == gameRoom.MasterNo;
                var bIsMyInfo = userInfo.UserNo == UserData.AccountInfo.UserNo;

                var memberIndex = dic_TeamMemberIndex[userInfo.Team];
                dic_TeamMemberIndex[userInfo.Team] = memberIndex + 1;

                Item_Room item = list_RoomItems[userInfo.Team - 1].list_RoomItem[memberIndex];
                item.SetInfo(userInfo, bIsMaster);
                if (!item.gameObject.activeSelf)
                    item.gameObject.SetActive(true);

                // 내 상태라면 준비 or 시작 버튼 세팅
                if (bIsMyInfo)
                {
                    _btn_Ready.gameObject.SetActive(!bIsMaster && userInfo.UserState == 0);
                    _btn_Start.gameObject.SetActive(bIsMaster);
                }
            }

            // Todo :: 추후 서버에서 재대로 된 값이 올것임
            // 현재는 팀이 무조건 2개, MaxUserCount는 모든 유저의 합으로 오니 하드코딩 해놓는다.

            var maxTeamUserCount = gameRoom.MaxUserCount / 2;
            // 비어있는 Item_Room 세팅
            foreach (var pair in dic_TeamMemberIndex)
            {
                var teamIdx = pair.Key - 1;
                var memberIdx = pair.Value;
                
                var list_roomItem = list_RoomItems[teamIdx].list_RoomItem;
                for (; memberIdx < maxTeamUserCount; ++memberIdx)
                    list_roomItem[memberIdx].SetInfo(null, false);

                for (; memberIdx < list_roomItem.Count; ++memberIdx)
                    list_roomItem[memberIdx].gameObject.SetActive(false);
            }
        }

        public void OnClick_Ready()
        {
            NetProcess.Request_ReadyGameRoom();
            _btn_Ready.gameObject.SetActive(false);
        }

        public void OnClick_Start()
        {
            NetProcess.Send_StartGameRoom();
        }

        public void Process_ReadyGameRoom(long userNo)
        {
            // Todo :: 최적화 필요
            for (int i = 0; i < list_RoomItems.Count; ++i)
            {
                var list_RoomItem = list_RoomItems[i].list_RoomItem;
                Item_Room item = list_RoomItem.Find(x => (x.userInfo?.UserNo ?? -1) == userNo);
                if (item == null)
                    continue;

                item.SetReady();
                break;
            }
        }
    }
}