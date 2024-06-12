using BubbleFighter.Network.Protocol;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Game.GameInput;

namespace Game
{
    public class Window_BattleMain : WindowBase, InputInstance
    {
        public enum CamControlMode
        { 
            Swipe,
            Joystick,
#if UNITY_EDITOR
            Mouse,
#endif
        }

        [Header("Child Components")]
        [SerializeField]
        private UIRemainTime _uiRemainTime = null;
        [SerializeField]
        private ButtonEx _btn_Pause = null;

        [Header("GamePad")]
        [SerializeField]
        private Joystick _joystick = null;
        [SerializeField]
        private Joystick _joystick_Cam = null;
        [SerializeField]
        private SwipePanel _swipe_Cam = null;
        [SerializeField]
        private GameButton _GameBtn_00 = null;
        [SerializeField]
        private GameButton _GameBtn_01 = null;
        [SerializeField]
        private GameButton _GameBtn_02 = null;
        [SerializeField]
        private GameButton _GameBtn_03 = null;

        private CamControlMode _camControlMode = CamControlMode.Swipe;
        private float _camControlSencitive = 1f;

        protected override void Awake()
        {
            base.Awake();

            GameInput.InputInst = this;

            _btn_Pause.onClick.Subscribe(() => CloseSelf());

            _joystick.Init(JoystickCode.Joystick_1, GameInput.KeyCode.Joystick_1);
            _joystick_Cam.Init(JoystickCode.Joystick_2, GameInput.KeyCode.Joystick_2);
            _GameBtn_00.Init(GameInput.KeyCode.Button_1);
            _GameBtn_01.Init(GameInput.KeyCode.Button_2);
            _GameBtn_02.Init(GameInput.KeyCode.Button_3);
            _GameBtn_03.Init(GameInput.KeyCode.Button_4);
        }

        public override void OnEvent_OnLastDepth()
        {
            base.OnEvent_OnLastDepth();

            ChangeCamControlMode((CamControlMode)PlayerPrefs.GetInt("CamControlMode", (int)CamControlMode.Swipe));
            _camControlSencitive = PlayerPrefs.GetFloat("CamControlSencitive", 100f) * 0.01f;
        }

        private void Update()
        {
            Update_CamControl();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if ((GameInput.InputInst as Window_BattleMain) == this)
                GameInput.InputInst = null;
        }

        public override void OnEvent_AfterOpen()
        {
            base.OnEvent_AfterOpen();

            if (BattleSceneControllerBase.Instance != null)
            {
                _uiRemainTime.SetEndTime(BattleSceneControllerBase.Instance.GameFinishTime);
                _uiRemainTime.SetZeroCallback(() => _uiRemainTime.gameObject.SetActive(false));
            }

#if UNITY_EDITOR
            if (_camControlMode == CamControlMode.Mouse)
                Cursor.lockState = CursorLockMode.Locked;
#endif
        }

        public override void OnEvent_OutLastDepth(bool isClose)
        {
            base.OnEvent_OutLastDepth(isClose);

#if UNITY_EDITOR
            if (_camControlMode == CamControlMode.Mouse)
                Cursor.lockState = CursorLockMode.None;
#endif
        }

        public override bool CloseSelf()
        {
            // 전투 씬이 아니라면 닫는다.
            if (Managers.Scene.CurrScene != SceneID.Battle)
                return base.CloseSelf();

#if UNITY_EDITOR
            // 테스트 씬인 경우 닫지 않는다.
            if (BattleSceneControllerBase.Instance is TestCharacterSceneController)
                return false;
#endif

            Managers.UI.OpenWindow(WindowID.Window_BattlePause);
            return false;
        }

