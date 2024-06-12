using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game
{
    public class ButtonEx : MonoBehaviour/*Graphic*/, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
    {
        private static Dictionary<int, ButtonEx> _dic_CurrPressedButtonEx = new Dictionary<int, ButtonEx>();
        private static int MaxItemStateTypeCount = System.Enum.GetValues(typeof(ItemStateType)).Length;

        [System.Serializable]
        public class UnityEventVec2 : UnityEvent<Vector2> { }        

        public enum ButtonStateType
        {
            Normal,
            Hightlight,
            Disable,
        }

        public enum ItemStateType
        {
            Normal,
            Hover,
            Pressed,
            Exited,
        }

        [System.Serializable]
        public class ItemStateInfo
        {
            public bool Enable = true;
            public Color Color = Color.white;
            public Vector3 Scale = Vector3.one;
        }

        [System.Serializable]
        public class GameObjectStateInfo
        {
            public bool Enable = true;
            public Vector3 Scale = Vector3.one;
        }

        [System.Serializable]
        public class ItemInfo
        {
            public Graphic Target = null;
            public ItemStateInfo[] stateInfo = null;
        }

        [System.Serializable]
        public class GameObjectInfo
        {
            public GameObject Target = null;
            public GameObjectStateInfo[] enableInfo = null;
        }

        [SerializeField]
        private Animation _anim;
        [SerializeField]
        private List<Text> _uiTxt_Buttons = null;
        
        [SerializeField]
        private float holdTime = 2f;        // 버튼 Hold 이벤트가 발동하기 까지의 시간
        [SerializeField]
        private float holdEffectTime = 1f;  // 버튼 Hold Fx가 발동하기 까지의 시간
        [SerializeField]
        private GameObject prf_Effect = null;


        [HideInInspector]
        public List<GameObjectInfo> goInfos;
        [HideInInspector]
        public List<ItemInfo> itemInfos;
        [HideInInspector]
        public AudioClip[] stateSoundClip = null;
        [HideInInspector]
        public AnimationClip[] stateAnimClip = null;

        [HideInInspector]
        [SerializeField]
        private int _eventGroupId = 0;

        [HideInInspector]
        [SerializeField]
        public bool bUseOnClick = true;
        [HideInInspector]
        [SerializeField]
        public Button.ButtonClickedEvent onClick = new Button.ButtonClickedEvent();
        [SerializeField]
        private AudioClip _audioClip_OnClick = null;

        [HideInInspector]
        [SerializeField]
        public bool bUseOnHold = false;
        /// <summary>
        /// 누르고 있을 때 한번만 발생하는 이벤트
        /// </summary>
        [HideInInspector]
        [SerializeField]
        public UnityEvent onHold = new UnityEvent();
        [SerializeField]
        private AudioClip _audioClip_OnHold = null;

        [HideInInspector]
        [SerializeField]
        public bool bUseOnPress = false;
        /// <summary>
        /// 누르기 시작할 때 발생하는 이벤트
        /// </summary>
        [HideInInspector]
        [SerializeField]
        public UnityEventVec2 onPressStart = new UnityEventVec2();
        ///// <summary>
        ///// 누르고 있을 때 지속적으로 발생하는 이벤트
        ///// </summary>
        //[HideInInspector]
        //[SerializeField]
        //public UnityEvent onPressed = new UnityEvent();
        /// <summary>
        /// 누르고 있을 때 지속적으로 발생하는 이벤트
        /// </summary>
        [HideInInspector]
        [SerializeField]
        public UnityEventVec2 onPressed = new UnityEventVec2();
        /// <summary>
        /// 누르기 종료 이벤트
        /// </summary>
        [HideInInspector]
        [SerializeField]
        public UnityEvent onPressEnd = new UnityEvent();

        //[SerializeField]
        //private AudioClip _audioClip_OnPress;

        private int _touchID = -2;
        private bool _isEntered;
        private float _currHoldTime;
        private ScrollRect _uiScroll = null;
        private Vector2 _touchDownPosition = Vector3.zero;

        private IDragHandler _parent_DragHandler = null;
        private IBeginDragHandler _parent_BeginDragHandler = null;
        private IEndDragHandler _parent_EndDragHandler = null;

        public List<Text> ButtonTexts => this._uiTxt_Buttons;

        private ButtonStateType _buttonState = ButtonStateType.Normal;
        public ButtonStateType ButtonState
        {
            get
            {
                return _buttonState;
            }
            set
            {
                if (value == _buttonState)
                    return;

                _buttonState = value;
                ChangeState(ItemStateType.Normal, true);
            }
        }

        private FXObject _holdFxObject = null;

        protected void Awake()
        {
            // 애니메이션 컴포넌트가 없다면
            if (_anim == null && stateAnimClip != null)
            {
                for (int i = 0; i < stateAnimClip.Length; ++i)
                {
                    if (stateAnimClip[i] == null)
                        continue;

                    _anim = this.gameObject.GetOrAddComponent(typeof(Animation)) as Animation;
                    break;
                }
            }

            // 애니메이션 컴포넌트가 있다면 애니메이션 추가
            if (_anim != null && stateAnimClip != null)
            {
                 for (int i = 0; i < stateAnimClip.Length; ++i)
                 {
                    if (stateAnimClip[i] == null)
                        continue;

                    if (_anim.GetClip(stateAnimClip[i].name) == null)
                       _anim.AddClip(stateAnimClip[i], stateAnimClip[i].name);
                 }
            }

            if (holdEffectTime > holdTime)
                holdEffectTime = holdTime;

            if (prf_Effect != null)
                Managers.FX.RegistFX(prf_Effect);
        }

        private void Start()
        {
            _uiScroll = this.GetComponentInParent(typeof(ScrollRect)) as ScrollRect;
        }

        protected void OnEnable()
        {
            ChangeState(ItemStateType.Normal, false);

            if (_holdFxObject != null)
            {
                _holdFxObject.ReturnToPoolForce();
                _holdFxObject = null;
            }
        }

        protected void Update()
        {
            Update_Hold();
            Update_Pressed();
        }

        protected void OnDisable()
        {
            _touchID = -2;
            _isEntered = false;
            _currHoldTime = 0f;

            if (_eventGroupId >= 0)
            {
                var btnGroup = _dic_CurrPressedButtonEx.GetOrNull(_eventGroupId);
                if (btnGroup != null && btnGroup == this)
                    _dic_CurrPressedButtonEx.Remove(_eventGroupId);
            }
        }

        protected void OnDistroy()
        {
            if (prf_Effect != null)
                Managers.FX.RemoveFX(prf_Effect.name);
        }


        protected void Update_Hold()
        {
            if (_touchID == -2 || !_isEntered || onHold == null || !bUseOnHold)
                return;

            _currHoldTime += Time.deltaTime;

            if (_currHoldTime >= holdEffectTime)
            {
                Vector2 canvasPos = Vector2.zero;
                if (_touchID == -1)
                {
                    // 마우스일 경우
                    canvasPos = Input.mousePosition;
                }
                else
                {
                    Touch touch = Input.GetTouch(_touchID);
                    canvasPos = touch.position;
                }

                canvasPos = Managers.UI.ConvertScreenToCanvasPoint(canvasPos);

                if (_holdFxObject != null)
                {
                    _holdFxObject.transform.localPosition = canvasPos;
                }
                else if (prf_Effect != null)
                {
                    _holdFxObject = Managers.FX.PlayFX(
                        prf_Effect.name,
                        canvasPos,
                        Quaternion.identity,
                        Vector3.one,
                        Managers.UI._rttr_CanvasRoot,
                        1f,
                        _currHoldTime - holdEffectTime);
                }
            }

            if (_currHoldTime < holdTime)
                return;

            if (onHold != null)
            {
                onHold?.Invoke();
            }
            _holdFxObject = null;

            if (_eventGroupId >= 0)
            {
                var btnGroup = _dic_CurrPressedButtonEx.GetOrNull(_eventGroupId);
                if (btnGroup != null && btnGroup == this)
                    _dic_CurrPressedButtonEx.Remove(_eventGroupId);
            }

            ItemStateType normalItemState = ItemStateType.Normal;
            if (_isEntered)
                normalItemState = ItemStateType.Hover;

            ChangeState(normalItemState, true);

            _touchID = -2;
            _isEntered = false;
            _currHoldTime = 0f;
        }

        protected void Update_Pressed()
        {
            if(_touchID == -2 || !_isEntered || !bUseOnPress || onPressed == null)
                return;

            if(onPressed != null)
            {
                //if(_audioClip_OnPress != null)
                //{
                //    Managers.SFX.PlaySFX(
                //        this.transform.position,
                //        this.transform.rotation,
                //        Vector3.one,
                //        null,
                //        _audioClip_OnPress,
                //        SFXType._2D,
                //        false);
                //}

                Vector2 vPos = Vector2.zero;
                if(_touchID == -1)
                {
                    // 마우스일 경우
                    vPos = Input.mousePosition;
                }
                else
                {
                    try
                    {
                        Touch touch = Input.GetTouch(_touchID);
                        vPos = touch.position;
                    }
                    catch
                    {
                        _touchID = -2;
                        _isEntered = false;
                        _currHoldTime = 0f;
                        _touchDownPosition = Vector2.zero;

                        ChangeState(ItemStateType.Normal, false);
                        return;
                    }

                }

                onPressed.Invoke(vPos);
            }
        }

        public void SetButtonText(string text)
        {
            if (_uiTxt_Buttons == null)
                return;

            for (int i = 0; i < _uiTxt_Buttons.Count; ++i)
            {
                if (_uiTxt_Buttons[i] != null)
                    _uiTxt_Buttons[i].text = text;
            }
        }

        public void SetButtonColor(string strHexCode)
        {
            if(_uiTxt_Buttons == null)
                return;

            for(int i = 0 ; i < _uiTxt_Buttons.Count ; ++i)
            {
                if(_uiTxt_Buttons[i] != null)
                    _uiTxt_Buttons[i].color = strHexCode.HexToColor();
            }
        }

        public void SetDragParent(IDragHandler _drag, IBeginDragHandler _beginDrag, IEndDragHandler _endDrag)
        {
            _parent_DragHandler = _drag;
            _parent_BeginDragHandler = _beginDrag;
            _parent_EndDragHandler = _endDrag;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            //Debug.Log("down");
            if (_uiScroll != null)
            {

                //Debug.Log($"ScrollVelocity : {_uiScroll.velocity}");
                if (_uiScroll.velocity.magnitude > 40)
                {
                    _uiScroll.StopMovement();
                    return;
                }

                _uiScroll.StopMovement();
            }

            if (_eventGroupId >= 0)
            {
                var btnGroup = _dic_CurrPressedButtonEx.GetOrNull(_eventGroupId);
                if (btnGroup != null && btnGroup != this)
                    return;
            }

            if (_touchID != -2)
                return;

//#if DevClient
//            Debug.Log($"<color=yellow>OnPointer Down</color> {this.gameObject.name}");
//#endif
            _touchID = eventData.pointerId;
            _isEntered = true;
            _currHoldTime = 0f;
            _touchDownPosition = eventData.position;

            if (_eventGroupId >= 0)
                _dic_CurrPressedButtonEx.AddOrRefresh(_eventGroupId, this);

            ChangeState(ItemStateType.Pressed, true);

            if (bUseOnPress && onPressStart != null)
            {
                //if (_audioClip_OnPress != null)
                //{
                //    Managers.SFX.PlaySFX(
                //        this.transform.position,
                //        this.transform.rotation,
                //        Vector3.one,
                //        null,
                //        _audioClip_OnPress,
                //        SFXType._2D,
                //        false);
                //}

                Vector2 vPos = Vector2.zero;
                if(_touchID == -1)
                {
                    // 마우스일 경우
                    vPos = Input.mousePosition;
                }
                else
                {
                    vPos = eventData.position;
                }

                onPressStart.Invoke(vPos);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //Debug.Log("up");
            if (_touchID == -2)
                return;

            if (_touchID != eventData.pointerId)
                return;

            if (_eventGroupId >= 0)
            {
                var btnGroup = _dic_CurrPressedButtonEx.GetOrNull(_eventGroupId);
                if (btnGroup == null || btnGroup != this)
                    return;

                _dic_CurrPressedButtonEx.Remove(_eventGroupId);
            }

//#if DevClient
//            Debug.Log($"<color=yellow>OnPointer Up</color> {this.gameObject.name}");
//#endif
            ItemStateType normalItemState = ItemStateType.Normal;
            if (_isEntered && (onHold == null || !bUseOnHold || _currHoldTime < holdEffectTime))
            {
                if (eventData.pointerDrag == null || (_touchDownPosition - eventData.position).magnitude < Managers.UI._EventSystem.pixelDragThreshold)
                {
                    if (onClick != null)
                    {
                        if (_audioClip_OnClick != null)
                        {
                            if (_audioClip_OnClick != null)
                            {
                                if (Managers.IsValid && Managers.SFX != null)
                                {
                                    Managers.SFX.PlaySFX(
                                        this.transform.position,
                                        this.transform.rotation,
                                        Vector3.one,
                                        null,
                                        _audioClip_OnClick,
                                        SFXType._2D,
                                        false);
                                }
                                else
                                {
                                    AudioSource.PlayClipAtPoint(_audioClip_OnClick, this.transform.position);
                                }
                            }
                        }

                        onClick.Invoke();
                    }
                }

                normalItemState = ItemStateType.Hover;
            }

            ChangeState(normalItemState, true);

            _touchID = -2;
            _isEntered = false;
            _currHoldTime = 0f;
            _touchDownPosition = Vector2.zero;
            if (_holdFxObject != null)
            {
                _holdFxObject.ReturnToPoolForce();
                _holdFxObject = null;
            }

            if (bUseOnPress && onPressEnd != null)
            {
                //if (_audioClip_OnPress != null)
                //{
                //    Managers.SFX.PlaySFX(
                //        this.transform.position,
                //        this.transform.rotation,
                //        Vector3.one,
                //        null,
                //        _audioClip_OnPress,
                //        SFXType._2D,
                //        false);
                //}

                onPressEnd.Invoke();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("Enter");
            if (_touchID == -2)
            {
                if (_buttonState == ButtonStateType.Normal)
                    ChangeState(ItemStateType.Hover, true);
            }
            else if (_touchID == eventData.pointerId)
            {
                _isEntered = true;
                _currHoldTime = 0f;
                ChangeState(ItemStateType.Pressed, true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("exit");
            if (_buttonState != ButtonStateType.Normal)
                return;

            if (_touchID == -2)
            {
                ChangeState(ItemStateType.Normal, true);
            }
            else if (_touchID == eventData.pointerId)
            {
                _isEntered = false;
                ChangeState(ItemStateType.Exited, true);
                if (_holdFxObject != null)
                {
                    _holdFxObject.ReturnToPoolForce();
                    _holdFxObject = null;
                }
            }

            if (EventSystem.current == null || EventSystem.current.isActiveAndEnabled == false)
                OnPointerUp(eventData);
        }

        private void ChangeState(ItemStateType itemState, bool onStateEffect)
        {
            var idx_ItemState = (int)itemState;
            var idx_BtnState = (int)_buttonState;

            if (itemInfos != null)
            {
                for (int i = 0; i < itemInfos.Count; ++i)
                {
                    var itemInfo = itemInfos[i];
                    if (itemInfo == null
                        || itemInfo.Target == null
                        || itemInfo.stateInfo == null
                        || itemInfo.stateInfo.Length <= idx_ItemState)
                        continue;

                    var stateInfo = itemInfo.stateInfo[idx_BtnState * MaxItemStateTypeCount + idx_ItemState];
                    if (stateInfo == null)
                        continue;

                    itemInfo.Target.gameObject.SetActive(stateInfo.Enable);
                    itemInfo.Target.color = stateInfo.Color;
                    itemInfo.Target.transform.localScale = stateInfo.Scale;
                }
            }

            if (goInfos != null)
            {
                for (int i = 0; i < goInfos.Count; ++i)
                {
                    var goInfo = goInfos[i];
                    if (goInfo == null
                        || goInfo.Target == null
                        || goInfo.enableInfo == null
                        || goInfo.enableInfo.Length <= idx_ItemState)
                        continue;

                    var enableInfo = goInfo.enableInfo[idx_BtnState * MaxItemStateTypeCount + idx_ItemState];
                    if (enableInfo == null)
                        continue;

                    goInfo.Target.SetActive(enableInfo.Enable);
                    goInfo.Target.transform.localScale = enableInfo.Scale;
                }
            }

            if (onStateEffect)
            {
                // 사운드 재생
                if (stateSoundClip != null
                    && stateSoundClip.Length > idx_ItemState
                    && stateSoundClip[idx_ItemState] != null)
                {
                    Managers.SFX.PlaySFX(
                        this.transform.position,
                        this.transform.rotation,
                        Vector3.one,
                        null,
                        stateSoundClip[idx_ItemState],
                        SFXType._2D,
                        false);
                }
                
                // 애니메이션 재생
                if (_anim != null)
                {
                    if (stateAnimClip != null
                        && stateAnimClip.Length > idx_ItemState
                        && stateAnimClip[idx_ItemState] != null)
                    {
                        _anim.CrossFade(stateAnimClip[idx_ItemState].name, 0.1f);
                    }
                    else
                    {
                        if (_anim.isPlaying)
                            _anim.Stop();
                    }
                }
            }
        }
    }
}