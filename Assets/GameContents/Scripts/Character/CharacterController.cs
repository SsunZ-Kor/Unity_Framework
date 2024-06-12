using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Game.Character
{
    public partial class CharacterController
    {
        public enum ControllerType
        {
            Manual,
            AI,
            Net,
        }

        public class JoystickInputInfo
        {
            public Vector2 vInput { get; private set; }
            public Vector2 vDir { get; private set; }
            public float fPower { get; private set; }

            public void SetInput(Vector2 vInput, Vector2 vDir, float fPower)
            {
                this.vInput = vInput;
                this.vDir = vDir;
                this.fPower = fPower;
            }
        }


        public CharacterObject Owner { get; private set; }
        public ControllerType CtrlType = ControllerType.Manual;

        public JoystickInputInfo[] _jInputInfos = new JoystickInputInfo[]
        {
            new JoystickInputInfo(),
            new JoystickInputInfo(),
        };

        public bool OnAim;

        public CharacterController(CharacterObject owner)
        {
            Owner = owner;
            OnAwake_Manual();
        }

        public void OnUpdate()
        {
            switch (CtrlType)
            {
                case ControllerType.Manual:
                    OnUpdate_Manual();
                    break;
                case ControllerType.AI:
                    OnUpdate_AI();
                    break;
                case ControllerType.Net:
                    OnUpdate_Net();
                    break;
            }
        }

        public void OnDestroy()
        {
            OnDestroy_Manual();
        }

        public JoystickInputInfo GetInputInfo(GameInput.JoystickCode jCode)
        {
            return _jInputInfos[(int)jCode];
        }
    }
}