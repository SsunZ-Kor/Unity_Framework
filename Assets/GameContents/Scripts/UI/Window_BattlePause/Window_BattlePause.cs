using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Window_BattlePause : WindowBase
    {
        [SerializeField]
        private ButtonEx _btn_ContinueGame = null;
        [SerializeField]
        private ButtonEx _btn_GoToLobby = null;
        [SerializeField]
        private ButtonEx _btn_Settings = null;

        protected override void Awake()
        {
            base.Awake();

            _btn_ContinueGame.onClick.Subscribe(() => CloseSelf());
            _btn_GoToLobby.onClick.Subscribe(OnClick_GoToLobby);
            _btn_Settings.onClick.Subscribe(OnClick_OpenSettings);
        }

        private void OnClick_GoToLobby()
        {
            NetProcess.Request_LeaveGameRoom(() =>
            {
                Managers.Scene.LoadScene(SceneID.Lobby, LoadingID.Loading_FadeInOut);
            });
        }

        private void OnClick_OpenSettings()
        {
            Managers.UI.OpenWindow(WindowID.Window_Settings);
        }
    }
}