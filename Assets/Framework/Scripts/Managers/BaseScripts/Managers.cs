using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
namespace Game
{
    public class Managers : MonoBehaviour
    {
        #region Define

        public interface IManagerInitEventCatcher
        {
            void OnManagerInitStateChange(InitState initState);
        }

        public enum InitState
        {
            None                    ,
            Start                   ,
            BGM                     ,
            BGM_End                 ,
            SFX                     ,
            SFX_End                 ,
            FX                      ,
            FX_End                  ,
            UI                      ,
            UI_End                  ,
            
            Net,
            Net_End,
            Scene                   ,
            Scene_End               ,
            GameData                ,
            GameData_End            ,
            End                     ,
        }
        #endregion

        #region Singleton Instance

        public static bool IsValid => _instance != null && _instance.State  == InitState.End;

        private static Managers _instance = null;
        public static Managers Instance
        {
            get
            {
                // 인스턴스가 없다면
                if (_instance == null)
                {
#if !UNITY_EDITOR
                    return null;
#else
                    // 기존에 남아있는 녀석이 있나 확인한다.
                    var go_Manager = GameObject.Find(typeof(Managers).Name);
                    // 기존에 남아있는 녀석이 있으면
                    if (go_Manager != null)
                    {
                        // 컴포넌트를 가져온다.
                        _instance = go_Manager.GetComponent(typeof(Managers)) as Managers;
                        // 없으면 붙여준다.
                        if (_instance == null)
                            _instance = go_Manager.AddComponent(typeof(Managers)) as Managers;
                    }
                    // 없으면 만든다.
                    else
                    {
                        go_Manager = new GameObject(typeof(Managers).Name);
                        _instance = go_Manager.AddComponent(typeof(Managers)) as Managers;
                    }


                    // 초기화가 안되있으면 강제 초기화해준다.
                    // Net, GameBase 제외
                    if (!_instance.IsInit)
                    {
                        DontDestroyOnLoad(_instance);

                        List<IEnumerator> initRoutain = new List<IEnumerator>();

                        _instance._bgm = _instance.CreateManager<BGMManager>();
                        initRoutain.Add(_instance._bgm.Init_Async());

                        _instance._sfx = _instance.CreateManager<SFXManager>();
                        initRoutain.Add(_instance._sfx.Init_Async());

                        _instance._fx = _instance.CreateManager<FXManager>();
                        initRoutain.Add(_instance._fx.Init_Async());

                        _instance._ui = _instance.CreateManager<WindowManager>();
                        initRoutain.Add(_instance._ui.Init_Async());
                        
                        _instance._scene = _instance.CreateManager<SceneManager>();
                        initRoutain.Add(_instance._scene.Init_Async());

                        _instance._gameData = _instance.CreateManager<GameDataManager>();
                        initRoutain.Add(_instance._gameData.Init_Async());
                       
                        for (int i = 0; i < initRoutain.Count; ++i)
                            while (initRoutain[i].MoveNext()) ;

                        _instance.State = InitState.End;
                    }
#endif
                }

                return _instance;
            }
        }
        #endregion

        #region Manager Get Properties
        public static BGMManager BGM { get { return Instance == null ? null : Instance._bgm; } }
        public static SFXManager SFX { get { return Instance == null ? null : Instance._sfx; } }
        public static FXManager FX { get { return Instance == null ? null : Instance._fx; } }
        public static WindowManager UI { get { return Instance == null ? null : Instance._ui; } }
        public static NetworkManager Net { get { return Instance == null ? null : Instance._net; } }
        public static SceneManager Scene { get { return Instance == null ? null : Instance._scene; } }
        public static GameDataManager GameData { get { return Instance == null ? null : Instance._gameData; } }
        #endregion

        #region Manager instance
        private BGMManager _bgm = null;
        private SFXManager _sfx = null;
        private FXManager _fx = null;
        private WindowManager _ui = null;
        private NetworkManager _net = null;
        private SceneManager _scene = null;
        private GameDataManager _gameData = null;        
        #endregion

