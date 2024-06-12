using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Game
{
    public class SystemPopup : MonoBehaviour
    {
        [System.Serializable]
        public class PopupButtonSet
        {
            [SerializeField]
            private GameObject _go_Root = null;
            [SerializeField]
            private ButtonEx _btn_Ok = null;
            [SerializeField]
            private ButtonEx _btn_No = null;
            [SerializeField]
            private ButtonEx _btn_Cancel = null;

            public void SetActive(bool bActive)
            {
                if (_go_Root != null)
                    _go_Root.SetActive(bActive);
            }

            public void SetCallback(UnityAction okCallback, UnityAction noCallback, UnityAction cancelCallback)
            {
                if (_btn_Ok != null) _btn_Ok.onClick.Subscribe(okCallback);
                if (_btn_No != null) _btn_No.onClick.Subscribe(noCallback);
                if (_btn_Cancel != null) _btn_Cancel.onClick.Subscribe(cancelCallback);
            }

            public void SetText(string okText, string noText, string cancelText)
            {
                if (_btn_Ok != null) _btn_Ok.SetButtonText(okText);
                if (_btn_No != null) _btn_No.SetButtonText(noText);
                if (_btn_Cancel != null) _btn_Cancel.SetButtonText(cancelText);
            }

            public bool AutoCancel()
            {
                if (_btn_Cancel != null && _btn_Cancel.gameObject.activeSelf)
                {
                    _btn_Cancel.onClick.Invoke();
                    return true;
                }

                if (_btn_No != null && _btn_No.gameObject.activeSelf)
                {
                    _btn_No.onClick.Invoke();
                    return true;
                }

                if (_btn_Ok != null && _btn_Ok.gameObject.activeSelf)
                {
                    _btn_Ok.onClick.Invoke();
                    return true;
                }

                return false;
            }
        }


        public struct PopupInfo
        {
            public string Title;
            public string Contents;
            public string Ok;
            public Action OkCallback;
            public bool bOkClose;
            public string No;
            public Action NoCallback;
            public bool bNoClose;
            public string Cancel;
            public Action CancelCallback;
            public bool bCancelClose;
        }

        [SerializeField]
        private Text _uiTxt_Title = null;
        [SerializeField]
        private Text _uiTxt_Contents = null;

        [Header("Buttons")]
        [SerializeField]
        private PopupButtonSet[] _buttonSet = null;

        private bool bAutoCloseOk = true;
        private bool bAutoCloseNo = true;
        private bool bAutoCloseCancel = true;
        private Action _okCallback = null;
        private Action _noCallback = null;
        private Action _cancelCallback = null;

        private WindowManager.PopupType _popupType;

        private PopupButtonSet _currButtonSet = null;


        private void Awake()
        {
            _buttonSet.ForEach((x) => x.SetActive(false));
            _buttonSet.ForEach((x) => x.SetCallback(OnClick_OK, OnClick_NO, OnClick_Cancel));
        }

        public void Init(WindowManager.PopupType popupType)
        {
            _popupType = popupType;
        }

        public void SetPopupInfo(PopupInfo popupInfo)
        {
            _uiTxt_Title.text = string.IsNullOrWhiteSpace(popupInfo.Title) ? "알림" : popupInfo.Title;
            _uiTxt_Contents.text = popupInfo.Contents;


            _okCallback = popupInfo.OkCallback;
            _noCallback = popupInfo.NoCallback;
            _cancelCallback = popupInfo.CancelCallback;

            int buttonCount = 0;
            if (!string.IsNullOrWhiteSpace(popupInfo.Ok))
                ++buttonCount;
            if (!string.IsNullOrWhiteSpace(popupInfo.No))
                ++buttonCount;
            if (!string.IsNullOrWhiteSpace(popupInfo.Cancel))
                ++buttonCount;

            var prevButtonSet = _currButtonSet;
            _currButtonSet = _buttonSet[buttonCount - 1];
            if (_currButtonSet != prevButtonSet)
            {
                if (prevButtonSet != null)
                    prevButtonSet.SetActive(false);

                _currButtonSet.SetActive(true);
            }

            _currButtonSet.SetText(popupInfo.Ok, popupInfo.No, popupInfo.Cancel);

            this.gameObject.SetActive(true);

            bAutoCloseOk = popupInfo.bOkClose;
            bAutoCloseNo = popupInfo.bNoClose;
            bAutoCloseCancel = popupInfo.bCancelClose;
        }

        public void OnClick_OK()
        {
            var callback = _okCallback;

            if (bAutoCloseOk)
            {
                _okCallback = null;
                _noCallback = null;
                _cancelCallback = null;

                this.gameObject.SetActive(false);
                Managers.UI.OnPopupClose(this._popupType);
            }

            callback?.Invoke();
        }

        public void OnClick_NO()
        {
            var callback = _noCallback;

            if (bAutoCloseNo)
            {
                _okCallback = null;
                _noCallback = null;
                _cancelCallback = null;

                this.gameObject.SetActive(false);
                Managers.UI.OnPopupClose(this._popupType);
            }

            callback?.Invoke();
        }

        public void OnClick_Cancel()
        {
            var callback = _cancelCallback;

            if (bAutoCloseCancel)
            {
                _okCallback = null;
                _noCallback = null;
                _cancelCallback = null;

                this.gameObject.SetActive(false);
                Managers.UI.OnPopupClose(this._popupType);
            }

            callback?.Invoke();
        }

        public bool AutoCancel()
        {
            if (!this.gameObject.activeSelf || _popupType == WindowManager.PopupType.System)
                return false;

            return _currButtonSet.AutoCancel();
        }
    }
}
