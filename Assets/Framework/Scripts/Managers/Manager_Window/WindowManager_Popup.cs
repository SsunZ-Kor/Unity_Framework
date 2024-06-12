using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game
{
    public partial class WindowManager
    {
        /// <summary>
        /// index로 사용중 Value 함부로 변경하지 말것
        /// </summary>
        public enum PopupType
        {
            Game,
            System,
        }

        private class PopupPair
        {
            public SystemPopup comp_popup { get; private set; }
            public Queue<SystemPopup.PopupInfo> queue_PopupInfo { get; private set; }

            public PopupPair(SystemPopup popup)
            {
                comp_popup = popup;
                queue_PopupInfo = new Queue<SystemPopup.PopupInfo>();
            }

            public bool IsValid => comp_popup != null && queue_PopupInfo != null;
            public bool IsOpened => comp_popup != null && comp_popup.gameObject.activeSelf;
        }

        private PopupPair[] _popupPairs;

        public bool IsActivePopup
        {
            get
            {
                if (_popupPairs == null)
                    return false;

                for (int i = 0; i < this._popupPairs.Length; ++i)
                {
                    if (this._popupPairs[i].IsOpened)
                        return true;
                }

                return false;
            }
        }
        
        private void ExcutePopupInfo(PopupType popupType, SystemPopup.PopupInfo popupInfo)
        {
            var popupPair = GetPopupPair(popupType);
            if (popupPair == null || !popupPair.IsValid)
                return;

            if (popupPair.IsOpened)
            {
                popupPair.queue_PopupInfo.Enqueue(popupInfo);
            }
            else
            {
                popupPair.comp_popup.SetPopupInfo(popupInfo);
                RefreshModalState();
                var wnd_Last = GetLastWindow();
                if (wnd_Last != null)
                    wnd_Last.OnEvent_OutLastDepth(false);
            }
        }

        public void EnqueuePopup(
            string title,
            string contents,
            PopupType popupType = PopupType.Game
            )
        {
            var str_Ok = "확인";
            EnqueuePopup(title, contents, str_Ok, null, null, null, null, null, popupType);
        }


        public void EnqueuePopup(
            string title,
            string contents,
            string ok,
            Action okCallback,
            PopupType popupType = PopupType.Game
            )
        {
            EnqueuePopup(title, contents, ok, okCallback, null, null, null, null, popupType);
        }

        public void EnqueuePopup(
            string title,
            string contents,
            Action okCallback,
            PopupType popupType = PopupType.Game
            )
        {
            var str_Ok = "확인";
            EnqueuePopup(title, contents, str_Ok, okCallback, null, null, null, null, popupType);
        }

        public void EnqueuePopup(
            string title,
            string contents,
            string ok,
            Action okCallback,
            string cancel,
            Action cancelCallback,
            PopupType popupType = PopupType.Game
            )
        {
            EnqueuePopup(title, contents, ok, okCallback, null, null, cancel, cancelCallback, popupType);
        }


        public void EnqueuePopup(
            string title,
            string contents,
            Action okCallback,
            Action cancelCallback,
            PopupType popupType = PopupType.Game
            )
        {
            var str_Ok = "확인";
            var str_Cancel = "취소";
            EnqueuePopup(title, contents, str_Ok, okCallback, null, null, str_Cancel, cancelCallback, popupType);
        }

        public void EnqueuePopup(
            string title, 
            string contents, 
            string ok, 
            Action okCallback, 
            string no, 
            Action noCallback, 
            string cancel,
            Action cancelCallback,
            PopupType popupType = PopupType.Game
            )
        {
            SystemPopup.PopupInfo popupInfo = new SystemPopup.PopupInfo()
            {
                Title          = title, 
                Contents       = contents, 
                Ok             = ok, 
                OkCallback     = okCallback, 
                bOkClose       = true, 
                No             = no, 
                NoCallback     = noCallback, 
                bNoClose       = true, 
                Cancel         = cancel,
                CancelCallback = cancelCallback,
                bCancelClose   = true
            };

            ExcutePopupInfo(popupType, popupInfo);
        }

        public void EnqueuePopup(
            string title,
            string contents,
            string ok,
            Action okCallback,
            bool bOkClose,
            string no,
            Action noCallback,
            bool bNoClose,
            string cancel,
            Action cancelCallback,
            bool bCancelClose,
            PopupType popupType = PopupType.Game
            )
        {
            SystemPopup.PopupInfo popupInfo = new SystemPopup.PopupInfo()
            {
                Title = title,
                Contents = contents,
                Ok = ok,
                OkCallback = okCallback,
                bOkClose = bOkClose,
                No = no,
                NoCallback = noCallback,
                bNoClose = bNoClose,
                Cancel = cancel,
                CancelCallback = cancelCallback,
                bCancelClose = bCancelClose,
            };

            ExcutePopupInfo(popupType, popupInfo);
        }

        public void EnqueuePopup(
            string title,
            string contents,
            Action okCallback,
            Action noCallback,
            Action cancelCallback,
            PopupType popupType = PopupType.Game
            )
        {
            var str_Ok = "예";
            var str_No = "아니오";
            var str_Cancel = "취소";
            EnqueuePopup(title, contents, str_Ok, okCallback, str_No, noCallback, str_Cancel, cancelCallback, popupType);
        }

        public void OnPopupClose(PopupType popupType)
        {
            var popupPair = GetPopupPair(popupType);
            if (popupPair == null || !popupPair.IsValid)
                return;

            if (popupPair.queue_PopupInfo.Count <= 0)
            {
                RefreshModalState();

                var wnd_Last = GetLastWindow();
                if (wnd_Last != null)
                    wnd_Last.OnEvent_OnLastDepth();

                return;
            }

            var popupInfo = popupPair.queue_PopupInfo.Dequeue();
            popupPair.comp_popup.SetPopupInfo(popupInfo);
        }

        private PopupPair GetPopupPair(PopupType popupType)
        {
            if (_popupPairs == null)
                return null;

            var idx_PopupType = (int)popupType;
            if (!_popupPairs.CheckIndex(idx_PopupType))
                return null;

            return _popupPairs[idx_PopupType];
        }
    }
}
