using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class BattleSceneState_End : BattleSceneStateBase
    {
        public override void OnStart()
        {
            base.OnStart();

            Managers.UI.OpenWindow(WindowID.Window_BattleEnd);
        }
    }
}
