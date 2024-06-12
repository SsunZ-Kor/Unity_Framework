using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class BattleSceneState_Playing : BattleSceneStateBase
    {
        public override void Init(BattleSceneController battleSceneCtrl)
        {
            base.Init(battleSceneCtrl);            
            
            // 미리 사용된 UI 초기화
            Managers.UI.GetWindow(WindowID.Window_BattleMain, true);
        }

        public override void OnStart()
        {
            base.OnStart();

            Managers.UI.OpenWindow(WindowID.Window_BattleMain);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}