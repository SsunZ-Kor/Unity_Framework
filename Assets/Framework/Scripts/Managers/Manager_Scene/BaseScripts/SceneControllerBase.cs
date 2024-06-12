using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class SceneControllerBase<T> : MonoBehaviour where T : SceneControllerBase<T>
    {
        public static T Instance { get; private set; } = null;

#if UNITY_EDITOR
        protected virtual bool IsTestScene => false;
#endif

        protected virtual void Awake()
        {
            Instance = this.GetComponent<T>();

            this.gameObject.tag = SceneManager.STR_CONTROLLER_BASE_TAG_NAME;
            StartCoroutine(Init());
        }

        /// <summary>
        /// 상속받은 자식들은 Override해서 작성한 후 마지막에 꼭 return yield base.Init()을 해주어야 합니다
        /// 
        /// GC.Collect 호출과 Manager.Scene에게 로딩을 끝남을 알려줍니다.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator Init()
        {
            yield return null;
            
            System.GC.Collect();

#if UNITY_EDITOR
            if (IsTestScene)
            {
                var canvasPerfect = Managers.UI._CanvasRoot.GetComponentsInChildren<UGUICanvasPerfect>();
                if (canvasPerfect != null)
                    canvasPerfect.ForEach((x) => x.Update_SizeDelta());
            }
#endif

            // 이 호출스택에서 로딩 UI를 종료하고 this.OnStartScene를 호출합니다.
            Managers.Scene.OnEndLoading();
        }

        /// <summary>
        /// 씬 시작 이벤트
        /// 
        /// 각 Scene의 MainController.Awake()
        /// -> Init()
        /// -> Managers.Scene.OnEndLoading();
        /// 의 호출 스택에서 
        /// SceneManager에서 SendMessage 형식으로 호출됨
        /// </summary>
        protected virtual void OnStartScene()
        {

        }

        /// <summary>
        /// 씬 종료 이벤트
        /// 
        /// Loading 페이지가 완전히 화면을 가린 후
        /// SceneManager에서 SendMessage 형식으로 호출됨
        /// 씬 전환 호출 직전에 호출되어 OnDestroy 보다 빠름
        /// </summary>
        protected virtual void OnEndScene(SceneID nextScene)
        {
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Init중에서만 사용하는 에러 팝업
        /// </summary>
        /// <param name="lz_title"></param>
        /// <param name="lz_contents"></param>
        /// <param name="okCallback"></param>
        protected virtual void ErrorPopupOnInit(string title, string contents, System.Action okCallback)
        {
            Managers.UI.Clear(true);
            Managers.UI.ChangeBackground(BackgroundID.BG_Lobby, 0f, null);
            Managers.UI.EnqueuePopup(title, contents, okCallback, WindowManager.PopupType.Game);
        }
    }
}