        public InitState State { get; private set; }
        public bool IsInit { get { return State != InitState.None; } }


#if DevClient
        public void Update()
        {
            if (Camera.allCamerasCount == 0)
                Debug.LogError("Cam is Empty");

            if (ConsoleWindow.instance != null)
            {
                if (Input.GetKeyDown(KeyCode.BackQuote))
                    ConsoleWindow.instance.gameObject.SetActive(!ConsoleWindow.instance.gameObject.activeSelf);

                if (Input.touchCount >= 5)
                    ConsoleWindow.instance.gameObject.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.Q)
                && Input.GetKey(KeyCode.LeftControl))
                GameRestart();
        }
#endif

        public IEnumerator Init_Async(IManagerInitEventCatcher catcher = null)
        {
            if (IsInit)
                yield break;

            yield return SetState(InitState.Start, catcher);

#if DevClient
            var prf_Console = Resources.Load<GameObject>("System/Canvas_Console");
            if (prf_Console != null)
            {
                var go_Console = GameObject.Instantiate(prf_Console) as GameObject;
                go_Console.SetActive(false);
            }
#endif

#if !UNITY_EDITOR
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
            Application.runInBackground = true;

            _instance = this;

            DontDestroyOnLoad(this.gameObject);
            
            yield return SetState(InitState.BGM, catcher);
            _bgm = CreateManager<BGMManager>();
            yield return _bgm.Init_Async();
            yield return SetState(InitState.BGM_End, catcher);

            yield return SetState(InitState.SFX, catcher);
            _sfx = CreateManager<SFXManager>();
            yield return _sfx.Init_Async();
            yield return SetState(InitState.SFX_End, catcher);

            yield return SetState(InitState.FX, catcher);
            _fx = CreateManager<FXManager>();
            yield return _fx.Init_Async();
            yield return SetState(InitState.FX_End, catcher);

            yield return SetState(InitState.UI, catcher);
            _ui = CreateManager<WindowManager>();
            yield return _ui.Init_Async();
            yield return SetState(InitState.UI_End, catcher);

            yield return SetState(InitState.Net, catcher);
            _net = CreateManager<NetworkManager>();
            yield return _net.Init_Async();
            yield return SetState(InitState.Net_End, catcher);

            yield return SetState(InitState.Scene, catcher);
            _scene = CreateManager<SceneManager>();
            yield return _scene.Init_Async();
            yield return SetState(InitState.Scene_End, catcher);

            yield return SetState(InitState.GameData, catcher);
            _gameData = CreateManager<GameDataManager>();
            yield return _gameData.Init_Async();
            yield return SetState(InitState.GameData_End, catcher);

            yield return SetState(InitState.End, catcher);
        }

        private IEnumerator SetState(InitState state, IManagerInitEventCatcher catcher)
        {
            this.State = state;
            catcher?.OnManagerInitStateChange(state);
            yield return null;
        }

        private T CreateManager<T>() where T : ManagerBase
        {
            // 새로운 인스턴스 생성
            GameObject go_NewManager = new GameObject(typeof(T).Name);
            go_NewManager.transform.SetParent(this.transform);

            // 컴포넌트 부착
            var comp_T = go_NewManager.AddComponent(typeof(T)) as T;
            go_NewManager.isStatic = true;

            return comp_T;
        }

        public static void Release()
        {
            if (_instance == null)
                return;

            GameObject.Destroy(_instance.gameObject);
        }

        public static void GameRestart()
        {
            if (Scene == null || Scene.CurrScene == SceneID.Intro)
            {
                Release();
                UnityEngine.SceneManagement.SceneManager.LoadScene("ForcedRestart", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
            else
            {
                // 씬 종료 이벤트 콜
                var go_sceneController = GameObject.FindGameObjectWithTag(SceneManager.STR_CONTROLLER_BASE_TAG_NAME);
                if (go_sceneController != null)
                    go_sceneController.SendMessage("OnEndScene", SceneID.Intro);

                Release();
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }

            UserData.Clear();
        }
    }
}