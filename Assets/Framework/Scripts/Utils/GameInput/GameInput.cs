using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class GameInput
    {
        #region Define
        public enum KeyCode
        {
            Joystick_1,
            Joystick_2,
            Button_1,
            Button_2,
            Button_3,
            Button_4,
        }

        public enum JoystickCode
        {
            Joystick_1,
            Joystick_2,
        }

        public enum KeyState
        { 
            Down,
            Press,
            Up,
        }


        public delegate void DelAsixEvent(JoystickCode jCode, Vector2 vInput, Vector2 vDir, float fPower); 
        public delegate void DelButtonEvent(KeyCode kCode, KeyState kState);

        public static DelAsixEvent OnAsixEvent = null;
        public static DelButtonEvent OnButtonEvent = null;

        /// <summary>
        /// 생성된 패드는 항상 GameInput.InputInst 에 등록하여 사용한다.
        /// </summary>
        public interface InputInstance
        {
            Vector2 GetAsix(JoystickCode jCode);
            Vector2 GetDir(JoystickCode jCode);
            float GetPower(JoystickCode jCode);
            bool GetKey(KeyCode kCode);
            bool GetKeyDown(KeyCode kCode);
            bool GetKeyUp(KeyCode kCode);
        }

        #endregion

        // 게임 패드는 2개이상 나올 수 없다고 판단
        public static InputInstance InputInst;

        public static Vector2 GetStickAsix(JoystickCode jCode)
        {
            if (InputInst == null)
                return Vector2.zero;

            return InputInst.GetAsix(jCode);
        }

        public static Vector2 GetStickDir(JoystickCode jCode)
        {
            if (InputInst == null)
                return Vector2.zero;

            return InputInst.GetDir(jCode);
        }

        public static float GetStickPower(JoystickCode jCode)
        {
            if (InputInst == null)
                return 0f;

            return InputInst.GetPower(jCode);
        }

        public static bool GetKey(KeyCode kCode)
        {
            if (InputInst == null)
                return false;

            return InputInst.GetKey(kCode);
        }

        public static bool GetKeyDown(KeyCode kCode)
        {
            if (InputInst == null)
                return false;

            return InputInst.GetKeyDown(kCode);

        }

        public static bool GetKeyUp(KeyCode kCode)
        {
            if (InputInst == null)
                return false;

            return InputInst.GetKeyUp(kCode);
        }
    }
}