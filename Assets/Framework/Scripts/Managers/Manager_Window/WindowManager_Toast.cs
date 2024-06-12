using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Game
{
    public partial class WindowManager
    {
        private ToastPopup _comp_Toast;

        public void EnqueueToast(string toastMessage)
        {
            if (_comp_Toast == null)
            {
                // Toast 메세지 설정
                var prf_Toast = Resources.Load<GameObject>("System/ToastPopup");
                if (prf_Toast != null)
                {
                    var _go_Toast = GameObject.Instantiate(prf_Toast, _rttr_holderForToast);
                    _comp_Toast = _go_Toast.GetComponent(typeof(ToastPopup)) as ToastPopup;
                    _go_Toast.transform.Reset();
                    _go_Toast.gameObject.SetActive(false);
                }
            }

            _comp_Toast.Enqueue(toastMessage);
        }
    }
}