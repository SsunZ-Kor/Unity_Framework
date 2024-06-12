using BubbleFighter.Network.Protocol;
using GameAnvil;
using GameAnvil.User;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace Game
{
    public static partial class NetProcess
    {
        public static void Send_BattleMove(bool useVel, Vector3 vVel, Vector3 vMove, Vector3 vPos, Quaternion qRot, int gravityFeildIdx)
        {

            var send_BattleMove = new SEND_Battle_Move()
            {
                Header = Managers.Net.CreateHeader(),
                Move = new Battle_Move()
                {
                    EventTime = UserData.TimeInfo.GetServerTimeMilliSec(),
                    UserUniqueKey = UserData.AccountInfo.UserNo,
                
                    IsVelocity = useVel,
                    VelocityVector = vVel.ToNetVector(),
                    MoveVector = vMove.ToNetVector(),
                    PositionVector = vPos.ToNetVector(),
                    RotationVector = qRot.ToNetVector(),
                
                    GravityObject = gravityFeildIdx,
                }
            };

            Managers.Net.Send(send_BattleMove);
        }

        public static void Notify_BattleMove(UserAgent userAgent, NOTIFY_Battle_Move notify)
        {
            var battleSceneCtrl = BattleSceneControllerBase.Instance;
            if (battleSceneCtrl == null)
                return;

            if (CheckError(notify.Header))
                return;

            var battleMove = notify.Move;

            var userChar = battleSceneCtrl.Dic_CharacterByUserNo.GetOrNull(battleMove.UserUniqueKey);
            if (userChar == null || !userChar.IsNetChar)
                return;

            // 중력장 업데이트
            if (userChar.GravityFeildIndex != battleMove.GravityObject)
            {
                if (userChar.GravityFeildIndex >= 0)
                {
                    // 이전 중력장 제거
                    var prvFeild = battleSceneCtrl.World.GetGravityFeild(userChar.GravityFeildIndex);
                    if (prvFeild != null)
                        prvFeild.UnControlGravity(userChar);
                }

                if (battleMove.GravityObject >= 0)
                {
                    // 현재 중력장 세팅
                    var crrFeild = battleSceneCtrl.World.GetGravityFeild(battleMove.GravityObject);
                    if (crrFeild != null)
                        crrFeild.ControlGravity(userChar);
                }
            }
            
            userChar.CharCtrl.Excute_Move(battleMove);
        }

        public static void Send_BattleAction(int actionIdx)
        {
            var send_BattleAction = new SEND_Battle_Action()
            {
                Header = Managers.Net.CreateHeader(),
                Action = new Battle_Action()
                {
                    EventTime = UserData.TimeInfo.GetServerTimeMilliSec(),
                    UserUniqueKey = UserData.AccountInfo.UserNo,

                    ActionIndex = actionIdx,
                },
            };

            Managers.Net.Send(send_BattleAction);
        }

        public static void Notify_BattleAction(UserAgent userAgent, NOTIFY_Battle_Action notify)
        {
            var battleSceneCtrl = BattleSceneControllerBase.Instance;
            if (battleSceneCtrl == null || CheckError(notify.Header))
                return;

            var battleAction = notify.Action;

            var userChar = battleSceneCtrl.Dic_CharacterByUserNo.GetOrNull(battleAction.UserUniqueKey);
            if (userChar == null || !userChar.IsNetChar)
                return;

            userChar.CharCtrl.Excute_Action(battleAction);
        }

        public static void Send_BattleHitEffect(List<ST_BattleHitEffect> list_AtkHitInfo)
        {
            if (list_AtkHitInfo == null || list_AtkHitInfo.Count == 0)
                return;

            var send_BattleHitEffect = new SEND_BattleHitEffect()
            {
                Header = Managers.Net.CreateHeader(),
            };

            send_BattleHitEffect.BattleHitEffect.AddRange(list_AtkHitInfo);

            Managers.Net.Send(send_BattleHitEffect);
        }

        public static void Notify_BattleHitEffect(UserAgent userAgent, NOTIFY_BattleHitEffect notify)
        {
            var battleSceneCtrl = BattleSceneControllerBase.Instance;
            if (battleSceneCtrl == null || CheckError(notify.Header) || notify.BattleHitEffect.Count == 0)
                return;

            for (int i = 0; i < notify.BattleHitEffect.Count; ++i)
            {
                var atkHitInfo = notify.BattleHitEffect[i];
                if (atkHitInfo == null)
                    continue;

                var userChar = battleSceneCtrl.Dic_CharacterByUserNo.GetOrNull(atkHitInfo.VictimUserNo);
                if (userChar == null || !userChar.IsNetChar)
                        continue;

                userChar.CharCtrl.Excute_BattleHitEffect(atkHitInfo);
            }
        }

        public static void Send_BattleGenerateProj(int projIdx, long projUniqueKey, Vector3 vPos, Quaternion qRot, int parentIdx)
        {
            var send_BattleGenerateProj = new SEND_Battle_Projectile_Generate()
            {
                Header = Managers.Net.CreateHeader(),
                Generate = new Battle_Projectile_Generate()
                {
                    EventTime = UserData.TimeInfo.GetServerTimeMilliSec(),
                    UserUniqueKey = UserData.AccountInfo.UserNo,

                    ProjIndex = projIdx,
                    FxUniqueKey = (int)projUniqueKey,
                    PositionVector = vPos.ToNetVector(),
                    RotationVector = qRot.ToNetVector(),
                    ParentIndex = parentIdx,
                },
            };

            Managers.Net.Send(send_BattleGenerateProj);
        }

        public static void Notify_BattleGenerateProj(UserAgent userAgent, NOTIFY_Battle_Projectile_Generate notify)
        {
            var battleSceneCtrl = BattleSceneControllerBase.Instance;
            if (battleSceneCtrl == null || CheckError(notify.Header))
                return;

            var battleGenProj = notify.Generate;

            var userChar = battleSceneCtrl.Dic_CharacterByUserNo.GetOrNull(battleGenProj.UserUniqueKey);
            if (userChar == null || !userChar.IsNetChar)
                return;

            userChar.CharCtrl.Excute_BattleGenerateProj(battleGenProj);
        }

        public static void Send_BattleReleaseProj(long projUniqueKey, Vector3 vPos)
        {
            var send_BattleReleaseProj = new SEND_Battle_Projectile_Release()
            {
                Header = Managers.Net.CreateHeader(),
                Release = new Battle_Projectile_Release()
                {
                    EventTime = UserData.TimeInfo.GetServerTimeMilliSec(),
                    UserUniqueKey = UserData.AccountInfo.UserNo,

                    FxUniqueKey = (int)projUniqueKey,
                    PositionVector = vPos.ToNetVector(),
                },
            };

            Managers.Net.Send(send_BattleReleaseProj);
        }

        public static void Notify_BattleReleaseProj(UserAgent userAgent, NOTIFY_Battle_Projectile_Release notify)
        {
            var battleSceneCtrl = BattleSceneControllerBase.Instance;
            if (battleSceneCtrl == null || CheckError(notify.Header))
                return;

            var battleRlsProj = notify.Release;

            var userChar = battleSceneCtrl.Dic_CharacterByUserNo.GetOrNull(battleRlsProj.UserUniqueKey);
            if (userChar == null || !userChar.IsNetChar)
                return;

            userChar.CharCtrl.Excute_Battle_ReleaseProj(battleRlsProj);
        }
    }
}