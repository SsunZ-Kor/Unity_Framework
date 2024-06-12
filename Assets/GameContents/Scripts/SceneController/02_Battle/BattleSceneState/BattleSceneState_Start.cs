using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class BattleSceneState_Start : BattleSceneStateBase
    {
        bool isShowStartCount = false;
        Window_BattleStart wnd_BattleStart;
        long ShowStartCountTime;

        public override void Init(BattleSceneController battleSceneCtrl)
        {
            base.Init(battleSceneCtrl);

            isShowStartCount = false;
            Managers.UI.GetWindow(WindowID.Window_BattleStart, true);
        }

        public override void OnStart()
        {
            base.OnStart();

            wnd_BattleStart = Managers.UI.OpenWindow(WindowID.Window_BattleStart) as Window_BattleStart;
            ShowStartCountTime = battleSceneCtrl.GameStartTime - 3000;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (isShowStartCount)
                return;

            if (ShowStartCountTime > UserData.TimeInfo.GetServerTimeMilliSec())
                return;

            isShowStartCount = true;
            wnd_BattleStart.Play(
                () =>
                {
                    wnd_BattleStart.CloseSelf();
                    battleSceneCtrl.ChangeState(BattleSceneStateType.Playing);
                }
                );
        }
    }
}