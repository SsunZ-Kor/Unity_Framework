using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Window_BattleEnd : WindowBase
    {
        [SerializeField]
        private ButtonEx _btn_ReturnToLobby = null;

        protected override void Awake()
        {
            base.Awake();

            _btn_ReturnToLobby.onClick.Subscribe(OnClick_ReturnToLobby);
        }

        public override bool CloseSelf()
        {
            OnClick_ReturnToLobby();
            return base.CloseSelf();
        }

        public void OnClick_ReturnToLobby()
        {
            UserData.RoomInfo.SetGameRoom(null);
            Managers.Scene.LoadScene(SceneID.Lobby, LoadingID.Loading_FadeInOut);
        }
    }
}
