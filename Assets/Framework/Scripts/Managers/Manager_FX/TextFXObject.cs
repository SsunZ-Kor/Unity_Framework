using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class TextFXObject : FXObject
    {
        [Header("Child Components")]
        [SerializeField]
        protected TextMesh _textMesh;
        [SerializeField]
        protected UnityEngine.UI.Text _textUI;

        public void SetText(string text)
        {
            if (_textMesh != null)
                _textMesh.text = text;

            if (_textUI != null)
                _textUI.text = text;
        }
    }
}