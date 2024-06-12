using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class ToastPopup : MonoBehaviour
    {
        [SerializeField]
        private Animation _anim = null;
        [SerializeField]
        private Text _uiTxt_Msg = null;

        private Queue<string> _queue_Msg = new Queue<string>();

        public void Enqueue(string msg)
        {
            if (string.IsNullOrEmpty(msg))
                return;

            if (_uiTxt_Msg == null)
                return;

            if (_uiTxt_Msg.text != null && msg.CompareTo(_uiTxt_Msg.text) == 0)
                return;

            if (!this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(true);
                _uiTxt_Msg.text = msg;
                _anim.Play();
            }
            else
            {
                _queue_Msg.Enqueue(msg);
            }

        }

        private void Update()
        {
            // 현재 애니메이션이 실행 중이거나, 로딩 중이라면 리턴
            if (this._anim.isPlaying || (Managers.Scene != null && Managers.Scene.loadingState != SceneManager.LoadingState.None))
                return;

            // 메시지가 남아있지 않다면 끈다.
            if (_queue_Msg.Count <= 0)
            {
                this.gameObject.SetActive(false);
                _uiTxt_Msg.text = string.Empty;
                return;
            }

            _uiTxt_Msg.text = _queue_Msg.Dequeue();
            _anim.Play();
        }

        private void Clear()
        {
            _queue_Msg.Clear();
        }
    }
}

