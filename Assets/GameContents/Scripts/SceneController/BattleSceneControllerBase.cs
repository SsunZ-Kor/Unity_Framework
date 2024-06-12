using BubbleFighter.Network.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Character;

namespace Game
{
    public abstract class BattleSceneControllerBase : SceneControllerBase<BattleSceneControllerBase>
    {
        [SerializeField]
        protected GameCamera _gameCam = null;
        public GameCamera GameCam => _gameCam;

        public long GameStartTime { get; protected set; }
        public long GameFinishTime { get; protected set; }

        public CharacterObject MyCharacter { get; protected set; }

        public Dictionary<long, CharacterObject> Dic_CharacterByUserNo = new Dictionary<long, CharacterObject>();

        public World World { get; protected set; }

        // 레이어 세팅
        protected int layerMask_MyCo { get; private set; }
        protected int layerMask_EnemyCo { get; private set; }

        // 타격 패킷 저장 :: LateUpdate에서 일괄처리
        public List<ST_BattleHitEffect> _list_PanddingAtkHitInfo = new List<ST_BattleHitEffect>(10);

        protected override void Awake()
        {
            layerMask_MyCo = LayerMask.GetMask("MyCompany_Character", "MyCompany_Object");
            layerMask_EnemyCo = LayerMask.GetMask("EnemyCompany_Character", "EnemyCompany_Object");

            base.Awake();
        }

        private void LateUpdate()
        {
            // 쌓인 HitInfo 보내기
            if (_list_PanddingAtkHitInfo.Count > 0)
            {
                if (IntroSceneController.UseUDP)
                    NetProcess.UDP_Send_BattleHitEffect(_list_PanddingAtkHitInfo);
                else
                    NetProcess.Send_BattleHitEffect(_list_PanddingAtkHitInfo);

                _list_PanddingAtkHitInfo.Clear();
            }
        }

        public virtual void StartByServer(long startTime, long endTime)
        {
            GameStartTime = startTime;
            GameFinishTime = endTime;
        }

        public virtual void FinishByServer()
        {
        }

        public virtual void JoinGameRoomUser(long userNo)
        {
            var netChar = Dic_CharacterByUserNo.GetOrNull(userNo);
            if (netChar == null || netChar.UserInfo == null)
                return;

            Managers.UI.EnqueueToast($"\"{netChar.UserInfo.UserName}\"님이 다시 접속하였습니다.");
        }

        public virtual void LeaveGameRoomUser(long userNo)
        {
            var netChar = Dic_CharacterByUserNo.GetOrNull(userNo);
            if (netChar == null || netChar.UserInfo == null)
                return;
         
            Managers.UI.EnqueueToast($"\"{netChar.UserInfo.UserName}\"님이 게임을 떠났습니다.");
        }

        public virtual void ExcuteHitEffect(CharacterObject attacker, AttackableObject victim, AttackableObject.AttackRuntime atkRuntime, float damageFactor, Vector3 atkPos)
        {
            if (victim is CharacterObject)
            {
                var victimChar = victim as CharacterObject;

                var newAtkHitInfo = CreateAtkHitInfo(attacker, victimChar, atkRuntime, damageFactor, atkPos);
                if (newAtkHitInfo == null)
                    return;

                _list_PanddingAtkHitInfo.Add(newAtkHitInfo);

                if (!victimChar.IsNetChar)
                {
                    victimChar.CharCtrl.Excute_BattleHitEffect(newAtkHitInfo);
                }
            }
        }

        private ST_BattleHitEffect CreateAtkHitInfo(CharacterObject attacker, CharacterObject victim, AttackableObject.AttackRuntime atkRuntime, float damageFactor, Vector3 atkPos)
        {
            if (atkRuntime == null || attacker == null || victim == null)
            {
                if (atkRuntime == null)
                    Debug.LogError("BattleSceneControllerBase->CreateNetHitEffect :: \"atkRuntime\" is Null");
                if (attacker == null)
                    Debug.LogError("BattleSceneControllerBase->CreateNetHitEffect :: \"attacker\" is Null");
                if (victim == null)
                    Debug.LogError("BattleSceneControllerBase->CreateNetHitEffect :: \"victim\" is Null");
                return null;
            }

            var newAtkHitInfo = new ST_BattleHitEffect();

            // 가해자, 피해자 세팅
            newAtkHitInfo.AttackerUserNo = attacker.UserInfo.UserNo;
            newAtkHitInfo.VictimUserNo = victim.UserInfo.UserNo;

            // Transform Event 세팅
            newAtkHitInfo.HitType = atkRuntime.Data.hitType.ToNetType();
            newAtkHitInfo.HitOffset = atkRuntime.Data.hitOffset.ToNetVector();
            newAtkHitInfo.HitPower = atkRuntime.Data.hitPower;

            switch (atkRuntime.Data.hitType)
            {
                case AttackableObject.HitType.AttachToMe:
                    newAtkHitInfo.ParentUserNo = attacker.UserInfo.UserNo;
                    newAtkHitInfo.ChildUserNo = victim.UserInfo.UserNo;
                    break;
                case AttackableObject.HitType.AttachToOther:
                    newAtkHitInfo.ParentUserNo = victim.UserInfo.UserNo;
                    newAtkHitInfo.ChildUserNo = attacker.UserInfo.UserNo;
                    break;
                default:
                    newAtkHitInfo.ParentUserNo = 0L;
                    newAtkHitInfo.ChildUserNo = 0L;
                    break;
            }

            // Todo :: CharacterBase.Stat.HitReaction에 따른 데미지, 이펙트 분기
            // Hit Fx And Damage
            newAtkHitInfo.DamageFactor = damageFactor;
            newAtkHitInfo.HitFxIdx = atkRuntime.idx_HitFx;
            newAtkHitInfo.HitSfxIdx = atkRuntime.idx_HitSfx;
            newAtkHitInfo.HitFxPosition = atkPos.ToNetVector();

            return newAtkHitInfo;
        }
    }
}
