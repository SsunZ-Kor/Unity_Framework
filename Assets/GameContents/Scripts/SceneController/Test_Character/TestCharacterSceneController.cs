using Game.Character;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class TestCharacterSceneController : BattleSceneControllerBase
    {
#if UNITY_EDITOR
        protected override bool IsTestScene => true;
#endif
        [SerializeField]
        private World _world = null;
        [SerializeField]
        private GameObject _myChar = null;
        [SerializeField]
        private GameObject _prf_TestCh = null;
        [SerializeField]
        private string _rootActionName = null;

        [SerializeField]
        private CharacterData _charData = null;
        [SerializeField]
        private WeaponData _weaponData = null;


        protected override IEnumerator Init()
        {
            Managers.UI.Clear();
            Managers.UI.ChangeBackground(BackgroundID.BG_None, 0, null);

            World = _world;

            if (_prf_TestCh == null)
            {
                Debug.LogError("_prf_TestCh is Null");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(_rootActionName))
            {
                Debug.LogError("_rootActionName is Null or White Space");
                yield break;
            }

            var list_ActionData = Managers.GameData.GetActionData(_rootActionName);
            if (list_ActionData == null || list_ActionData.Count == 0)
            {
                Debug.LogError("_rootActionName is Not Found");
                yield break;
            }

            // 프리펩, 레이어 준비
            var go_TestCh = GameObject.Instantiate(_prf_TestCh);

            // MyCharacter 세팅
            var go_MyChar = GameObject.Instantiate(_myChar);
            var myChar = go_MyChar.GetComponent<CharacterObject>();
            myChar.Init(null, _charData, _weaponData, go_TestCh, list_ActionData);
            myChar.SetLayer(1, 1, layerMask_MyCo, layerMask_EnemyCo);
            myChar.ActionCtrl.PlayAction(_rootActionName);

            myChar.SetPosition(Vector3.up * 20f);

            // 게임 패드 On
            Managers.UI.OpenWindow(WindowID.Window_BattleMain);

            // 카메라 세팅
            GameCam.Init(World.StartVCam, World.List_VCamInfo);
            GameCam.SetTarget(myChar, layerMask_EnemyCo, float.PositiveInfinity);

            yield return base.Init();
        }
    }
}