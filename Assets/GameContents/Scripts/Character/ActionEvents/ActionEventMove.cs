using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventMove : ActionEventDurationDataBase
    {
        public override bool IgnoreOnNetCharCtrl => true;

        public enum MoveType
        {
            Camera,
            Input,
            InputWithoutPower,
            Character
        }

        public bool GroundMove = false;
        public float MoveSpdPerSec; // 초당 이동 속도

        public MoveType moveType; // Move를 실행할 기준 점
        public float moveHorizontalDegreeds; // 기준 점에서 얼마나 회전한 방향으로 갈것인가
        public bool FixedToInitial; // 계속 추적할 것인가 첫 프레임의 방향으로 고정할 것인가

        public override IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime)
        {
            return new ActionEventMoveRuntime(owner, actionDataRuntime, this);
        }

        public override void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
        }
        
        public override ActionEventBase Clone()
        {
            return new ActionEventMove()
            {
                StartTime = this.StartTime,
                EndTime = this.EndTime,

                GroundMove = this.GroundMove,
                MoveSpdPerSec = this.MoveSpdPerSec,
                moveType = this.moveType,
                moveHorizontalDegreeds = this.moveHorizontalDegreeds,
                FixedToInitial = this.FixedToInitial,
            };
        }

#if UNITY_EDITOR
        public override void OnGUI(ActionData actionData, int index)
        {
            base.OnGUI(actionData, index);

            EditorGUILayout.BeginHorizontal();
            {
                GroundMove = EditorGUILayout.Toggle("GroundMove", GroundMove);
                MoveSpdPerSec = EditorGUILayout.FloatField("MoveSpdPerSec", MoveSpdPerSec);
                moveType = (MoveType)EditorGUILayout.EnumPopup("MoveType", moveType);
                moveHorizontalDegreeds = EditorGUILayout.FloatField("MoveDirDegreeds", moveHorizontalDegreeds);
                FixedToInitial = EditorGUILayout.Toggle("FixedToInitial", FixedToInitial);
            }
            EditorGUILayout.EndHorizontal();
        }
#endif
    }

    public class ActionEventMoveRuntime : ActionEventRuntimeDurationBase<ActionEventMove>
    {
        private Vector3 vWorldDir;
        private float moveFactor;

        public ActionEventMoveRuntime(CharacterObject owner, ActionRuntime actionDataRuntime, ActionEventMove data) : base(owner, actionDataRuntime, data)
        {

        }

        public override void Init()
        {
            return;
        }
        
        public override void OnStart(float actionElapsedTime)
        {
            base.OnStart(actionElapsedTime);
            
            moveFactor = 1f;
            if (_eventData.FixedToInitial)
            {
                switch (_eventData.moveType)
                {
                    case ActionEventMove.MoveType.Camera:
                        UpdateDir_Camera();
                        break;
                    case ActionEventMove.MoveType.Input:
                    case ActionEventMove.MoveType.InputWithoutPower:
                        UpdateDir_Input();
                        break;
                    case ActionEventMove.MoveType.Character:
                        UpdateDir_Character();
                        break;
                }

                _owner.SetMove(vWorldDir * (_eventData.MoveSpdPerSec));
            }

        }

        public override void OnUpdate(float actionElapsedTime)
        {
            base.OnUpdate(actionElapsedTime);

            var moveDist = _eventData.MoveSpdPerSec * deltaTime;

            if (!_eventData.FixedToInitial)
            {
                switch (_eventData.moveType)
                {
                    case ActionEventMove.MoveType.Camera:
                        UpdateDir_Camera();
                        break;
                    case ActionEventMove.MoveType.Input:
                        UpdateDir_Input();
                        moveFactor = _owner.CharCtrl.GetInputInfo(GameInput.JoystickCode.Joystick_1).fPower;
                        break;
                    case ActionEventMove.MoveType.InputWithoutPower:
                        UpdateDir_Input();
                        break;
                    case ActionEventMove.MoveType.Character:
                        UpdateDir_Character();
                        break;
                }
            }

            var vMove = vWorldDir * (_eventData.MoveSpdPerSec * moveFactor);
            _owner.SetMove(vMove);
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

        }

        public override void OnEnd()
        {
            base.OnEnd();

            _owner.SetMove(Vector3.zero);
        }

        private void UpdateDir_Camera()
        {
            // 카메라 방향에서 중력 Up의 수직인 방향Vector를 구한다.
            var vCamLook = BattleSceneControllerBase.Instance.GameCam.CamForward;
            var vCamRight = Vector3.Cross(vCamLook, _owner.WorldUp);
            vCamLook = Vector3.Cross(_owner.WorldUp, vCamRight);
            var newRot = Quaternion.LookRotation(vCamLook, _owner.WorldUp);

            // Data 보정
            if (_eventData.moveHorizontalDegreeds != 0f)
                newRot *= Quaternion.Euler(0, _eventData.moveHorizontalDegreeds, 0f);

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

            vWorldDir = newRot * Vector3.forward;
        }

        private void UpdateDir_Character()
        {
            var newRot = _owner.transform.rotation;

            // Data 보정
            if (_eventData.moveHorizontalDegreeds != 0f)
                newRot *= Quaternion.Euler(0, _eventData.moveHorizontalDegreeds, 0f);

            vWorldDir = newRot * Vector3.forward;
        }
    }
}