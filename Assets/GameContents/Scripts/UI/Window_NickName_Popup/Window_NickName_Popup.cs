using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Window_NickName_Popup : WindowBase
    {
        private const int nMaxLength_RoomName = 16;

        [Header("Child Components")]
        [SerializeField]
        private InputField _uiTxt_RoomName = null;
        [SerializeField]
        private Text _uiTxt_Count = null;
        [SerializeField]
        private ButtonEx _btn_Confirm = null;

        protected override void Awake()
        {
            base.Awake();

            _btn_Confirm.onClick.Subscribe(OnClick_Done);
            OnEvent_InputChanged(string.Empty);
            if (_uiTxt_Count != null)
                _uiTxt_Count.text = $"(0/{nMaxLength_RoomName})";

            _uiTxt_RoomName.characterLimit = 0;
        }

        public override bool CloseSelf()
        {
            if (string.IsNullOrWhiteSpace(UserData.AccountInfo.NickName))
            {
                Managers.UI.EnqueuePopup("알림", "닉네임을 입력해 주세요");
                return false;
            }

            return base.CloseSelf();
        }

        public void OnEvent_InputChanged(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _btn_Confirm.ButtonState = ButtonEx.ButtonStateType.Disable;
                if (_uiTxt_Count != null)
                    _uiTxt_Count.text = $"(0/{nMaxLength_RoomName})";
            }
            else
            {
                text.Trim();

                _btn_Confirm.ButtonState = ButtonEx.ButtonStateType.Normal;
                if (_uiTxt_Count != null)
                    _uiTxt_Count.text = $"({text.Length}/{nMaxLength_RoomName})";
            }

            if (text.Length > nMaxLength_RoomName)
            {
                _uiTxt_RoomName.text = text.Remove(nMaxLength_RoomName);
                StartCoroutine(CoMoveToEnd());
            }
        }

        public void OnClick_Done()
        {
            if (string.IsNullOrWhiteSpace(_uiTxt_RoomName.text))
            {
                Managers.UI.EnqueuePopup("알림", "닉네임을 입력해 주세요");
                return;
            }

            NetProcess.Request_NickName(_uiTxt_RoomName.text, () => this.CloseSelf());
        }


        IEnumerator CoMoveToEnd()
        {
            yield return null;

            _uiTxt_RoomName.MoveTextEnd(false);
        }
    }
}