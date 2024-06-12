using BubbleFighter.Network.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Window_LobbyMain : WindowBase
    {
        [SerializeField]
        private ButtonEx _btn_GoToRoomList = null;

        protected override void Awake()
        {
            base.Awake();

            if (_btn_GoToRoomList != null)
                _btn_GoToRoomList.onClick.Subscribe(OnClick_GoToRoomList);
        }

        public override bool CloseSelf()
        {
            if (Managers.Scene.CurrScene == SceneID.Lobby)
            {
                Managers.UI.EnqueuePopup("알림", "게임을 종료하시겠습니까?", Application.Quit, null);
                return false;
            }

            return base.CloseSelf();
        }

        private void OnClick_GoToRoomList()
        {
            NetProcess.Request_GameRoomList();
        }
    }
}