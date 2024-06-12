using BubbleFighter.Network.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{
    public partial class CharacterController
    {
        /* Action */
        private long lastExcuteUnixTime_Action = -1;

        /* Move */
        private Battle_Move moveInfo;
        private Vector3 vPos;
        private Vector3 vMove;
        private float fMoveSpdPerSec;

        private float SyncTime = 0.2f; // 추후 SyncTime에 맞춘다.
        private float remainSyncTime_Pos; // 추후 SyncTime에 맞춘다.

        /* Projectile */
        protected Dictionary<long, ProjectileBase> dic_ActiveProj { get; private set; } = new Dictionary<long, ProjectileBase>();

        public void OnUpdate_Net()
        {
            OnUpdate_Pos();
        }

        protected void OnUpdate_Pos()
        {
            if (moveInfo == null)
                return;

            remainSyncTime_Pos -= Time.deltaTime;
            var factor = Mathf.Clamp01(remainSyncTime_Pos / SyncTime);

            // 위치값
            // 새 위치값
            var newPos = Vector3.Lerp(vPos, Owner.transform.position, factor);
            var dirNewPos = (newPos - Owner.transform.position).normalized;

            Owner.SetMove(dirNewPos * fMoveSpdPerSec);
            vPos += vMove * Time.deltaTime;
        }

        public void Excute_Move(Battle_Move battleMove)
        {
            if (this.CtrlType != ControllerType.Net
                || battleMove == null
                || (moveInfo != null && moveInfo.EventTime > battleMove.EventTime))
                return;

            moveInfo = battleMove;

            // 포지션 및 움직임 세팅
            {
                vPos = battleMove.PositionVector.ToVector3();
                vMove = battleMove.MoveVector.ToVector3();
                fMoveSpdPerSec = vMove.magnitude;

                // 갭차이가 너무 크면 새로 세팅한다.
                if ((vPos - Owner.transform.position).magnitude > 1f)
                    Owner.Motor.SetTransientPosition(vPos);

                remainSyncTime_Pos = SyncTime;
            }

            // 회전 세팅
            Owner.SetLook(battleMove.RotationVector.ToQuaternion(), 1440f);

            // 물리 이동 세팅
            if (battleMove.IsVelocity)
            {
                Owner.SetVelocity(battleMove.VelocityVector.ToVector3());
            }
        }

        public void Excute_Action(Battle_Action battleAction)
        {
            if (this.CtrlType != ControllerType.Net
                || battleAction == null
                || battleAction.EventTime <= lastExcuteUnixTime_Action
                || !Owner.List_ActionData.CheckIndex(battleAction.ActionIndex))
                return;

            lastExcuteUnixTime_Action = battleAction.EventTime;

            Owner.ActionCtrl.PlayAction(Owner.List_ActionData[battleAction.ActionIndex].Name);
        }

        public virtual void Excute_BattleHitEffect(ST_BattleHitEffect atkHitInfo)
        {
            // Next Action Trigger
            if (atkHitInfo.HitPower > 10f)
                Owner.ActionCtrl.OnTriggerEvent(Action.TriggerEventType.Hit_Strong);
            else if (atkHitInfo.HitPower > 5f)
                Owner.ActionCtrl.OnTriggerEvent(Action.TriggerEventType.Hit_Medium);
            else if (atkHitInfo.HitPower > 0f)
                Owner.ActionCtrl.OnTriggerEvent(Action.TriggerEventType.Hit_Weak);

            // Todo
            // StatCtrl.SetHp(atkHitInfo.);
            // Power 대로 밀쳐지기
            // Action
        }

        public void Excute_BattleGenerateProj(Battle_Projectile_Generate battleGenProj)
        {
            if (this.CtrlType != ControllerType.Net)
                return;

            var projName = Owner.GetProjName(battleGenProj.ProjIndex);
            if (projName == null)
                return;

            ProjectileBase proj = Managers.FX.PlayFX(
               projName,
               battleGenProj.PositionVector.ToVector3(),
               battleGenProj.RotationVector.ToQuaternion(),
               Vector3.one,
               battleGenProj.ParentIndex < 0 ? null : Owner.GetParent((int)battleGenProj.ParentIndex),
               1f,
               0f) as ProjectileBase;

            dic_ActiveProj.AddOrRefresh(battleGenProj.FxUniqueKey, proj);
        }

        public void Excute_Battle_ReleaseProj(Battle_Projectile_Release battleRelProj)
        {
            if (this.CtrlType != ControllerType.Net)
                return;

            var proj = dic_ActiveProj.GetOrNull(battleRelProj.FxUniqueKey);
            if (proj != null)
            {
                proj.transform.localPosition = battleRelProj.PositionVector.ToVector3();
                proj.ReturnToPool();
            }
        }
    }
}