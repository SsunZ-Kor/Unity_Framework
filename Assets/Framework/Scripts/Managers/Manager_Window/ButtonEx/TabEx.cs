using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class TabEx : MonoBehaviour
    {
        [SerializeField]
        public ButtonEx[] ButtonGroup = null;

        protected System.Action<int> OnClickCallback = null;
        protected System.Action<int> OnClickFailedCallback = null;

        public int Index { get; protected set; } = -1;

        public int Count
        {
            get
            {
                if(ButtonGroup == null)
                    return 0;

                return ButtonGroup.Length;
            }
        }
        
        public void Awake()
        {
            for(int i = 0; i < ButtonGroup.Length; ++i)
            {
                var button = ButtonGroup[i];
                if (button == null)
                    continue;

                var index = i;
                button.onClick.Subscribe(() => { ChangeIndex(index); });
            }
        }

        public virtual void ChangeIndex(int index)
        {
             // 같은 인덱스면 호출스택 종료
             if (Index == index)
                 return;

             // 인덱스 유효성 검증
             if (index < 0
                 || index >= ButtonGroup.Length)
                 return;

             // 활성화 될 버튼 유효성 검증
             var currButton = ButtonGroup[index];
             if (currButton == null 
                 || currButton.ButtonState == ButtonEx.ButtonStateType.Disable)
            {
                OnClickFailedCallback?.Invoke(index);
                return;
            }

            // 기존 활성화 버튼 종료
            if (Index != -1)
             {
                 var prevButton = ButtonGroup[Index];
                if (prevButton != null)
                     prevButton.ButtonState = ButtonEx.ButtonStateType.Normal;
             }

            Index = index;
            currButton.ButtonState = ButtonEx.ButtonStateType.Hightlight;
            
            OnClickCallback?.Invoke(index);
        }

        public void ChangeButtonState(int idx_Button, ButtonEx.ButtonStateType stateType)
        {
            if (ButtonGroup == null)
                return;

            if (!ButtonGroup.CheckIndex(idx_Button))
                return;

            var btn = ButtonGroup[idx_Button];
            if (btn == null)
                return;

            btn.ButtonState = stateType;
        }

        public ButtonEx.ButtonStateType GetButtonState(int idx_Button)
        {
            if (ButtonGroup == null)
                return ButtonEx.ButtonStateType.Disable;

            if (!ButtonGroup.CheckIndex(idx_Button))
                return ButtonEx.ButtonStateType.Disable;

            var btn = ButtonGroup[idx_Button];
            if (btn == null)
                return ButtonEx.ButtonStateType.Disable;

            return btn.ButtonState;
        }

        public void SetCallback(System.Action<int> callback, System.Action<int> failedCallback = null)
        {
            OnClickCallback = callback;
            OnClickFailedCallback = failedCallback;
        }

        public void SetButtonText(int nIndex, string strText)
        {
            if(nIndex < 0 || nIndex >= ButtonGroup.Length || ButtonGroup[nIndex] == null)
                return;

            ButtonGroup[nIndex].SetButtonText(strText);
        }

        public void ClearIndex()
        {
            // 기존 활성화 버튼 종료
            if(Index != -1)
            {
                var prevButton = ButtonGroup[Index];
                if(prevButton != null)
                    prevButton.ButtonState = ButtonEx.ButtonStateType.Normal;
            }

            this.Index = -1;
        }
        public ButtonEx GetItemComponent(int idx){
            return this.ButtonGroup[idx];
            
        }
        public GameObject GetItemObject(int idx){
            return this.ButtonGroup[idx].gameObject;
            
        }
    }
}
