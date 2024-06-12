using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{
    public partial class CharacterController
    {
        private static Action.TriggerEventType[][] triggerEventFromGameInput;

#if UNITY_EDITOR
        bool bUseKeyboard = false;
#endif

        private void OnAwake_Manual()
        {
            if (triggerEventFromGameInput == null)
            {
                triggerEventFromGameInput = new Action.TriggerEventType[][]
                {
                    new Action.TriggerEventType[] { Action.TriggerEventType.Joystick_1_Down, Action.TriggerEventType.Joystick_1_Press, Action.TriggerEventType.Joystick_1_Up },
                    new Action.TriggerEventType[] { Action.TriggerEventType.Joystick_2_Down, Action.TriggerEventType.Joystick_2_Press, Action.TriggerEventType.Joystick_2_Up },
                    new Action.TriggerEventType[] { Action.TriggerEventType.Button_1_Down  , Action.TriggerEventType.Button_1_Press  , Action.TriggerEventType.Button_1_Up   },
                    new Action.TriggerEventType[] { Action.TriggerEventType.Button_2_Down  , Action.TriggerEventType.Button_2_Press  , Action.TriggerEventType.Button_2_Up   },
                    new Action.TriggerEventType[] { Action.TriggerEventType.Button_3_Down  , Action.TriggerEventType.Button_3_Press  , Action.TriggerEventType.Button_3_Up   },
                    new Action.TriggerEventType[] { Action.TriggerEventType.Button_4_Down  , Action.TriggerEventType.Button_4_Press  , Action.TriggerEventType.Button_4_Up   },
                };
            }

            GameInput.OnAsixEvent += OnAsixEvent;
            GameInput.OnButtonEvent += OnButtonEvent;
        }

        private void OnUpdate_Manual()
        {
            if (BattleSceneController.Instance != null)
                this.OnAim = BattleSceneController.Instance.GameCam.HasAimHitInfo;

#if UNITY_EDITOR
            // Joystick 01 테스트 코드
            {
                var vInput = Vector2.zero;
                if (Input.GetKey(UnityEngine.KeyCode.W))
                    vInput.y += 1f;
                if (Input.GetKey(UnityEngine.KeyCode.S))
                    vInput.y -= 1f;
                if (Input.GetKey(UnityEngine.KeyCode.A))
                    vInput.x -= 1f;
                if (Input.GetKey(UnityEngine.KeyCode.D))
                    vInput.x += 1f;

                var fPower = vInput.magnitude;
                if (fPower > 0f)
                {
                    vInput *= (1f / fPower);
                    if (!bUseKeyboard)
                    {
                        bUseKeyboard = true;
                        GameInput.OnAsixEvent -= OnAsixEvent;
                    }

                    OnAsixEvent(GameInput.JoystickCode.Joystick_1, vInput, vInput, fPower);
                }
                else
                {
                    if (bUseKeyboard)
                    {
                        bUseKeyboard = false;
                        GameInput.OnAsixEvent += OnAsixEvent;
                    }
                }
            }
#endif
        }

        private void OnDestroy_Manual()
        {
            GameInput.OnAsixEvent -= OnAsixEvent;
            GameInput.OnButtonEvent -= OnButtonEvent;
        }

        public void OnAsixEvent(GameInput.JoystickCode jCode, Vector2 vInput, Vector2 vDir, float fPower)
        {
            if (CtrlType != ControllerType.Manual)
                return;

            GetInputInfo(jCode).SetInput(vInput, vDir, fPower);
        }

        public void OnButtonEvent(GameInput.KeyCode kCode, GameInput.KeyState state)
        {
            if (CtrlType != ControllerType.Manual)
                return;

            try
            {
                var eventType = triggerEventFromGameInput[(int)kCode][(int)state];
                Owner.ActionCtrl.OnTriggerEvent(eventType);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}