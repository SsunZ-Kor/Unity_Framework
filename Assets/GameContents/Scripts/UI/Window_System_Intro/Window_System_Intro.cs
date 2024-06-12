using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Window_System_Intro : WindowBase
    {
        [SerializeField]
        private ButtonEx _btn_TouchToStart = null;
        [SerializeField]
        private Slider _uiSlider_InitState = null;

        [SerializeField]
        private GameObject _panel_ServerSelect = null;
        [SerializeField]
        private Dropdown _uiDropdown_ServerSelect = null;
        [SerializeField]
        private ButtonEx _btn_ServerSelect = null;

        System.Action _startCallback;

        protected override void Awake()
        {
            base.Awake();

            _btn_TouchToStart.onClick.Subscribe(OnClick_TouchToStart);

            // 데브 서버 선택 세팅
            var _currSelectedDevServerIndex = PlayerPrefs.GetInt("ServerIdx", -1);
            _currSelectedDevServerIndex = Mathf.Clamp(_currSelectedDevServerIndex, 0, NetworkManager.ServerInfos.Length - 1);

            List<Dropdown.OptionData> list_DropDownOption = new List<Dropdown.OptionData>();

            for (int i = 0; i < NetworkManager.ServerInfos.Length; ++i)
            {
                var dropDownOptionData = new Dropdown.OptionData();
                dropDownOptionData.text = NetworkManager.ServerInfos[i].serverName;

                list_DropDownOption.Add(dropDownOptionData);
            }

            _uiDropdown_ServerSelect.AddOptions(list_DropDownOption);
            _uiDropdown_ServerSelect.value = _currSelectedDevServerIndex;

            _btn_ServerSelect.onClick.Subscribe(() =>
            {
                SetActive_SelectServer(false);
                Managers.Net.CrrServerInfo = NetworkManager.ServerInfos[_uiDropdown_ServerSelect.value];
            });
        }

        public override bool CloseSelf()
        {
            if (Managers.Scene.CurrScene == SceneID.Intro)
                return false;

            return base.CloseSelf();
        }

        public void SetTouchToStartCallback(System.Action startCallback)
        {
            _startCallback = startCallback;
        }

        public void SetActive_TouchToStart(bool bActive)
        {
            if (_btn_TouchToStart != null)
                _btn_TouchToStart.gameObject.SetActive(bActive);
        }

        public void SetActive_InitSlider(bool bActive)
        {
            if (_uiSlider_InitState != null)
                _uiSlider_InitState.gameObject.SetActive(bActive);
        }

        public void SetActive_SelectServer(bool bActive)
        {
            if (_panel_ServerSelect != null)
                _panel_ServerSelect.SetActive(bActive);
        }

        public void SetInitSlider(float factor)
        {
            if (_uiSlider_InitState != null)
                _uiSlider_InitState.value = factor;
        }

        private void OnClick_TouchToStart()
        {
            SetActive_TouchToStart(false);
            if (_startCallback != null)
            {
                var call = _startCallback;
                _startCallback = null;
                call.Invoke();
            }
        }
    }
}