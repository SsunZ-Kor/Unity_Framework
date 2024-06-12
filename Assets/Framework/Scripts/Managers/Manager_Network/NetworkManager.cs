using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameAnvil;
using GameAnvil.Defines;
using GameAnvil.Connection;
using GameAnvil.User;
using BubbleFighter.Network.Protocol;
using System;
using GameAnvil.Connection.Defines;

namespace Game
{
    public partial class NetworkManager : ManagerBase
    {
        public enum ConnectState
        { 
            None,
            ConnectSession,
            Auth,
            GetChannelList,
            GetChannelInfo,
            Login,
            Done,
        }

        public class ServerInfo
        {
            public string serverName;
            public string serviceName;
            public int serviceSubId;
            public string ip;
            public int port;
            public int udp_port;
        }

        public static ServerInfo[] ServerInfos = new ServerInfo[]
        {
            // 테스트용 개발 서버
            new ServerInfo()
            {
                serverName = "TEST",
                serviceName = "GameService",
                serviceSubId = 1,
                ip = "133.186.142.59",
                port = 11400,
                udp_port = 11401,
            },
#if UNITY_EDITOR
            // 서버 대규 책임님 개인 PC
            new ServerInfo()
            {
                serverName = "DAE-GYU",
                serviceName = "GameService",
                serviceSubId = 1,
                ip = "10.77.11.41",
                port = 11200,
                udp_port = 11201,
            },
#endif
        };

        public ServerInfo CrrServerInfo = null;

        public string deviceId;
        public string accountId;
        public string userType;
        public string userId;
        public string authToken;

        private Connector connector;
        private System.Threading.Thread connectorThread;

        private ConnectionAgent connectionAgent = null;
        public UserAgent userAgent { get; private set; } = null;
        private string selectedChannelId;

        public ConnectState State { get; private set; }

        public bool IsConnected
        {
            get
            {
                return State == ConnectState.Done
                    && connector != null
                    && connector.IsConnected()
                    && connector.IsAuthenticate()
                    && this.CrrServerInfo != null
                    && connector.IsLoggedIn(this.CrrServerInfo.serviceName, this.CrrServerInfo.serviceSubId)
                    && userAgent != null
                    && userAgent.IsLoggedIn();
            }
        }

        private void Awake()
        {
            // 프로토콜 등록
            var protocolMgr = ProtocolManager.getInstance();
            if (protocolMgr != null)
                protocolMgr.RegisterProtocol(0, BubbleFighter.Network.Protocol.ProtocolReflection.Descriptor);
        }

        private void Update()
        {
            if (connector != null)
            {
                connector.Update();

                // UDP Logic
                udpParse();
            }
        }

        private void OnDestroy()
        {
            if (userAgent != null)
            {
                userAgent.onLoginListeners -= OnLogin;
                NetProcess.RemoveListener(userAgent);
            }

            if (connectionAgent != null)
            {
                connectionAgent.onConnectListeners -= OnConnect;
                connectionAgent.onAuthenticationListeners -= OnAuthenticate;
                connectionAgent.onChannelListListeners -= OnGetChannelList;
                connectionAgent.onChannelInfoListeners -= OnGetChannelInfo;
                connectionAgent.onDisconnectListeners -= OnDisconnected;
                connectionAgent.onErrorCommandListeners -= OnError;
                connectionAgent.onErrorCustomCommandListeners -= OnError;
            }

            // 프로토콜 해제
            var protocolMgr = ProtocolManager.getInstance();
            if (protocolMgr != null)
                protocolMgr.UnregisterProtocol(0);
        }

        public override IEnumerator Init_Async()
        {
            if (CrrServerInfo == null)
            {
                if (ServerInfos.Length < 1)
                {
                    throw new Exception("ServerInfos is Null or Zero");
                }
                else if (ServerInfos.Length == 1)
                {
                    CrrServerInfo = ServerInfos[0];
                    if (CrrServerInfo == null)
                        throw new Exception("ServerInfos[0] is Null");
                }
                else
                {
                    Window_System_Intro wnd_Intro = null;
                    while (wnd_Intro == null)
                    {
                        wnd_Intro = Managers.UI.GetWindow(WindowID.Window_System_Intro, false) as Window_System_Intro;
                        yield return null;
                    }

                    wnd_Intro.SetActive_InitSlider(false);
                    wnd_Intro.SetActive_SelectServer(true);

                    yield return new WaitUntil(() => CrrServerInfo != null);

                    wnd_Intro.SetActive_InitSlider(true);
                }
            }

            deviceId = SystemInfo.deviceUniqueIdentifier;
            if (PlayerPrefs.HasKey("ACCOUNT_ID"))
            {
                accountId = PlayerPrefs.GetString("ACCOUNT_ID");
            }
            else
            {
                accountId = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString("ACCOUNT_ID", accountId);
            }

            userType = "GameUser";

            State = ConnectState.None;

            yield break;
        }

