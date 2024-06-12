using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character.Action
{
    public enum JoystickTriggerType
    {
        Over,
        OverOrSame,
        Under,
        UnderOrSame,
    }

    public enum TriggerEventType
    { 
        None = -1,
        Joystick_1_Down     = 0,
        Joystick_1_Press    = 1,
        Joystick_1_Up       = 2,
        Joystick_2_Down     = 3,
        Joystick_2_Press    = 4,
        Joystick_2_Up       = 5,
        Button_1_Down       = 6,
        Button_1_Press      = 7,
        Button_1_Up         = 8,
        Button_2_Down       = 9,
        Button_2_Press      = 10,
        Button_2_Up         = 11,
        Button_3_Down       = 12,
        Button_3_Press      = 13,
        Button_3_Up         = 14,
        Button_4_Down       = 15,
        Button_4_Press      = 16,
        Button_4_Up         = 17,
        Hit_Strong          = 18,
        Hit_Medium          = 19,
        Hit_Weak            = 20,
        Max                 = 21,
    }

    public class ActionTriggerData : ScriptableObject
    {
        public bool bUseJoystick;
        public GameInput.JoystickCode JoystickType;
        public JoystickTriggerType JoystickPowerTriggerType;
        [Range(0f, 1f)]
        public float JoystickInputPower = 1f;

        public bool OnAim;

        public TriggerEventType EventType = TriggerEventType.None;

        public virtual bool CheckTrigger(CharacterObject owner)
        {
            // 조이스틱 체크
            if (bUseJoystick)
            {
                float power = owner.CharCtrl.GetInputInfo(JoystickType).fPower;
                
                switch (JoystickPowerTriggerType)
                {
                    case JoystickTriggerType.Over:        if (power <= JoystickInputPower) return false; else break;
                    case JoystickTriggerType.OverOrSame:  if (power <  JoystickInputPower) return false; else break;
                    case JoystickTriggerType.Under:       if (power >= JoystickInputPower) return false; else break;
                    case JoystickTriggerType.UnderOrSame: if (power >  JoystickInputPower) return false; else break;
                }
            }

            // 에임 체크
            if (OnAim && !owner.CharCtrl.OnAim)
                return false;

            return true;

            /* 아래 이벤트의 경우 발생 시 
             * 해당 이벤트가 포함된 트리거 데이터만 CheckTrigger
             
            if (bUseButton)
            if (bUseHitPower)
            */
        }

#if UNITY_EDITOR
        public virtual void OnGUI_Detail(ActionData actionData)
        {
            EditorGUILayout.BeginVertical("Box");
            {
                if (bUseJoystick)
                {
                    var defalutGUIColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Joystick On"))
                        bUseJoystick = false;

                    GUI.backgroundColor = defalutGUIColor;
                }
                else
                {
                    if (GUILayout.Button("Joystick Off"))
                        bUseJoystick = true;
                }

                if (EventType != TriggerEventType.None)
                {
                    var defalutGUIColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("TriggerEvent On"))
                        EventType = TriggerEventType.None;

                    GUI.backgroundColor = defalutGUIColor;
                }
                else
                {
                    if (GUILayout.Button("TriggerEvent Off"))
                        EventType = TriggerEventType.Button_1_Down;
                }

                if (OnAim)
                {
                    var defalutGUIColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Target In Aim On"))
                        OnAim = false;

                    GUI.backgroundColor = defalutGUIColor;
                }
                else
                {
                    if (GUILayout.Button("Target In Aim Off"))
                        OnAim = true;
                }
            }
            EditorGUILayout.EndVertical();
        }

        public virtual void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(200));
                    {
                        if (bUseJoystick)
                        {
                            EditorGUILayout.BeginHorizontal("Box");
                            EditorGUILayout.LabelField($"JOYSTICK");
                            EditorGUILayout.EndHorizontal();
                        }
                        if (OnAim)
                        {
                            EditorGUILayout.BeginHorizontal("Box");
                            EditorGUILayout.LabelField($"TARGET IN AIM");
                            EditorGUILayout.EndHorizontal();
                        }
                        if (EventType != TriggerEventType.None)
                        {
                            EditorGUILayout.BeginHorizontal("Box");
                            EditorGUILayout.LabelField($"TRIGGER EVENT");
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    {
                        if (bUseJoystick)
                        {
                            EditorGUILayout.BeginHorizontal("Box");
                            {
                                JoystickType = (GameInput.JoystickCode)EditorGUILayout.EnumPopup(JoystickType);
                                JoystickPowerTriggerType = (JoystickTriggerType)EditorGUILayout.EnumPopup(JoystickPowerTriggerType);
                                JoystickInputPower = EditorGUILayout.Slider(JoystickInputPower, 0f, 1f);
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        if (OnAim)
                        {
                            EditorGUILayout.BeginHorizontal("Box");
                            {
                                EditorGUILayout.LabelField("    ");
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        if (EventType != TriggerEventType.None)
                        {
                            EditorGUILayout.BeginHorizontal("Box");
                            {
                                EventType = (TriggerEventType)EditorGUILayout.EnumPopup(EventType);
                                if (EventType == TriggerEventType.Max)
                                    EventType = TriggerEventType.None;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
#endif
    }
}