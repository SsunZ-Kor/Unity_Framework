using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventCamMove : ActionEventBase
    {
        public override bool IgnoreOnNetCharCtrl => true;

        public GameCamera.CamMoveInfo CamMoveInfo;
        public bool bResetCamPos = false;

        public override ActionEventBase Clone()
        {
            return new ActionEventCamMove()
            {
                bResetCamPos = this.bResetCamPos,
                CamMoveInfo = new GameCamera.CamMoveInfo()
                {
                    AddOffset = this.CamMoveInfo?.AddOffset ?? Vector3.zero,
                    ReachTime = this.CamMoveInfo?.ReachTime ?? 0f,
                    StayTime = this.CamMoveInfo?.StayTime ?? 0f,
                    ComebackTime = this.CamMoveInfo?.ComebackTime ?? 0f,
                },
            };
        }

        public override IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime)
        {
            return new ActionEventCamMoveRuntime(owner, actionDataRuntime, this);
        }

        public override void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
        }

#if UNITY_EDITOR
        public override void OnGUI(ActionData actionData, int index)
        {
            base.OnGUI(actionData, index);

                if (CamMoveInfo == null)
                    CamMoveInfo = new GameCamera.CamMoveInfo();

            EditorGUILayout.BeginHorizontal();
            {
                CamMoveInfo.AddOffset = EditorGUILayout.Vector3Field("Offset", CamMoveInfo.AddOffset);
                bResetCamPos = EditorGUILayout.Toggle("ResetCamPos", bResetCamPos);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                CamMoveInfo.ReachTime = EditorGUILayout.FloatField("ReachTime", CamMoveInfo.ReachTime);
                CamMoveInfo.StayTime = EditorGUILayout.FloatField("StayTime", CamMoveInfo.StayTime);
                CamMoveInfo.ComebackTime = EditorGUILayout.FloatField("ComebackTime", CamMoveInfo.ComebackTime);
            }
            EditorGUILayout.EndHorizontal();
        }
#endif
    }

    public class ActionEventCamMoveRuntime : ActionEventRuntimeBase<ActionEventCamMove>
    {

        public ActionEventCamMoveRuntime(CharacterObject owner, ActionRuntime actionDataRuntime, ActionEventCamMove data) : base(owner, actionDataRuntime, data)
        {

        }

        public override void Init()
        {
        }

        public override void OnStart(float actionElapsedTime)
        {
            base.OnStart(actionElapsedTime);

            if (BattleSceneControllerBase.Instance != null)
                BattleSceneControllerBase.Instance.GameCam.MoveOffsetEffect(_eventData.CamMoveInfo, _eventData.bResetCamPos);
        }
    }
}
