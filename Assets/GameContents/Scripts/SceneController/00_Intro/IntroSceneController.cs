using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    // 게임 가장 처음 시작될때 들어오는 Main 오브젝트
    // SceneControllerBase는 상속받지 않는다.
    public class IntroSceneController : SceneControllerBase<IntroSceneController>
    {
        [SerializeField]
        private AnimPanel _animPanel_CanvasIntro = null;
        [SerializeField]
        private Managers _managers = null;

        // Todo :: UDP 전환 완료시 삭제
        [SerializeField]
        private bool _useUDP = false;
        public static bool UseUDP { get; private set; } 

        protected override IEnumerator Init()
        {
            UseUDP = _useUDP;

            yield return new WaitUntil(() => _managers != null);

            // 매니저 초기화
            StartCoroutine(_managers.Init_Async());
            
            // UI 초기화까지 대기
            yield return new WaitUntil(() => _managers != null && _managers.State == Managers.InitState.UI_End);

            // Window_Intro 오픈
            var wnd_Intro = Managers.UI.OpenWindow(WindowID.Window_System_Intro) as Window_System_Intro;
            wnd_Intro.SetActive_TouchToStart(false);
            wnd_Intro.SetActive_InitSlider(false);
            wnd_Intro.SetActive_SelectServer(false);

            // 버튼 콜백으로 서버 연결 할당

            // 로고 출력 애니메이션
            _animPanel_CanvasIntro.Play(() =>
                {
                    Destroy(_animPanel_CanvasIntro.gameObject);
                    _animPanel_CanvasIntro = null;
                    Managers.UI.UpdateCamStack();
                },
                null);

            // Window_Intro :: Init Gauge 갱신
            wnd_Intro.SetActive_InitSlider(true);
            var maxState = (float)(Managers.InitState.End - Managers.InitState.UI_End);
            while (true)
            {
                if (_managers.State == Managers.InitState.End)
                    break;

                var crrState = (float)(_managers.State - Managers.InitState.UI_End);
                wnd_Intro.SetInitSlider(crrState / maxState);

                yield return null;
            }

            // 게임 시작 버튼 활성화
            wnd_Intro.SetActive_InitSlider(false);
            wnd_Intro.SetActive_TouchToStart(true);
            wnd_Intro.SetTouchToStartCallback(() => Managers.Net.AutoConnect(true));

            // 서버 연결 시까지 대기
            yield return new WaitUntil(() => Managers.Net.State == NetworkManager.ConnectState.Done);

            // 진행 중인 게임이 있다면?
            if (UserData.RoomInfo.GameRoom != null)
            {
                Managers.UI.EnqueuePopup(
                    "알림",
                    "진행중인 게임이 있습니다.\n\n계속 진행하시겠습니까?",
                    this.GoToBattle,
                    this.GoToLobby
                    );
            }
            // 없다면
            else
            {
                GoToLobby();
            }
        }

        private void GoToLobby()
        {
            // 로비 진입
            Managers.Scene.LoadScene(SceneID.Lobby, LoadingID.Loading_Box_Scale);
            //NetProcess.Request_EnterLobby(()=>
            //    Managers.Scene.LoadScene(SceneID.Lobby, LoadingID.Loading_Box_Scale));
        }

        private void GoToBattle()
        {
            Managers.Scene.LoadScene(SceneID.Battle, LoadingID.Loading_FadeInOut);
        }
    }
}
