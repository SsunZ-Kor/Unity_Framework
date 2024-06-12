using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public abstract class WindowBase : MonoBehaviour
    {
        public enum BgmType
        {
            None,
            OneShot,
            Loop,
            KeepPrevBgm,
        }

        // WindowManager에서 스택 관리 하기 위한 Node
        public LinkedListNode<WindowBase> Node_WindowStack { get; private set; }
        public WindowID WindowId { get; private set; }

        [Header("WindowBase")]
        [SerializeField]
        protected Animation _anim = null;
        [SerializeField]
        protected AnimationClip _animClip_OpenWindow;

        [Header("WindowBase :: Popup Setting")]
        // Popup형태 (이전 스택의 UI와 겹침) 여부
        public bool IsPopup = false;
        public bool UseBlurBackground = true;

        [Header("WindowBase :: GNB setting -Non popup type only-")]
        [SerializeField]
        protected GlobalNavigationBar.ItemType _gnbItemSetting;

        // Background 관련
        [Header("WindowBase :: BG setting -Non popup type only-")]
        [SerializeField]
        protected BackgroundID _backgroundId = BackgroundID.BG_None;
        [SerializeField]
        protected float _changeTime = 0.3f;

        // BGM 관련
        [Header("WindowBase :: BGM setting -Non popup type only(Loop)-")]
        [SerializeField]
        public BgmType _bgmUserBgm = BgmType.None;
        [SerializeField]
        public AudioClip _bgm_audioClip = null;
        [SerializeField]
        public bool _bgm_keepPrevNormalizedTime = false;
        [SerializeField]
        public BGMManager.FadeType _bgm_FadeType = BGMManager.FadeType.CrossFade;
        [SerializeField]
        public float _bgm_FadeTime = 1f;

        [Header("WindowBase :: Others")]
        public int _tutorialGuideDataId = -1;


        //[SerializeField]
        //protected Text m_txtTitle = null;
        //[SerializeField]
        //protected Text m_txtSubTitle = null;

        protected string _lzKey_Title;
        protected string _lzKey_Desc;

        protected bool IsApplicationQuitting { get; private set; } = false;
        public BackgroundID BgId => _backgroundId;

        public bool IsLastDepth => Node_WindowStack.List != null && Node_WindowStack.Next == null && !Managers.UI.IsActivePopup;

        protected virtual void Awake()
        {
            if (_anim != null)
                _anim.playAutomatically = false;

            Node_WindowStack = new LinkedListNode<WindowBase>(this);

            var wndTypeName = this.GetType().Name;
            _lzKey_Title = $"{wndTypeName}_Title";
            _lzKey_Desc = $"{wndTypeName}_Desc";

            Application.quitting += OnQuitting;
        }

        protected virtual void OnDestroy()
        {
            Application.quitting -= OnQuitting;
        }

        /// <summary>
        /// 뒤로가기, 새로열기 등 화면 전면에 나오면 무조건 호출됨
        /// </summary>
        public virtual void Refresh()
        {
        }

        public virtual void RefreshSubmoduleUI()
        {
            RefreshGNB();
            RefreshBackground();
            RefreshBgm();
        }

        public virtual void RefreshGNB()
        {
            if (IsPopup)
                return;

            Managers.UI.GNB.SetItem(_gnbItemSetting);
        }

        public virtual void RefreshBackground()
        {
            if (IsPopup)
                return;

            Managers.UI.ChangeBackground(_backgroundId, _changeTime, null);
        }

        public virtual void RefreshBgm()
        {
            if (!IsPopup)
            {
                switch (this._bgmUserBgm)
                {
                    case BgmType.None:
                    {
                        Managers.BGM.PlayBGM(null, this._bgm_FadeType, this._bgm_FadeTime, this._bgm_keepPrevNormalizedTime);
                    }
                    break;
                    case BgmType.Loop:
                    {
                        Managers.BGM.PlayBGM(this._bgm_audioClip, this._bgm_FadeType, this._bgm_FadeTime, this._bgm_keepPrevNormalizedTime);
                    }
                    break;
                    case BgmType.OneShot:
                    {
                        Managers.BGM.PlayBGM(null, this._bgm_FadeType, this._bgm_FadeTime, this._bgm_keepPrevNormalizedTime);
                        if (this._bgm_audioClip != null)
                            Managers.SFX.PlaySFX(this._bgm_audioClip, SFXType._2D);
                    }
                    break;
                    case BgmType.KeepPrevBgm:
                    break;
                }
            } 
            else
            {
                if (this._bgmUserBgm == BgmType.OneShot && this._bgm_audioClip != null)
                    Managers.SFX.PlaySFX(this._bgm_audioClip, SFXType._2D);
            }
        }

        /// <summary>
        /// 스택에 없던 WIndow가 새로열린 직후에만 호출됨
        /// </summary>
        public virtual void OnEvent_AfterOpen()
        {
            SafeAddAnimClip(_animClip_OpenWindow);
            if (_anim != null && _animClip_OpenWindow != null)
                _anim.Play(_animClip_OpenWindow.name);            
        }

        /// <summary>
        /// 다음과 같은 경우에 이벤트가 호출됩니다.
        /// 
        /// 1. 신규로 OpenWindow 될 때
        /// 2. 이후에 켜진 Window가 모두 Close되어 재 Open될때
        /// </summary>
        public virtual void OnEvent_OnLastDepth()
        {
            this.Refresh();
        }

        /// <summary>
        /// 다음과 같은 경우에 이벤트가 호출됩니다.
        /// 
        /// 1. Popup을 포함한 화면 최상단에서 출력되던 도중, 다른 Window 혹은 Popup이 OpenWindow 되기 직전
        /// 2. Window가 스택에서 제거되어 완전히 종료될 때
        /// </summary>
        /// <param name="isClose">
        /// true : Close로 인한 Inactive, 즉 윈도우 스택에서 Pop 될 때
        /// false : OpenWindow로 인한 Inactive
        /// </param>
        public virtual void OnEvent_OutLastDepth(bool isClose)
        {
            if (_anim != null && _animClip_OpenWindow != null)
            {
                if (!isClose && _anim.IsPlaying(_animClip_OpenWindow.name))
                {
                    _anim.Stop();
                    this.SampleAnimClip(_animClip_OpenWindow.name, 1f);
                }
            }
        }

        public virtual bool CloseSelf()
        {
            return Managers.UI.CloseWindow(this);
        }

        protected void SafeAddAnimClip(AnimationClip animClip)
        {
            if (animClip == null)
                return;

            if (_anim == null)
            {
                _anim = this.gameObject.GetOrAddComponent(typeof(Animation)) as Animation;
                _anim.playAutomatically = false;
            }

            if (_anim.GetClip(animClip.name) != null)
                _anim.RemoveClip(animClip.name);

            _anim.AddClip(animClip, animClip.name);
        }

        protected void SampleOpenAnimClip(float factor)
        {
            if (_animClip_OpenWindow == null)
                return;

            SampleAnimClip(_animClip_OpenWindow, factor);
        }

        public void SampleAnimClip(string clipName, float factor)
        {
            if (_anim == null
                || string.IsNullOrEmpty(clipName)
                || _anim.GetClip(clipName) == null)
                return;

            _anim.Stop();
            _anim[clipName].enabled = true;
            _anim[clipName].time = _anim[clipName].length * factor;
            _anim[clipName].weight = 1;
            _anim.Sample();
            _anim[clipName].enabled = false;
        }

        protected void SampleAnimClip(AnimationClip clip, float factor)
        {
            if (clip == null)
                return;

            SampleAnimClip(clip.name, factor);
        }

        private void OnQuitting()
        {
            IsApplicationQuitting = true;
        }

        public void SetWindowId(WindowID windowId)
        {
            this.WindowId = windowId;
        }
    }
}