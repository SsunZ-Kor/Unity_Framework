using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class TextImgFxObject : FXObject
    {
        [Header("Text And Image")]
        [SerializeField]
        protected Text _uiTxt;
        [SerializeField]
        protected Image _uiImg;

        public void SetText(string text, Sprite sprite)
        {
            if (_uiTxt != null)
                _uiTxt.text = text;

            if (_uiImg != null)
                _uiImg.sprite = sprite;
        }
    }
}
