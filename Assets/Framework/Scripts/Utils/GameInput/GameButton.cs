using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Game
{
    public class GameButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField]
        private bool bExcutePointerExit = true;

        private GameInput.KeyCode _myCode;

        private int _touchId = -1;

        public bool IsDown { get; private set; }
        public bool IsPress { get; private set; }
        public bool IsUp { get; private set; }

        public void Init(GameInput.KeyCode myCode)
        {
            _myCode = myCode;
        }

        private void OnEnable()
        {
            this._touchId = -1;
            this.IsDown = false;
            this.IsPress = false;
            this.IsUp = false;
        }

        private void Update()
        {
            if (IsPress)
                OnButtonEvent(GameInput.KeyState.Press);
        }

        private void OnDisable()
        {
            this._touchId = -1;
            this.IsDown = false;
            this.IsPress = false;
            this.IsUp = false;

            this.StopAllCoroutines();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (this._touchId != -1)
                return;

            // 터치 아이디 저장
#if UNITY_EDITOR
            this._touchId = 1;
#else
        this._touchId = eventData.pointerId;
#endif
            IsPress = true;
            IsDown = true;
            IsUp = false;

            StartCoroutine(Cor_ReleaseDown());

            OnButtonEvent(GameInput.KeyState.Down);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!bExcutePointerExit)
                return;

            if (this._touchId == -1)
                return;

            // 터치 아이디 해제
            this._touchId = -1;

            IsPress = false;
            IsDown = false;
            IsUp = true;
            StartCoroutine(Cor_ReleaseUp());

            OnButtonEvent(GameInput.KeyState.Up);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (this._touchId == -1)
                return;

            // 터치 아이디 해제
            this._touchId = -1;

            IsPress = false;
            IsDown = false;
            IsUp = true;
            StartCoroutine(Cor_ReleaseUp());

            OnButtonEvent(GameInput.KeyState.Up);
        }

        IEnumerator Cor_ReleaseDown()
        {
            yield return new WaitForEndOfFrame();

            IsDown = false;
        }

        IEnumerator Cor_ReleaseUp()
        {
            yield return new WaitForEndOfFrame();

            IsUp = false;
        }

        private void OnButtonEvent(GameInput.KeyState state)
        {
            if (GameInput.OnButtonEvent != null)
                GameInput.OnButtonEvent.Invoke(_myCode, state);
        }
    }
}