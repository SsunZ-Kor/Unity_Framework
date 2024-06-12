using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventMoveByVelocity : ActionEventBase
    {
        public override bool IgnoreOnNetCharCtrl => true;

        public enum MoveType
        {
            Camera,
            Input,
            Character
        }

        public float MoveSpdPerSec; // 초당 이동 속도

        public MoveType moveType; // Move를 실행할 기준 점
        public float moveHorizontalDegreeds; // 기준 점에서 얼마나 회전한 방향으로 갈것인가 (+Left -right)
        public float moveVerticalDegreeds; // 기준 점에서 얼마나 회전한 방향으로 갈것인가 (+down -up)

        public override IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime)
        {
            return new ActionEventMoveByVelocityRuntime(owner, actionDataRuntime, this);
        }

        public override void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
        }

        public override ActionEventBase Clone()
        {
            return new ActionEventMoveByVelocity()
            {
                StartTime = this.StartTime,

                MoveSpdPerSec = this.MoveSpdPerSec,
                moveType = this.moveType,
                moveHorizontalDegreeds = this.moveHorizontalDegreeds,
                moveVerticalDegreeds = this.moveVerticalDegreeds,
            };
        }

#if UNITY_EDITOR
        public override void OnGUI(ActionData actionData, int index)
        {
            base.OnGUI(actionData, index);

            EditorGUILayout.BeginHorizontal();
            {
                MoveSpdPerSec = EditorGUILayout.FloatField("MoveSpdPerSec", MoveSpdPerSec);
                moveType = (MoveType)EditorGUILayout.EnumPopup("MoveType", moveType);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                moveHorizontalDegreeds = EditorGUILayout.FloatField("moveHorizontalDegreeds", moveHorizontalDegreeds);
                EditorGUILayout.LabelField("좌 : - / 우 : +");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                moveVerticalDegreeds = EditorGUILayout.FloatField("moveVerticalDegreeds", moveVerticalDegreeds);
                EditorGUILayout.LabelField("상 : - / 하 : +");
            }
            EditorGUILayout.EndHorizontal();
        }
#endif
    }

    public class ActionEventMoveByVelocityRuntime : ActionEventRuntimeBase<ActionEventMoveByVelocity>
    {
        private Vector3 vWorldDir;

        public ActionEventMoveByVelocityRuntime(CharacterObject owner, ActionRuntime actionDataRuntime, ActionEventMoveByVelocity data) : base(owner, actionDataRuntime, data)
        {

        }

        public override void Init()
        {
        }

        public override void OnStart(float actionElapsedTime)
        {
            base.OnStart(actionElapsedTime);

            switch (_eventData.moveType)
            {
                case ActionEventMoveByVelocity.MoveType.Camera:
                    UpdateDir_Camera();
                    break;
                case ActionEventMoveByVelocity.MoveType.Input:
                    UpdateDir_Input();
                    break;
                case ActionEventMoveByVelocity.MoveType.Character:
                    UpdateDir_Character();
                    break;
            }

            var velocity = vWorldDir * _eventData.MoveSpdPerSec;
            _owner.SetVelocity(velocity);
        }

        private void UpdateDir_Camera()
        {
            // 카메라 방향에서 중력 Up의 수직인 방향Vector를 구한다.
            var vCamLook = BattleSceneControllerBase.Instance.GameCam.CamForward;
            var vCamRight = Vector3.Cross(vCamLook, _owner.WorldUp);
            vCamLook = Vector3.Cross(_owner.WorldUp, vCamRight);
            var newRot = Quaternion.LookRotation(vCamLook, _owner.WorldUp);

            // Data 보정
            newRot *= Quaternion.Euler(0, _eventData.moveHorizontalDegreeds, 0f);
            newRot *= Quaternion.Euler(_eventData.moveVerticalDegreeds, 0f, 0f);

            vWorldDir = newRot * Vector3.forward;
        }

        private void UpdateDir_Input()
        {
            var vInput = _owner.CharCtrl.GetInputInfo(GameInput.JoystickCode.Joystick_1).vInput;
            if (vInput == Vector2.zero)
                vInput = Vector2.up;

            // 카메라 방향에서 중력 Up의 수직인 방향Vector를 구한다.
            var vCamLook = BattleSceneControllerBase.Instance.GameCam.CamForward;
            var vCamRight = Vector3.Cross(vCamLook, _owner.WorldUp);
            vCamLook = Vector3.Cross(_owner.WorldUp, vCamRight);
            var newRot = Quaternion.LookRotation(vCamLook, _owner.WorldUp);
            
            // Input 보정
            newRot *= Quaternion.LookRotation(new Vector3(vInput.x, 0f, vInput.y), Vector3.up);

            // Data 보정
            if (_eventData.moveHorizontalDegreeds != 0f)
                newRot *= Quaternion.Euler(0, _eventData.moveHorizontalDegreeds, 0f);
            if (_eventData.moveVerticalDegreeds != 0f)
                newRot *= Quaternion.Euler(_eventData.moveVerticalDegreeds, 0f, 0f);

            vWorldDir = newRot * Vector3.forward;
        }

        private void UpdateDir_Character()
        {
            var newRot = _owner.transform.rotation;

            // Data 보정
            newRot *= Quaternion.Euler(0, _eventData.moveHorizontalDegreeds, 0f);
            newRot *= Quaternion.Euler(_eventData.moveVerticalDegreeds, 0f, 0f);

            vWorldDir = newRot * Vector3.forward;
        }
    }
}