        public void AutoConnect(bool bForced)
        {
            // 커넥트 중인지 체크
            if (!bForced && State != ConnectState.Done)
                return;

            Managers.UI.SetActiveConnectBlock(true);
            State = ConnectState.None;

#if DevClient
            Debug.Log("NetworkManager->AutoConnect");
#endif

            // 인터넷 연결 상태 체크
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Managers.UI.EnqueuePopup(
                    "알림",
                    "네트워크가 연결되어있지 않습니다.",
                    "재시도",
                    () => AutoConnect(true),
                    WindowManager.PopupType.System);
            }

            if (connector == null || !connector.IsConnected())
            {
                ConnectSession();
                return;
            }
            
            if (!connector.IsAuthenticate())
            {
                Authenticate();
                return;
            }

            userAgent = connector.GetUserAgent(CrrServerInfo.serviceName, CrrServerInfo.serviceSubId);
            if (userAgent == null || !userAgent.IsLoggedIn())
            {
                GetChannelList();
                return;
            }

            State = ConnectState.Done;
            Managers.UI.SetActiveConnectBlock(false);
        }

        private void ConnectSession()
        {
            if (connector == null)
            {
                connector = new Connector();
                connector.config.packetTimeout = 15;
                connector.config.defaultReqestTimeout = 15;
                connector.config.pingInterval = 7; // 추후 전투 상황에서와 로비 상황에서의 핑 인터벌을 다르게 주어야함
                connector.config.useIPv6 = true;
                
                connectionAgent = connector.GetConnectionAgent();
                connectionAgent.onConnectListeners += OnConnect;
                connectionAgent.onAuthenticationListeners += OnAuthenticate;
                connectionAgent.onChannelListListeners += OnGetChannelList;
                connectionAgent.onChannelInfoListeners += OnGetChannelInfo;
                connectionAgent.onDisconnectListeners += OnDisconnected;
                connectionAgent.onErrorCommandListeners += OnError;
                connectionAgent.onErrorCustomCommandListeners += OnError;
            }

#if DevClient
            Debug.Log($"NetworkManager->ConnectSession : {CrrServerInfo.ip}:{CrrServerInfo.port}");
#endif

            State = ConnectState.ConnectSession;

            // 뚝끊겨버림, 비동기 처리한다.
            System.Threading.Tasks.Task.Run(() => connectionAgent.Connect(CrrServerInfo.ip, CrrServerInfo.port));
        }

        private void OnConnect(ConnectionAgent connectionAgent, ResultCodeConnect result)
        {
            switch (result)
            {
                case ResultCodeConnect.CONNECT_SUCCESS:
                case ResultCodeConnect.CONNECT_ALREADY_CONNECTED:
                    {
                        Authenticate();
                    }
                    break;
                case ResultCodeConnect.CONNECT_FAIL:
                    {
                        Managers.UI.SetActiveConnectBlock(false);
                        State = ConnectState.None;
#if DevClient
                        Debug.LogError($"NetworkManager->OnConnect is Failed :: {result}");
#endif

                        Managers.UI.EnqueuePopup(
                            "알림",
                            "연결에 실패하였습니다.\n\n다시 시도하시겠습니까?",
                            () => AutoConnect(true),
                            WindowManager.PopupType.System);
                    }
                    break;
                default:
                    {
                        // 얌전히 기다린다.
                        Debug.Log($"NetworkManager->OnConnect :: {result}");
                    }
                    break;
            }
        }

