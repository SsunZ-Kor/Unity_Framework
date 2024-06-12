using BubbleFighter.Network.Protocol;
using Game.Character;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static partial class NetProcess
    {
        #region Util
        public static Vector ToNetVector(this Vector3 v3)
        {
            return new Vector()
            {
                X = v3.x,
                Y = v3.y,
                Z = v3.z,
            };
        }

        public static Vector3 ToVector3(this Vector v)
        {
            return new Vector3()
            {
                x = v.X,
                y = v.Y,
                z = v.Z,
            };
        }

        public static Vector ToNetVector(this Quaternion q)
        {
            return q.eulerAngles.ToNetVector();
        }

        public static Quaternion ToQuaternion(this Vector v)
        {
            return Quaternion.Euler(v.X, v.Y, v.Z);
        }

        public static ENUM_HitType ToNetType(this CharacterObject.HitType type)
        {
            switch (type)
            {
                case AttackableObject.HitType.None:
                    return ENUM_HitType.None;
                case AttackableObject.HitType.Move:
                    return ENUM_HitType.Move;
                case AttackableObject.HitType.AttachToMe:
                    return ENUM_HitType.AttachToMe;
                case AttackableObject.HitType.AttachToOther:
                    return ENUM_HitType.AttachToOther;
            }

            return ENUM_HitType.None;
        }

        public static CharacterObject.HitType ToDataType(this ENUM_HitType type)
        {
            switch (type)
            {
                case ENUM_HitType.None:
                    return AttackableObject.HitType.None;
                case ENUM_HitType.Move:
                    return AttackableObject.HitType.Move;
                case ENUM_HitType.AttachToMe:
                    return AttackableObject.HitType.AttachToMe;
                case ENUM_HitType.AttachToOther:
                    return AttackableObject.HitType.AttachToOther;
            }

            return AttackableObject.HitType.None;
        }
        #endregion

        public static void UDP_Send_BattleMove(bool useVel, Vector3 vVel, Vector3 vMove, Vector3 vPos, Quaternion qRot, int gravityFeildIdx)
        {
            var send = new UDP_Battle_Move()
            {
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

            Managers.Net.HybridSend(ENUM_UDP_MSGID.MsgidBattleMove, send); //ALEX2020.10.21 UDP 전송 적용
        }

        public static void UDP_Recive_BattleMove(UDP_Battle_Move rec)
        {
            var battleSceneCtrl = BattleSceneControllerBase.Instance;
            if (battleSceneCtrl == null)
                return;

            var battleMove = rec.Move;

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

        public static void UDP_Send_BattleAction(int actionIdx)
        {
            var send = new UDP_Battle_Action()
            {
                Action = new Battle_Action()
                {
                    EventTime = UserData.TimeInfo.GetServerTimeMilliSec(),
                    UserUniqueKey = UserData.AccountInfo.UserNo,

                    ActionIndex = actionIdx,
                },
            };

            //Managers.Net.Send(send);
            Managers.Net.HybridSend(ENUM_UDP_MSGID.MsgidBattleAction, send); //ALEX2020.10.21 UDP 전송 적용
        }

        public static void UDP_Recive_BattleAction(UDP_Battle_Action rec)
        {
            var battleSceneCtrl = BattleSceneControllerBase.Instance;
            if (battleSceneCtrl == null)
                return;

            var battleAction = rec.Action;

            // 내 캐릭터 예외처리
            if (battleSceneCtrl.MyCharacter.UserInfo.UserNo == battleAction.UserUniqueKey)
                return;

            var userChar = BattleSceneControllerBase.Instance.Dic_CharacterByUserNo.GetOrNull(battleAction.UserUniqueKey);
            if (userChar == null || !userChar.IsNetChar)
                return;

            userChar.CharCtrl.Excute_Action(battleAction);
        }

        public static void UDP_Send_BattleHitEffect(List<ST_BattleHitEffect> list_AtkHitInfo)
        {
            if (list_AtkHitInfo == null || list_AtkHitInfo.Count == 0)
                return;

            var send = new UDP_BattleHitEffect();
            send.BattleHitEffect.AddRange(list_AtkHitInfo);

            //Managers.Net.Send(send);
            Managers.Net.HybridSend(ENUM_UDP_MSGID.MsgidBattleHitEffect, send); //ALEX2020.10.21 UDP 전송 적용
        }

        public static void UDP_Recive_BattleHitEffect(UDP_BattleHitEffect rec)
        {
            if (BattleSceneControllerBase.Instance == null)
                return;

            if (rec.BattleHitEffect.Count == 0)
                return;

            for (int i = 0; i < rec.BattleHitEffect.Count; ++i)
            {
                var atkHitInfo = rec.BattleHitEffect[i];
                if (atkHitInfo == null)
                    continue;

                var userChar = BattleSceneControllerBase.Instance.Dic_CharacterByUserNo.GetOrNull(atkHitInfo.VictimUserNo);
                if (userChar == null || !userChar.IsNetChar)
                    continue;

                userChar.CharCtrl.Excute_BattleHitEffect(atkHitInfo);
            }
        }

        public static void UDP_Send_BattleGenerateProj(int projIdx, long projUniqueKey, Vector3 vPos, Quaternion qRot, int parentIdx)
        {
            var send = new UDP_Battle_Projectile_Generate()
            {
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

            //Managers.Net.Send(send);
            Managers.Net.HybridSend(ENUM_UDP_MSGID.MsgidBattleProjectileGenerate, send); //ALEX2020.10.21 UDP 전송 적용
        }

        public static void UDP_Recive_BattleGenerateProj(UDP_Battle_Projectile_Generate rec)
        {
            if (BattleSceneControllerBase.Instance == null)
                return;

            var battleGenProj = rec.Generate;

            var userChar = BattleSceneControllerBase.Instance.Dic_CharacterByUserNo.GetOrNull(battleGenProj.UserUniqueKey);
            if (userChar == null || !userChar.IsNetChar)
                return;

            userChar.CharCtrl.Excute_BattleGenerateProj(battleGenProj);
        }

        public static void UDP_Send_BattleReleaseProj(long projUniqueKey, Vector3 vPos)
        {
            var send = new UDP_Battle_Projectile_Release()
            {
                Release = new Battle_Projectile_Release()
                {
                    EventTime = UserData.TimeInfo.GetServerTimeMilliSec(),
                    UserUniqueKey = UserData.AccountInfo.UserNo,

                    FxUniqueKey = (int)projUniqueKey,
                    PositionVector = vPos.ToNetVector(),
                },
            };

            //Managers.Net.Send(send);
            Managers.Net.HybridSend(ENUM_UDP_MSGID.MsgidBattleProjectileRelease, send); //ALEX2020.10.21 UDP 전송 적용
        }

        public static void UDP_Recive_BattleReleaseProj(UDP_Battle_Projectile_Release rec)
        {
            if (BattleSceneControllerBase.Instance == null)
                return;

            var battleRlsProj = rec.Release;

            var userChar = BattleSceneControllerBase.Instance.Dic_CharacterByUserNo.GetOrNull(battleRlsProj.UserUniqueKey);
            if (userChar == null || !userChar.IsNetChar)
                return;

            userChar.CharCtrl.Excute_Battle_ReleaseProj(battleRlsProj);
        }

        public static void UDP_Receive_Ping(UDP_PING rec)
        {
            Debug.Log($"Received UDP PING FROM : {rec.Info}:{rec.Address}:{rec.Port}");

            var msg = new UDP_PONG()
            {
                Info = UserData.AccountInfo.NickName,
            };

            Managers.Net.SendUDP(rec.Address, rec.Port, ENUM_UDP_MSGID.MsgidPong, msg);
        }

        public static void UDP_Receive_Pong(UDP_PONG rec)
        {
            Debug.Log($"Received UDP PONG : {rec.Info}");
        }
    }
}