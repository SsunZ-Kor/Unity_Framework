using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Window_Settings : WindowBase
    {
        [SerializeField]
        public TabEx _uiTab_CamMode;
        [SerializeField]
        public Slider _uiSlider_Sencitive;

        protected override void Awake()
        {
            base.Awake();

            _uiTab_CamMode.SetCallback(OnTab_SelectCamMode);

            if (_uiSlider_Sencitive.onValueChanged == null)
                _uiSlider_Sencitive.onValueChanged = new Slider.SliderEvent();

            _uiSlider_Sencitive.onValueChanged.AddListener(OnSlider_ChangeSencitive);
        }

        public override void OnEvent_OnLastDepth()
        {
            base.OnEvent_OnLastDepth();

            var currSelectedCamMode = (Window_BattleMain.CamControlMode)PlayerPrefs.GetInt("CamControlMode", (int)Window_BattleMain.CamControlMode.Swipe);

            var idx = 0;
            switch(currSelectedCamMode)
            {
                case Window_BattleMain.CamControlMode.Swipe:
                    idx = 0;
                    break;
                case Window_BattleMain.CamControlMode.Joystick:
                    idx = 1;
                    break;
            }

            _uiTab_CamMode.ChangeIndex(idx);

            _uiSlider_Sencitive.value = PlayerPrefs.GetFloat("CamControlSencitive", 100f);
        }

        public override void OnEvent_OutLastDepth(bool isClose)
        {
            base.OnEvent_OutLastDepth(isClose);

            PlayerPrefs.Save();
        }

        private void OnTab_SelectCamMode(int idx)
        {
            switch (idx)
            {
                case 0:
                    PlayerPrefs.SetInt("CamControlMode", (int)Window_BattleMain.CamControlMode.Swipe);
                    break;
                case 1:
                    PlayerPrefs.SetInt("CamControlMode", (int)Window_BattleMain.CamControlMode.Joystick);
                    break;
            }
        }

        private void OnSlider_ChangeSencitive(float value)
        {
            PlayerPrefs.SetFloat("CamControlSencitive", value);
        }
    }
}