        private void Authenticate()
        {

#if DevClient
            Debug.Log("NetworkManager->Authenticate");
#endif

            ENUM_OS osType = ENUM_OS.OsNone;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    osType = ENUM_OS.OsAndroid;
                    break;
                case RuntimePlatform.IPhonePlayer:
                    osType = ENUM_OS.OsIos;
                    break;
                case RuntimePlatform.WebGLPlayer:
                    osType = ENUM_OS.OsWeb;
                    break;
            }

            var currCultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            var regionInfo = new System.Globalization.RegionInfo(currCultureInfo.LCID);

            var req_Auth = new REQ_Auth()
            {
                Header = CreateHeader(),
                Auth = new ST_Auth()
                {
                    UserId = this.accountId,
                    AccessToken = "",
                    AccessTokenSecret = "",
                    RegDate = DateTime.Now.ToUnixEpochTime(),
                    LastLoginDate = DateTime.Now.ToUnixEpochTime()
                },
                Device = new ST_Device()
                {
                    Os = osType,
                    OsVersion = SystemInfo.operatingSystem,
                    ServiceLanguage = ENUM_Language.LanguageKo,
                    ServiceCountry = ENUM_Country.CountryKr,
                    Platform = ENUM_Platform.PlatformNone,
                    PlatformSdkVersion = "",
                    ServiceProvider = ENUM_ServiceProvider.ServiceProviderNone,
                    ClientIp = Utils.GetLocalAddressIPv4(),
                    AppVersion = Application.version,
                    Uuid = deviceId,
                    DeviceName = SystemInfo.deviceName,
                    Locale = currCultureInfo.Name,
                    LocaleLanguage = currCultureInfo.TwoLetterISOLanguageName,
                    LocaleCountryCode = regionInfo.TwoLetterISORegionName,
                    Gaid = "",
                    Idfa = ""
                }
            };

            var payload = NetProcess.CreatePayload(req_Auth);

