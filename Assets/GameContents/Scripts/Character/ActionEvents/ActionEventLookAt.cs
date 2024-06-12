using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventLookAt : ActionEventDurationDataBase
    {
        public enum LookAtType
        { 
            Camera,
            CameraAim,
            Input,
        }

        public override bool IgnoreOnNetCharCtrl => true;

        public LookAtType lookAtType;
        public float angularVelocity; // 해당 방향과 동기화 각속도
        public bool FixedToInitial; // 계속 추적할 것인가 첫 프레임의 방향으로 고정할 것인가

        public override IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime)
        {
            return new ActionEventLookAtRuntime(owner, actionDataRuntime, this);
        }

        public override void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
        }
        
        public override ActionEventBase Clone()
        {
            return new ActionEventLookAt()
            {
                StartTime = this.StartTime,
                EndTime = this.EndTime,

                lookAtType = this.lookAtType,
                angularVelocity = this.angularVelocity,
                FixedToInitial = this.FixedToInitial, 
            };
        }

#if UNITY_EDITOR
        public override void OnGUI(ActionData actionData, int index)
        {
            base.OnGUI(actionData, index);

            EditorGUILayout.BeginHorizontal();
            {
                lookAtType = (LookAtType)EditorGUILayout.EnumPopup("LookAtType", lookAtType);
                angularVelocity = EditorGUILayout.FloatField("AngularVelocity", angularVelocity);
                FixedToInitial = EditorGUILayout.Toggle("FixedToInitial", FixedToInitial);
            }
            EditorGUILayout.EndHorizontal();
        }
#endif
    }

    public class ActionEventLookAtRuntime : ActionEventRuntimeDurationBase<ActionEventLookAt>
    {
        private Quaternion qWorldDir;

        private float debugTime;

        public ActionEventLookAtRuntime(CharacterObject owner, ActionRuntime actionDataRuntime, ActionEventLookAt data) : base(owner, actionDataRuntime, data)
        {

        }

        public override void Init()
        {
        }

        public override void OnStart(float actionElapsedTime)
        {
            base.OnStart(actionElapsedTime);

            if (_eventData.FixedToInitial)
            {
                switch (_eventData.lookAtType)
                {
                    case ActionEventLookAt.LookAtType.Camera:
                        UpdateDir_Camera();
                        break;
                    case ActionEventLookAt.LookAtType.CameraAim:
                        UpdateDir_CameraAim();
                        break;
                    case ActionEventLookAt.LookAtType.Input:
                        UpdateDir_Input();
                        break;
                }

                _owner.SetLook(qWorldDir, _eventData.angularVelocity);
            }
        }

        public override void OnUpdate(float actionElapsedTime)
        {
            base.OnUpdate(actionElapsedTime);

            if (!_eventData.FixedToInitial)
            {
                switch (_eventData.lookAtType)
                {
                    case ActionEventLookAt.LookAtType.Camera:
                        UpdateDir_Camera();
                        break;
                    case ActionEventLookAt.LookAtType.CameraAim:
                        UpdateDir_CameraAim();
                        break;
                    case ActionEventLookAt.LookAtType.Input:
                        UpdateDir_Input();
                        break;
                }

                _owner.SetLook(qWorldDir, _eventData.angularVelocity);
            }
        }

        public override void OnEnd()
        {
            base.OnEnd();

            _owner.SetLook(Quaternion.LookRotation(_owner.transform.forward, _owner.WorldUp), 1440f);
        }

        private void UpdateDir_Camera()
        {
            // 카메라 방향에서 중력 Up의 수직인 방향Vector를 구한다.
            var vCamLook = BattleSceneControllerBase.Instance.GameCam.CamForward;
            var vCamRight = Vector3.Cross(vCamLook, _owner.WorldUp);
            vCamLook = Vector3.Cross(_owner.WorldUp, vCamRight);

            qWorldDir = Quaternion.LookRotation(vCamLook, _owner.WorldUp);
        }

        private void UpdateDir_CameraAim()
        {
            // Aim 방향에서 중력 Up의 수직인 방향Vector를 구한다.
            var vAimDir = BattleSceneControllerBase.Instance.GameCam.AimDir;
            var vAimRight = Vector3.Cross(vAimDir, _owner.WorldUp);
            vAimDir = Vector3.Cross(_owner.WorldUp, vAimRight);

            qWorldDir = Quaternion.LookRotation(vAimDir, _owner.WorldUp);
        }

        private void UpdateDir_Input()
        {
            var vInput = _owner.CharCtrl.GetInputInfo(GameInput.JoystickCode.Joystick_1).vInput;
            if (vInput == Vector2.zero)
            {
                qWorldDir = _owner.transform.rotation;
            }
            else
            {
                // 카메라 방향에서 중력 Up의 수직인 방향Vector를 구한다.
                var vAimDir = BattleSceneControllerBase.Instance.GameCam.AimDir;
                var vAimRight = Vector3.Cross(vAimDir, _owner.WorldUp);
                vAimDir = Vector3.Cross(_owner.WorldUp, vAimRight);

                qWorldDir = Quaternion.LookRotation(vAimDir, _owner.WorldUp);

                // Input 보정
                qWorldDir *= Quaternion.LookRotation(new Vector3(vInput.x, 0f, vInput.y), Vector3.up);
            }
        }
    }
}