using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Game
{

    public class GlobalNavigationBar : MonoBehaviour
    {
        [System.Flags]
        public enum ItemType
        {
            None = 0,
            Back = 1,
        }

        [SerializeField]
        private ButtonEx _btn_Back= null;

        private void Awake()
        {
            _btn_Back.onClick.Subscribe(() => Managers.UI.CloseLast(false));
        }

        public void SetItem(ItemType itemFlags)
        {
            _btn_Back.gameObject.SetActive((itemFlags & ItemType.Back) != 0);
        }
    }
}