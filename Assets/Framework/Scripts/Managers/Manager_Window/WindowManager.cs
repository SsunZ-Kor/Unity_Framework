using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game
{
    public partial class WindowManager : ManagerBase
    {
        public RectTransform _rttr_CanvasRoot = null;
        public Canvas _CanvasRoot = null;
        public EventSystem _EventSystem = null;

        private GameObject _go_NetBlock;
        private bool bActive_NetBlock = false;
        private bool bActive_ConnectBlock = false;

        private readonly Dictionary<WindowID, WindowBase> _dic_WindowInsts = new Dictionary<WindowID, WindowBase>();
        private readonly LinkedList<WindowBase> _llist_WindowStack = new LinkedList<WindowBase>();

        public RectTransform _rttr_holderForWndNormal = null;
        public RectTransform _rttr_holderForGNB = null;
        public RectTransform _rttr_holderForWndPopup = null;
        public RectTransform _rttr_holderForToast = null;
        public RectTransform _rttr_holderForPopup = null;
        public RectTransform _rttr_holderForNetAjection = null;
        public RectTransform _rttr_holderForLoading = null;
        public RectTransform _rttr_holderForPopup_System = null;
        public RectTransform _rttr_Modal = null;

        public Vector2 Resolution => _rttr_CanvasRoot.rect.size;

        public Rect ResolutionRect => _rttr_CanvasRoot.rect;

        public GlobalNavigationBar GNB { get; private set; }

        public bool IsNetBlockOn
        {
            get
            {
                if (_go_NetBlock == null)
                    return false;

                return _go_NetBlock.activeSelf;
            }
        }

        public bool IsInStack(WindowID windowID)
        {
            var wnd = _dic_WindowInsts.GetOrNull(windowID);
            if (wnd == null)
                return false;

            return wnd.Node_WindowStack.List == this._llist_WindowStack;
        }


        #region Initialize
        public override IEnumerator Init_Async()
        {

#if UNITY_EDITOR
            _CanvasRoot = GameObject.FindObjectOfType(typeof(Canvas)) as Canvas;
            if (_CanvasRoot == null || !_CanvasRoot.gameObject.name.Contains("_Main"))
            {
#endif
                var prf_CanvasMain = Resources.Load<GameObject>("System/Canvas_Main");
                var go_CanvasMain = GameObject.Instantiate(prf_CanvasMain) as GameObject;

                _CanvasRoot = go_CanvasMain.GetComponent(typeof(Canvas)) as Canvas;

                yield return null;
                var canvasPerfect = go_CanvasMain.GetComponentsInChildren<UGUICanvasPerfect>();
                if (canvasPerfect != null)
                    canvasPerfect.ForEach((x) => x.Update_SizeDelta());
#if UNITY_EDITOR
            }

            _EventSystem = GameObject.FindObjectOfType(typeof(EventSystem)) as EventSystem;
            if (_EventSystem == null)
            {
#endif
                var prf_EventMain = Resources.Load<GameObject>("System/EventSystem_Main");
                var go_EventMain = GameObject.Instantiate(prf_EventMain) as GameObject;

                _EventSystem = go_EventMain.GetComponent(typeof(EventSystem)) as EventSystem;
                int defaultValue = _EventSystem.pixelDragThreshold;
                _EventSystem.pixelDragThreshold =
                        Mathf.Max(
                             defaultValue,
                             (int)(defaultValue * Screen.dpi / 160f));
#if UNITY_EDITOR
            }
#endif
            _rttr_CanvasRoot = _CanvasRoot.transform as RectTransform;

            DontDestroyOnLoad(_CanvasRoot.gameObject);
            DontDestroyOnLoad(_EventSystem.gameObject);
            
            _rttr_holderForWndNormal = _rttr_CanvasRoot.Find("Holder_WndNormal") as RectTransform;
            _rttr_holderForGNB = _rttr_CanvasRoot.Find("Holder_GNB") as RectTransform;
            _rttr_holderForWndPopup = _rttr_CanvasRoot.Find("Holder_WndPopup") as RectTransform;
            _rttr_holderForToast = _rttr_CanvasRoot.Find("Holder_Toast") as RectTransform;
            _rttr_holderForPopup = _rttr_CanvasRoot.Find("Holder_Popup") as RectTransform;
            _rttr_holderForNetAjection = _rttr_CanvasRoot.Find("Holder_NetAjection") as RectTransform;
            _rttr_holderForLoading = _rttr_CanvasRoot.Find("Holder_Loading") as RectTransform;
            _rttr_holderForPopup_System = _rttr_CanvasRoot.Find("Holder_Popup_System") as RectTransform;

            _rttr_Modal = _rttr_CanvasRoot.Find("Modal") as RectTransform;
            if (_rttr_Modal != null)
            {
                var btn_Modal = _rttr_Modal.gameObject.GetOrAddComponent(typeof(ButtonEx)) as ButtonEx;
                btn_Modal.onClick.Subscribe(() => this.CloseLast(false));
                _rttr_Modal.gameObject.SetActive(false);
            }

            yield return null;

            // GNB
            // 팝업 설정
            var prf_GNB = Resources.Load<GameObject>("System/GNB");
            if (prf_GNB == null)
            {
                Debug.LogError("System_UI/GNB is Not Found");
            }
            else
            {
                var go_Popup = GameObject.Instantiate(prf_GNB, _rttr_holderForGNB) as GameObject;
                go_Popup.transform.Reset();
                go_Popup.gameObject.SetActive(true);
                GNB = go_Popup.GetComponent<GlobalNavigationBar>();
                GNB.SetItem(GlobalNavigationBar.ItemType.None);
            }


            // 팝업 설정
            this._popupPairs = new PopupPair[2];
            var prf_GamePopup = Resources.Load<GameObject>("System/GamePopup");
            if (prf_GamePopup == null)
            {
                Debug.LogError("System_UI/Window_Game_Popup is Not Found");
            }
            else
            {
                var go_Popup = GameObject.Instantiate(prf_GamePopup, _rttr_holderForPopup) as GameObject;
                go_Popup.transform.Reset();
                go_Popup.gameObject.SetActive(false);
                var comp_Popup = go_Popup.GetComponent<SystemPopup>();
                comp_Popup.Init(PopupType.Game);

                _popupPairs[0] = new PopupPair(comp_Popup);
            }

            var prf_SystemPopup = Resources.Load<GameObject>("System/SystemPopup");
            if (prf_SystemPopup == null)
            {
                Debug.LogError("System_UI/Window_System_Popup is Not Found");
            }
            else
            {
                var go_Popup = GameObject.Instantiate(prf_SystemPopup, _rttr_holderForPopup_System) as GameObject;
                go_Popup.transform.Reset();
                go_Popup.gameObject.SetActive(false);
                var comp_Popup = go_Popup.GetComponent<SystemPopup>();
                comp_Popup.Init(PopupType.System);

                _popupPairs[1] = new PopupPair(comp_Popup);
            }

            yield return null;

            // 넷 블럭
            var prf_NetAjection = Resources.Load<GameObject>("System/NetBlock");
            if (prf_NetAjection != null)
            {
                _go_NetBlock = GameObject.Instantiate(prf_NetAjection, _rttr_holderForNetAjection);
                _go_NetBlock.transform.Reset();
                _go_NetBlock.gameObject.SetActive(false);
            }

            Init_Background();
        }
        #endregion

        #region Mono Life Cycle
        private void Awake()
        {
            Awake_Touch();
        }
        private void Update()
        {
            Update_Touch();
        }

        private void LateUpdate()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            // 이벤트 시스템이 꺼져 있으면 return
            if (this._EventSystem == null || !this._EventSystem.isActiveAndEnabled)
                return;

            CloseLast(false);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            OnDisable_Background();
        }
        
        private void OnDestroy()
        {
            if (_CanvasRoot != null)
                GameObject.DestroyImmediate(_CanvasRoot.gameObject);
            if (_EventSystem != null)
                GameObject.DestroyImmediate(_EventSystem.gameObject);

            OnDestroy_Background();
            OnDestroy_Touch();
        }

#endregion

#region For Window

        /// <summary>
        /// 윈도우 인스턴스를 반환합니다.
        /// </summary>
        /// <param name="windowId">획득 하려는 윈도우 ID</param>
        /// <param name="ifNotExistCreate">인스턴스가 없을 경우 인스턴스를 생성할 것인가? true : 생성한다 // false : 생성하지 않는다</param>
        /// <returns></returns>
        public WindowBase GetWindow(WindowID windowId, bool ifNotExistCreate)
        {
            WindowBase result = null;
            if (_dic_WindowInsts.TryGetValue(windowId, out result))
            {
                if (result == null)
                    _dic_WindowInsts.Remove(windowId);
            }

            if (result != null)
                return result;

            if (!ifNotExistCreate)
                return null;

            var prf_Window = Resources.Load<GameObject>($"UI/Prefab_Window/{windowId}");
            if (prf_Window == null)
                throw new System.Exception(string.Format("윈도우 프리펩을 찾을 수 없습니다 : {0}", windowId));

            var go_Window = GameObject.Instantiate(prf_Window, this._rttr_holderForWndNormal) as GameObject;
            result = go_Window.GetComponent(typeof(WindowBase)) as WindowBase;
            result.SetWindowId(windowId);
            this._dic_WindowInsts.Add(windowId, result);

            if (result.IsPopup)
                result.transform.SetParent(this._rttr_holderForWndPopup);

            var rectTransform = result.transform as RectTransform;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;

            if (go_Window.activeSelf)
                go_Window.gameObject.SetActive(false);

            return result;
        }

        public WindowBase GetLastWindow()
        {
            return _llist_WindowStack?.Last?.Value;
        }

        public WindowID GetLastWindowId()
        {
            var wnd_Last = GetLastWindow();
            if (wnd_Last == null)
                return WindowID.NONE;

            return wnd_Last.WindowId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowId"></param>
        /// <returns>현재 켜져있던 Window가 Open을 거부했을 경우 
        /// 요청한 Window는 activeFalse상태로 리턴됩니다. </returns>
        public WindowBase OpenWindow(WindowID windowId)
        {
            var window = GetWindow(windowId, true);

            if (!window.IsPopup)
            {
                var node_Window = _llist_WindowStack.Last;
                while (node_Window != null)
                {
                    var closeWindow = node_Window.Value;
                    node_Window = node_Window.Previous;

                    if (!closeWindow.gameObject.activeSelf)
                        break;

                    closeWindow.OnEvent_OutLastDepth(false);
                    closeWindow.gameObject.SetActive(false);
                }
            }
            else
            {
                var node_Window = _llist_WindowStack.Last;
                if (node_Window != null)
                    node_Window.Value.OnEvent_OutLastDepth(false);
            }

            if (window.Node_WindowStack.List != null)
                window.Node_WindowStack.List.Remove(window.Node_WindowStack);

            _llist_WindowStack.AddLast(window.Node_WindowStack);
            window.gameObject.SetActive(true);
            window.transform.SetAsLastSibling();

            window.OnEvent_AfterOpen();
            window.OnEvent_OnLastDepth();

            RefreshModalState();
            window.RefreshSubmoduleUI();

            return window;
        }

        public void RefreshLast()
        {
            var windowNode = _llist_WindowStack.Last;
            if (windowNode == null)
                return;

            // 팝업이 아닌 Window까지 찾는다.
            while (windowNode != null && windowNode.Value.IsPopup)
            {
                var prevWindowNode = windowNode.Previous;
                if (prevWindowNode == null 
                    || !prevWindowNode.Value.gameObject.activeSelf)
                    break;

                windowNode = windowNode.Previous;
            }

            // 갱신
            while (windowNode != null)
            {
                windowNode.Value.Refresh();
                windowNode = windowNode.Next;
            }
        }

        public bool CloseLast(bool bForce)
        {
            if (_popupPairs != null)
            {
                for (int i = _popupPairs.Length -1; i >= 0; --i)
                {
                    var pair = _popupPairs[i];
                    if (pair != null && pair.IsOpened)
                        return pair.comp_popup.AutoCancel();
                }
            }

            if (!bForce)
            {
                if ((Managers.Scene != null && Managers.Scene.loadingState != SceneManager.LoadingState.None)
                    || (_go_NetBlock != null && _go_NetBlock.activeSelf))
                    return false;
            }

            if (_llist_WindowStack == null || _llist_WindowStack.Count == 0)
                return true;

            var node_CloseWindow = _llist_WindowStack.Last;
            if (node_CloseWindow == null || node_CloseWindow.Value == null)
            {
                _llist_WindowStack.RemoveLast();
                return true;
            }

            return node_CloseWindow.Value.CloseSelf();
        }

        public bool CloseWindow(WindowID windowID)
        {
            return CloseWindow(_dic_WindowInsts.GetOrNull(windowID));
        }

        public bool CloseWindow(WindowBase closeWindow)
        {
            if (closeWindow == null)
                return true;

            // 이미 꺼져 있는거여도 리턴
            if (closeWindow.Node_WindowStack.List == null)
                return true;

            // 일단 스택에서 제거
            var node_CloseWindow = closeWindow.Node_WindowStack;
            node_CloseWindow.List.Remove(node_CloseWindow);

            closeWindow.OnEvent_OutLastDepth(true);

            // 켜져 있는데 팝업이 아니라면 마지막 스택부터 팝업이 아닐때 까지 킨다.
            if (closeWindow.gameObject.activeSelf && !closeWindow.IsPopup)
            {
                var node_OpenWindow = _llist_WindowStack.Last;
                while (node_OpenWindow != null)
                {
                    var openWindow = node_OpenWindow.Value;
                    if (!openWindow.IsPopup)
                    {
                        openWindow.RefreshSubmoduleUI();
                        break;
                    }

                    node_OpenWindow = node_OpenWindow.Previous;
                }

                while (node_OpenWindow != null)
                {
                    var openWindow = node_OpenWindow.Value;
                    node_OpenWindow = node_OpenWindow.Next;

                    openWindow.gameObject.SetActive(true);
                    openWindow.OnEvent_OnLastDepth();
                }
            }
            else
            {
                var node_OpenWindow = _llist_WindowStack.Last;
                if (node_OpenWindow != null)
                    node_OpenWindow.Value.OnEvent_OnLastDepth();
            }

            // 윈도우 닫기
            closeWindow.gameObject.SetActive(false);
            RefreshModalState();

            return true;
        }

        /// <summary>
        /// 주의! 모든 WindowEvent를 거치지 않고 스택상의 Window가 Off됩니다.
        /// </summary>
        /// <param name="bDestroy"></param>
        public void Clear(bool bDestroy)
        {
            if (GNB != null)
                GNB.SetItem(GlobalNavigationBar.ItemType.None);

            var node_closeWindow = _llist_WindowStack.Last;
            while (node_closeWindow != null)
            {
                var closeWindow = node_closeWindow.Value;
                node_closeWindow = node_closeWindow.Previous;

                closeWindow.gameObject.SetActive(false);
            }

            _llist_WindowStack.Clear();

            if (bDestroy)
            {
                foreach (var pair in _dic_WindowInsts)
                {
                    if (pair.Value != null)
                        GameObject.Destroy(pair.Value.gameObject);
                }

                _dic_WindowInsts.Clear();
            }

            // 배경정리
            OnClear_Background(bDestroy);
            RefreshModalState();
            UpdateCamStack();
        }

        public void Clear(params WindowID[] windowIDs)
        {
            if (windowIDs == null)
                return;
            
            for (int i = 0; i< windowIDs.Length; ++i)
            {
                var wnd = _dic_WindowInsts.GetOrNull(windowIDs[i]);
                if (wnd == null)
                    continue;

                if (wnd.gameObject.activeSelf)
                    wnd.CloseSelf();

                _dic_WindowInsts.Remove(windowIDs[i]);
                GameObject.Destroy(wnd.gameObject);
            }
        }

        public bool IsLastWindow()
        {
            return _llist_WindowStack.Count == 1;
        }
#endregion

        public Vector2 ConvertScreenToCanvasPoint(Vector2 vScreen)
        {
            Vector2 result;

            if (this._rttr_CanvasRoot == null
				|| this._CanvasRoot == null 
				|| this._CanvasRoot.worldCamera == null)
                return Vector2.zero;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    this._rttr_CanvasRoot, vScreen, this._CanvasRoot.worldCamera, out result);

            return result;
        }

        public Vector2 ConvertWorldToCanvasPoint(Vector3 vWorld)
        {
            return this._rttr_CanvasRoot.InverseTransformPoint(vWorld);
        }

        public Vector2 ConvertViewportToCanvasPoint(Vector3 vViewport)
        {
            var vWorld = this._CanvasRoot.worldCamera.ViewportToWorldPoint(vViewport);
            return this._rttr_CanvasRoot.InverseTransformPoint(vWorld);
        }

        public Vector2 ConvertViewportToWorldPoint(Vector3 vViewport)
        {
            return this._CanvasRoot.worldCamera.ViewportToWorldPoint(vViewport);
        }

        public void SetActiveNetBlock(bool bActive)
        {
            this.bActive_NetBlock = bActive;

            if (_go_NetBlock != null)
                _go_NetBlock.SetActive(bActive_NetBlock || this.bActive_ConnectBlock);
        }


        public void SetActiveConnectBlock(bool bActive)
        {
            this.bActive_ConnectBlock = bActive;

            if (_go_NetBlock != null)
                _go_NetBlock.SetActive(bActive_NetBlock || this.bActive_ConnectBlock);
        }

        public void RefreshModalState()
        {
            if (_rttr_Modal == null)
                return;

            RectTransform rttr_Parent = null;
            int idx_Sibling = -1;

            _rttr_Modal.gameObject.SetActive(false);
            _rttr_Modal.SetParent(_rttr_CanvasRoot);

            // 시스템 팝업이 살아있다면
            if (_popupPairs != null)
            {
                for (int i = _popupPairs.Length - 1; i >= 0; --i)
                {
                    var pair = _popupPairs[i];
                    if (pair.IsOpened)
                    {
                        rttr_Parent = pair.comp_popup.transform.parent as RectTransform;
                        idx_Sibling = pair.comp_popup.transform.GetSiblingIndex();
                    }
                }
            }

            if (rttr_Parent == null)
            {
                if (_llist_WindowStack == null || _llist_WindowStack.Count <= 0)
                    return;

                var node_Window = _llist_WindowStack.Last;
                if (node_Window == null)
                    return;


                while (node_Window != null)
                {
                    var window = node_Window.Value;
                    node_Window = node_Window.Previous;

                    if (!window.gameObject.activeSelf)
                        break;

                    if (window.UseBlurBackground)
                    {
                        rttr_Parent = window.transform.parent as RectTransform;
                        idx_Sibling = window.transform.GetSiblingIndex();
                        break;
                    }
                }
            }

            if (rttr_Parent != null)
            {
                _rttr_Modal.gameObject.SetActive(true);
                _rttr_Modal.SetParent(rttr_Parent);
                _rttr_Modal.SetSiblingIndex(idx_Sibling);

                _rttr_Modal.anchoredPosition = Vector3.zero;
                _rttr_Modal.sizeDelta = new Vector2(2340, 0);
            }
        }
    }
}