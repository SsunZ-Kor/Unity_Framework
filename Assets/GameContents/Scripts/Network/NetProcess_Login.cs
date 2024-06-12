using BubbleFighter.Network.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static partial class NetProcess
    {
        public static void Response_Login(RES_Login res, ST_GameRoom gameRoom)
        {
            if (CheckError(res.Header))
                return;

            var newAccountInfo = new NetAccountInfo();

            newAccountInfo.NickName = res.Nickname;
            newAccountInfo.UserNo = res.UserNo;

            UserData.SetAccountInfo(newAccountInfo);

            // 현재 진행중인 게임 Room 정보, 게임 진행중에 튕겼을때 유효하다. 아니라면 NULL
            if (gameRoom != null)
                UserData.RoomInfo.SetGameRoom(gameRoom);
        }

        public static void Request_EnterLobby(System.Action callback)
        {
            var req_EnterLobby = new REQ_EnterLobby()
            {
                Header = Managers.Net.CreateHeader(),
            };

            Managers.Net.Request<RES_EnterLobby>(req_EnterLobby, (res) =>
            {
                if (CheckError(res.Header))
                    return;

                callback?.Invoke();
            });
        }

        public static void Request_NickName(string nickName, System.Action callback)
        {
            var req_ConfigNickname = new REQ_ConfigNickname()
            {
                Header = Managers.Net.CreateHeader(),
                Nickname = nickName,
            };

            Managers.Net.Request<RES_ConfigNickname>(req_ConfigNickname, (res) =>
            {
                if (CheckError(res.Header))
                    return;

                UserData.AccountInfo.NickName = res.Nickname;

                callback?.Invoke();
            });
        }
    }
}