            State = ConnectState.Auth;
            connectionAgent.Authenticate(this.deviceId, this.accountId, string.Empty, payload);
        }

        private void OnAuthenticate(
            ConnectionAgent connectionAgent, 
            ResultCodeAuth result, 
            List<ConnectionAgent.LoginedUserInfo> loginedUserInfoList, 
            string message, 
            Payload payload)
        {
            if (result == ResultCodeAuth.AUTH_SUCCESS
                || result == ResultCodeAuth.AUTH_FAIL_MAINTENANCE)
            {
                var res = payload.GetMessage<RES_Auth>();
                if(!NetProcess.CheckError(res.Header))
                {
                    this.userId = res.UserId;
                    this.authToken = res.AuthToken;
                    GetChannelList();
                    return;
                }
            }
            
            State = ConnectState.None;
            Managers.UI.SetActiveConnectBlock(false);
#if DevClient
            Debug.LogError($"NetworkManager->OnAuthenticate is Failed :: {result}");
#endif

            Managers.UI.EnqueuePopup(
                "알림",
                "인증에 실패하였습니다.\n\n다시 시도하시겠습니까?",
                () => AutoConnect(true),
                WindowManager.PopupType.System);
        }

        private void GetChannelList()
        {
            // 일단 채널 관련되서는 사용하지 않아본다.
            // 2020. 09. 23. 클라 김선재, 서버 김대규
            Login();
            return;

#if DevClient
            Debug.Log("NetworkManager->GetChannelList");
#endif

            State = ConnectState.GetChannelList;
            connectionAgent.GetChannelList(CrrServerInfo.serviceName);
        }

        private void OnGetChannelList(ConnectionAgent connectionAgent, ResultCodeChannelList result, List<string> channelIdList)
        {
            if (result == ResultCodeChannelList.CHANNEL_LIST_SUCCESS)
            {
                // Todo :: 혹시 채널 선택할 일이 있다면 UI 등에서 처리해줘야함
                if (channelIdList.CheckIndex(0))
                    selectedChannelId = channelIdList[0];
                else
                    selectedChannelId = string.Empty;

                GetChannelInfo();
                return;
            }
            
            State = ConnectState.None;
            Managers.UI.SetActiveConnectBlock(false);
#if DevClient
            Debug.LogError($"NetworkManager->OnGetChannelList is Failed :: {result}");
#endif

            Managers.UI.EnqueuePopup(
                "알림",
                "채널 리스트 획득에 실패하였습니다.\n\n다시 시도하시겠습니까?",
                () => AutoConnect(true),
                WindowManager.PopupType.System);
        }

        private void GetChannelInfo()
        {

#if DevClient
            Debug.Log("NetworkManager->GetChannelInfo");
#endif

            State = ConnectState.GetChannelInfo;
            connectionAgent.GetChannelInfo(CrrServerInfo.serviceName, selectedChannelId);
        }

        private void OnGetChannelInfo(ConnectionAgent connectionAgent, ResultCodeChannelInfo result, List<Payload> channelInfoList)
        {
            if (result == ResultCodeChannelInfo.CHANNEL_INFO_SUCCESS)
            {
                Login();
                return;
            }
           
            State = ConnectState.None;
            Managers.UI.SetActiveConnectBlock(false);
#if DevClient
            Debug.LogError($"NetworkManager->GetChannelInfo is Failed :: {result}");
#endif

            Managers.UI.EnqueuePopup(
                "알림",
                "채널 정보 획득에 실패하였습니다.\n\n다시 시도하시겠습니까?",
                () => AutoConnect(true),
                WindowManager.PopupType.System);
        }

        private void Login()
        {

#if DevClient
            Debug.Log("NetworkManager->Login");
#endif

            var req = new REQ_Login()
            {
                Header = CreateHeader(),
                Login = ENUM_Login.LoginGuest,
                UserId = userId,
                AuthToken = authToken,
            };

            var payload = NetProcess.CreatePayload(req);

            State = ConnectState.Login;
            userAgent = connector.GetUserAgent(CrrServerInfo.serviceName, CrrServerInfo.serviceSubId);
            if (userAgent == null)
            {
                userAgent = connector.CreateUserAgent(CrrServerInfo.serviceName, CrrServerInfo.serviceSubId);
                userAgent.onLoginListeners += OnLogin;
                NetProcess.AddListener(userAgent);

                userAgent.Login(this.userType, "", payload);
            }
        }

        private void OnLogin(UserAgent userAgent, ResultCodeLogin result, UserAgent.LoginInfo loginInfo)
        {
            if (result == ResultCodeLogin.LOGIN_SUCCESS
                || result == ResultCodeLogin.LOGIN_FAIL_MAINTENANCE)
            {
                var res_Login = loginInfo.Payload.GetMessage<RES_Login>();
                if (!NetProcess.CheckError(res_Login.Header))
                {
                    State = ConnectState.Done;
                    Managers.UI.SetActiveConnectBlock(false);

                    // 진행 중인 게임룸 정보, 없다면 NULL
                    var st_GameRoom = loginInfo.Payload.GetMessage<ST_GameRoom>();

                    NetProcess.Response_Login(res_Login, st_GameRoom);
                    return;
                }
            }

            State = ConnectState.None;
            Managers.UI.SetActiveConnectBlock(false);
#if DevClient
            Debug.LogError($"NetworkManager->OnLogin is Failed :: {result}");
#endif

            Managers.UI.EnqueuePopup(
                "알림",
                "로그인에 실패하였습니다.\n\n다시 시도하시겠습니까?",
                () => AutoConnect(true),
                WindowManager.PopupType.System);
        }

        private void OnDisconnected(ConnectionAgent connectionAgent, ResultCodeDisconnect result, bool force, Payload payload)
        {
            switch (result)
            {
                case ResultCodeDisconnect.SOCKET_DISCONNECT:
                    {

                    }
                    break;
                case ResultCodeDisconnect.SOCKET_ERROR:
                    {

                    }
                    break;
                case ResultCodeDisconnect.SOCKET_TIME_OUT:
                    {

                    }
                    break;
            }
        }

        private void OnError(ConnectionAgent connectionAgent, ErrorCode errorCode, Commands commands)
        {
            OnError(connectionAgent, errorCode, commands.ToString());
        }

        private void OnError(ConnectionAgent connectionAgent, ErrorCode errorCode, string command)
        {
            Managers.UI.EnqueuePopup(
                "알림",
#if DevClient
                $"ErrorCode : {errorCode}\nErrorCommands : {command}\n\n게임을 다시 시작합니다.",
#else
                $"ErrorCode : {(int)errorCode}\nErrorCommands : {commands\n\n게임을 다시 시작합니다.}",
#endif
                Managers.GameRestart,
                WindowManager.PopupType.System);
        }
    }
}
