using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class LobbySceneController : SceneControllerBase<LobbySceneController>
    {
        protected override IEnumerator Init()
        {
            Managers.UI.Clear(true);

            Managers.UI.OpenWindow(WindowID.Window_LobbyMain);

            if (string.IsNullOrWhiteSpace(UserData.AccountInfo.NickName))
                Managers.UI.OpenWindow(WindowID.Window_NickName_Popup);

            return base.Init();
        }
    }
}