using BubbleFighter.Network.Protocol;
using Game.Character;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class BattleSceneController : BattleSceneControllerBase
    {
        private Dictionary<BattleSceneStateType, BattleSceneStateBase> _dic_BattleSceneState = null;

        private BattleSceneStateType _currStateType = BattleSceneStateType.None;
        private BattleSceneStateBase _currState = null;

        private SortedDictionary<int, SortedDictionary<long, CharacterObject>> _dic_CharByTeamNo = new SortedDictionary<int, SortedDictionary<long, CharacterObject>>();

        private void Update()
        {
            if (_currState != null)
                _currState.OnUpdate();
        }

        protected override IEnumerator Init()
        {
            // UI 초기화
            Managers.UI.Clear(true);

            var gameRoom = UserData.RoomInfo.GameRoom;

            // Scene State 초기화
            _dic_BattleSceneState = new Dictionary<BattleSceneStateType, BattleSceneStateBase>();

            _dic_BattleSceneState.Add(BattleSceneStateType.Ready, new BattleSceneState_Ready());
            _dic_BattleSceneState.Add(BattleSceneStateType.Start, new BattleSceneState_Start());
            _dic_BattleSceneState.Add(BattleSceneStateType.Playing, new BattleSceneState_Playing());
            _dic_BattleSceneState.Add(BattleSceneStateType.End, new BattleSceneState_End());

            foreach(var pair in _dic_BattleSceneState)
                pair.Value.Init(this);

            /* 맵 로딩 */
            var prf_World = Resources.Load<GameObject>("World/World_Test/World_Test");
            if (prf_World == null)
                yield break;

            var go_World = GameObject.Instantiate(prf_World);
            World = go_World.GetComponent<World>();
            
            go_World.transform.Reset();

            /* 캐릭터 로딩 */
            var prf_MyChar = Resources.Load<GameObject>("Character/Prefab_Character/MyCharacter");

            // 유저 생성 :: 유저 정보 정리
            var myTeamNo = -1;
            var dic_Users = new Dictionary<long, ST_GameRoomUser>();
            for (int i = 0; i < gameRoom.Users.Count; ++i)
            {
                var userInfo = gameRoom.Users[i];
                if (userInfo.UserNo == UserData.AccountInfo.UserNo)
                    myTeamNo = userInfo.Team;

                dic_Users.AddOrRefresh(userInfo.UserNo, userInfo);
            }

            // 유저 생성 :: 캐릭터 오브젝트 생성
            var dic_TeamMemberIndex = new Dictionary<int, int>();
            for (int i = 0; i < gameRoom.Users.Count; ++i)
            {
                var userInfo = gameRoom.Users[i];

                // 데이터
                var charData = Managers.GameData.GetData<CharacterData>(userInfo.CharacterDataId);
                var weaponData = Managers.GameData.GetData<WeaponData>(userInfo.WeaponDataId);
                var list_Actions = Managers.GameData.GetActionData(weaponData.RootActionName);
                
                // 캐릭터
                var prf_Char = prf_MyChar;
                var go_Char = GameObject.Instantiate(prf_Char);
                var userChar = go_Char.GetComponent<CharacterObject>();

                // 캐릭터 :: 스폰 포인트 배치

                // 캐릭터 위치 정보 Index
                var teamMemberIndex = 0;
                if (dic_TeamMemberIndex.TryGetValue(userInfo.Team, out teamMemberIndex))
                {
                    teamMemberIndex = 0;
                    dic_TeamMemberIndex.Add(userInfo.Team, 1);
                }
                else
                {
                    dic_TeamMemberIndex[userInfo.Team] = teamMemberIndex + 1;
                }

                // 모델
                var prf_Model = Resources.Load<GameObject>(charData.ModelPrefabPath);
                var go_Model = GameObject.Instantiate(prf_Model);

                // 캐릭터 Init
                userChar.Init(userInfo, charData, weaponData, go_Model, list_Actions);
                userChar.SetLayer(myTeamNo, userInfo.Team, layerMask_MyCo, layerMask_EnemyCo);
                userChar.ActionCtrl.PlayAction(weaponData.RootActionName);

                // 캐릭터 위치
                var tr_SponePoint = World.GetSponeInfo(userInfo.Team, teamMemberIndex);
                userChar.SetPosition(tr_SponePoint.position);
                userChar.SetRotation(tr_SponePoint.rotation);

                // 생성된 캐릭터 정리
                var dic_Char = _dic_CharByTeamNo.GetOrCreate(userInfo.Team);
                dic_Char.AddOrRefresh(userInfo.UserNo, userChar);

                Dic_CharacterByUserNo.Add(userInfo.UserNo, userChar);
                
                if (userInfo.UserNo == UserData.AccountInfo.UserNo)
                {
                    MyCharacter = userChar;
                    userChar.CharCtrl.CtrlType = Character.CharacterController.ControllerType.Manual;
                }
                else
                {
                    userChar.CharCtrl.CtrlType = Character.CharacterController.ControllerType.Net;
                }
            }

            // 카메라 세팅
            GameCam.Init(World.StartVCam, World.List_VCamInfo);
            GameCam.SetTarget(MyCharacter, MyCharacter.LayerMask_Enemy, float.PositiveInfinity);

            /* 서버에 로딩 완료를 알린 후 레디 상태 대기 */
            NetProcess.Send_LoadingFinished();

            while (_currStateType != BattleSceneStateType.Ready)
                yield return null;
            
            yield return base.Init();
        }

        protected override void OnStartScene()
        {
            base.OnStartScene();

            ChangeState(BattleSceneStateType.Start);
        }

        public void ChangeState(BattleSceneStateType state)
        {
            if (_currStateType == state)
                return;

            if (_currState != null)
                _currState.OnEnd();

            _currStateType = state;

            _currState = _dic_BattleSceneState.GetOrNull(state);

            if (_currState != null)
                _currState.OnStart();
        }

        public override void StartByServer(long startTime, long finishTime)
        {
            base.StartByServer(startTime, finishTime);
            ChangeState(BattleSceneStateType.Ready);
        }

        public override void FinishByServer()
        {
            base.FinishByServer();
            ChangeState(BattleSceneStateType.End);
        }
    }
}