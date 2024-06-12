using BubbleFighter.Network.Protocol;
using Game;
using GameAnvil;
using GameAnvil.Defines;
using GameAnvil.User;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Game
{
    public static partial class NetProcess
    {
        /// <summary>
        /// 생성 된 방 리스트 요청
        /// </summary>
        public static void Request_GameRoomList()
        {
            var req_RoomList = new REQ_RoomList()
            {
                Header = Managers.Net.CreateHeader(),
            };

            Managers.Net.Request<RES_RoomList>(req_RoomList, (res) =>
            {
                if (CheckError(res.Header))
                    return;

                var list_RoomList = res.RoomList.ToList();

                var wnd_RoomMain = Managers.UI.GetWindow(WindowID.Window_RoomList, false) as Window_RoomList;
                if (wnd_RoomMain == null)
                    wnd_RoomMain = Managers.UI.OpenWindow(WindowID.Window_RoomList) as Window_RoomList;

                wnd_RoomMain.SetRoomList(list_RoomList);
            });
        }

        /// <summary>
        /// 방 생성하기
        /// </summary>
        /// <param name="createRoomName"></param>
        public static void Request_CreateGameRoom(string createRoomName)
        {
            var req_CreateGameRoom = new REQ_CreateGameRoom()
            {
                Header = Managers.Net.CreateHeader(),
                RoomName = createRoomName,
            };

            // UserAgent 기능 사용, 예외처리 한다.
            var req_Payload = CreatePayload(req_CreateGameRoom);

            Managers.UI.SetActiveNetBlock(true);
            Managers.Net.userAgent?.CreateRoom("ROOM_BF1", req_Payload, (userAgent, result, roomId, roomName, payload) =>
            {
                Managers.UI.SetActiveNetBlock(false);

                // 결과 에러처리
                switch (result)
                {
                    case GameAnvil.Defines.ResultCodeCreateRoom.CREATE_ROOM_SUCCESS:
                        break;
                    default:
                        Managers.UI.EnqueuePopup("알림", "방 생성에 실패하셨습니다.");
                        return;
                }

                var res = payload.GetMessage<RES_CreateGameRoom>();
                if (CheckError(res.Header))
                    return;

                // 룸 정보 세팅
                UserData.RoomInfo.SetGameRoom(res.Room);

                // 방 진입
                var wnd_Room = Managers.UI.OpenWindow(WindowID.Window_Room) as Window_Room;
                wnd_Room.RefreshGameRoom();

                // 방정보를 토대로 UDP 스트림 생성
                Managers.Net.ConnectUDPStream(res.Room, false);
            });
        }

        /// <summary>
        /// 방 참가하기
        /// </summary>
        /// <param name="joinRoomId"></param>
        public static void Request_JoinGameRoom(int joinRoomId)
        {
            var req_JoinGameRoom = new REQ_JoinGameRoom()
            {
                Header = Managers.Net.CreateHeader(),
                RoomId = joinRoomId,
            };

            // UserAgent 기능 사용
            var req_Payload = CreatePayload(req_JoinGameRoom);

            Managers.UI.SetActiveNetBlock(true);
            Managers.Net.userAgent?.JoinRoom("ROOM_BF1", joinRoomId, req_Payload, (userAgent, result, roomId, roomName, payload) =>
            {
                Managers.UI.SetActiveNetBlock(false);

                // 결과 예외처리
                switch (result)
                {
                    case GameAnvil.Defines.ResultCodeJoinRoom.JOIN_ROOM_SUCCESS:
                        break;
                    case GameAnvil.Defines.ResultCodeJoinRoom.JOIN_ROOM_FAIL_ROOM_DOES_NOT_EXIST:
                        Managers.UI.EnqueuePopup("알림", "방 정보가 존재하지 않습니다.");
                        return;
                    default:
                        Managers.UI.EnqueuePopup("알림", "방 입장에 실패하였습니다.");
                        return;
                }

                var res = payload.GetMessage<RES_JoinGameRoom>();
                if (CheckError(res.Header))
                    return;

                // 룸 정보 세팅
                UserData.RoomInfo.SetGameRoom(res.Room);

                // 방 진입
                var wnd_Room = Managers.UI.OpenWindow(WindowID.Window_Room) as Window_Room;
                wnd_Room.RefreshGameRoom();

                // 방정보를 토대로 UDP 스트림 생성
                Managers.Net.ConnectUDPStream(res.Room, false);
            });
        }

        /// <summary>
        /// 준비 완료
        /// </summary>
        public static void Request_ReadyGameRoom()
        {
            var req_ReadyGameRoom = new REQ_ReadyGameRoom()
            {
                Header = Managers.Net.CreateHeader(),
            };

            Managers.Net.Request<RES_ReadyGameRoom>(req_ReadyGameRoom, (res) =>
            {
                if (CheckError(res.Header))
                    return;

                // 룸 정보 세팅
                UserData.RoomInfo.GameUserReady(UserData.AccountInfo.UserNo);

                // UI는 버튼 누르는 즉시 갱신된다.
                // Todo : 나중에 서버에서 리퀘스트 받을때 처리하는 것으로 고치자
                // 2020. 10. 05. 클라 김선재
            });
        }

        /// <summary>
        /// 방에서 나가기 요청
        /// </summary>
        /// <param name="callback"></param>
        public static void Request_LeaveGameRoom(System.Action callback)
        {
            var req_LeaveGameRoom = new REQ_LeaveGameRoom()
            {
                Header = Managers.Net.CreateHeader(),
            };

            // UserAgent 기능 사용
            var req_Payload = CreatePayload(req_LeaveGameRoom);

            Managers.UI.SetActiveNetBlock(true);
            Managers.Net.userAgent?.LeaveRoom(req_Payload, (UserAgent userAgent, ResultCodeLeaveRoom result, bool force, int roomId, Payload payload) =>
            {
                Managers.UI.SetActiveNetBlock(false);

                // 결과 예외처리
                switch (result)
                {
                    case ResultCodeLeaveRoom.LEAVE_ROOM_SUCCESS:
                        break;
                    default:
                        Managers.UI.EnqueuePopup("알림", "방 나가기를 실패하였습니다.");
                        return;
                }

                var res = payload.GetMessage<RES_LeaveGameRoom>();
                if (CheckError(res.Header))
                    return;

                UserData.RoomInfo.SetGameRoom(null);
                callback?.Invoke();

                Managers.Net.DisconnectUDPStream();
            });
        }

        /// <summary>
        /// 게임 시작
        /// </summary>
        public static void Send_StartGameRoom()
        {
            var send_StartGameRoom = new SEND_StartGameRoom()
            {
                Header = Managers.Net.CreateHeader(),
            };

            Managers.Net.Send(send_StartGameRoom);
        }

        /// <summary>
        /// 타 유저 참가
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="notify"></param>
        private static void Notify_JoinGameRoom(UserAgent agent, NOTIFY_JoinGameRoom notify)
        {
            if (CheckError(notify.Header))
                return;

            UserData.RoomInfo.SetGameRoom(notify.Room);

            // UI 갱신
            var wnd_Room = Managers.UI.GetWindow(WindowID.Window_Room, false) as Window_Room;
            if (wnd_Room != null)
                wnd_Room.RefreshGameRoom();

            if (BattleSceneControllerBase.Instance != null)
                BattleSceneControllerBase.Instance.JoinGameRoomUser(notify.JoinUserNo);
        }

        /// <summary>
        /// 타 유저 레디
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="notify"></param>
        private static void Notify_ReadyGameRoom(UserAgent agent, NOTIFY_ReadyGameRoom notify)
        {
            if (CheckError(notify.Header))
                return;

            UserData.RoomInfo.SetGameRoom(notify.Room);

            // UI 갱신
            var wnd_Room = Managers.UI.GetWindow(WindowID.Window_Room, false) as Window_Room;
            if (wnd_Room != null)
                wnd_Room.RefreshGameRoom();
        }

        private static void Notify_LeaveGameRoom(UserAgent agent, NOTIFY_LeaveGameRoom notify)
        {
            if (CheckError(notify.Header))
                return;

            UserData.RoomInfo.SetGameRoom(notify.Room);

            // UI 갱신
            var wnd_Room = Managers.UI.GetWindow(WindowID.Window_Room, false) as Window_Room;
            if (wnd_Room != null)
                wnd_Room.RefreshGameRoom();

            if (BattleSceneControllerBase.Instance != null)
                BattleSceneControllerBase.Instance.LeaveGameRoomUser(notify.UserNo);
        }

        /// <summary>
        /// 서버가 스타트 하라고 알려줌
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="notify"></param>
        private static void Notify_StartLoadingRoom(UserAgent agent, NOTIFY_StartLoadingRoom notify)
        {
            if (CheckError(notify.Header))
                return;

            UserData.SetTimeInfo(notify.ServerTime);

            Managers.Scene.LoadScene(SceneID.Battle, LoadingID.Loading_ScreenShotFadeOut);
        }

        public static void Send_LoadingFinished()
        {
            var send_LoadingFinished = new SEND_LoadingFinished()
            {
                Header = Managers.Net.CreateHeader(),
            };

            Managers.Net.Send(send_LoadingFinished);
        }

        private static void Notify_StartGameRoom(UserAgent agent, NOTIFY_StartGameRoom notify)
        {
            if (BattleSceneControllerBase.Instance == null)
                return;

            if (CheckError(notify.Header))
                return;

            BattleSceneControllerBase.Instance.StartByServer(notify.Room.BattleStartTime, notify.Room.BattleFinishTime);
        }

        private static void Notify_FinishGameRoom(UserAgent agent, NOTIFY_FinishGameRoom notify)
        {
            if (BattleSceneControllerBase.Instance == null)
                return;

            if (CheckError(notify.Header))
                return;

            BattleSceneControllerBase.Instance.FinishByServer();

            Managers.Net.DisconnectUDPStream();
        }

        private static void Notify_UDPInfo(UserAgent agent, NOTIFY_UDPInfo notify)
        {
            //if (BattleSceneControllerBase.Instance == null)
            //    return;

            if (CheckError(notify.Header))
                return;

            UserData.RoomInfo.SetGameRoom(notify.Room);
        }
        
    }
}