        private void Update_CamControl()
        {
#if UNITY_EDITOR
            // 테스트 코드
            if (Input.GetKeyDown(UnityEngine.KeyCode.Alpha1))
                ChangeCamControlMode(CamControlMode.Joystick);
            else if (Input.GetKeyDown(UnityEngine.KeyCode.Alpha2))
                ChangeCamControlMode(CamControlMode.Swipe);
            else if (Input.GetKeyDown(UnityEngine.KeyCode.Alpha3))
                ChangeCamControlMode(CamControlMode.Mouse);
#endif

            if (BattleSceneControllerBase.Instance == null || BattleSceneControllerBase.Instance.GameCam == null)
                return;

            var vCamMove = Vector2.zero;
            switch (_camControlMode)
            {
                case CamControlMode.Joystick:
                    {
                        vCamMove = _joystick_Cam.Asix;
                        vCamMove.x *= (300f * _camControlSencitive);
                        vCamMove.y *= (150f * _camControlSencitive);
                    }
                    break;
                case CamControlMode.Swipe:
                    {
                        vCamMove = _swipe_Cam.Swipe * (30f * _camControlSencitive); 
                    }
                    break;
#if UNITY_EDITOR
                case CamControlMode.Mouse:
                    {
                        vCamMove.x = Input.GetAxis("Mouse X");
                        vCamMove.y = Input.GetAxis("Mouse Y");
                        vCamMove *= (300f * _camControlSencitive);
                    }
                    break;
#endif
            }

            vCamMove *= Time.deltaTime;
            BattleSceneControllerBase.Instance.GameCam.AddRotationH(vCamMove.x);
            BattleSceneControllerBase.Instance.GameCam.AddRotationV(-vCamMove.y);
        }

        public void ChangeCamControlMode(CamControlMode mode)
        {
            _camControlMode = mode;

            _joystick_Cam.gameObject.SetActive(mode == CamControlMode.Joystick);
            _swipe_Cam.gameObject.SetActive(mode == CamControlMode.Swipe);
#if UNITY_EDITOR
            Cursor.lockState = mode == CamControlMode.Mouse ? CursorLockMode.Locked : CursorLockMode.None;
#endif
        }

        public Vector2 GetAsix(JoystickCode jCode)
        {
            switch(jCode)
            {
                case JoystickCode.Joystick_1: return _joystick.Asix;
                case JoystickCode.Joystick_2: return _joystick_Cam.Asix;
            }

            return Vector2.zero;
        }

        public Vector2 GetDir(JoystickCode jCode)
        {
            switch (jCode)
            {
                case JoystickCode.Joystick_1: return _joystick.Dir;
                case JoystickCode.Joystick_2: return _joystick_Cam.Dir;
            }

            return Vector2.zero;
        }

        public float GetPower(JoystickCode jCode)
        {
            switch (jCode)
            {
                case JoystickCode.Joystick_1: return _joystick.Power;
                case JoystickCode.Joystick_2: return _joystick_Cam.Power;
            }

            return 0f;
        }

        public bool GetKey(GameInput.KeyCode kCode)
        {
            switch (kCode)
            {
                case GameInput.KeyCode.Joystick_1: return _joystick.IsPress;
                case GameInput.KeyCode.Joystick_2: return _joystick_Cam.IsPress;
                case GameInput.KeyCode.Button_1: return _GameBtn_00.IsPress;
                case GameInput.KeyCode.Button_2: return _GameBtn_01.IsPress;
                case GameInput.KeyCode.Button_3: return _GameBtn_02.IsPress;
                case GameInput.KeyCode.Button_4: return _GameBtn_03.IsPress;
            }

            return false;
        }

        public bool GetKeyDown(GameInput.KeyCode kCode)
        {
            switch (kCode)
            {
                case GameInput.KeyCode.Joystick_1: return _joystick.IsDown;
                case GameInput.KeyCode.Joystick_2: return _joystick_Cam.IsDown;
                case GameInput.KeyCode.Button_1: return _GameBtn_00.IsDown;
                case GameInput.KeyCode.Button_2: return _GameBtn_01.IsDown;
                case GameInput.KeyCode.Button_3: return _GameBtn_02.IsDown;
                case GameInput.KeyCode.Button_4: return _GameBtn_03.IsDown;
            }

            return false;
        }

        public bool GetKeyUp(GameInput.KeyCode kCode)
        {
            switch (kCode)
            {
                case GameInput.KeyCode.Joystick_1: return _joystick.IsUp;
                case GameInput.KeyCode.Joystick_2: return _joystick_Cam.IsUp;
                case GameInput.KeyCode.Button_1: return _GameBtn_00.IsUp;
                case GameInput.KeyCode.Button_2: return _GameBtn_01.IsUp;
                case GameInput.KeyCode.Button_3: return _GameBtn_02.IsUp;
                case GameInput.KeyCode.Button_4: return _GameBtn_03.IsUp;
            }

            return false;
        }
    }
}
