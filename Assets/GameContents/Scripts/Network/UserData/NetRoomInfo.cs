using BubbleFighter.Network.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{

    public class NetRoomInfo
    {
        public ST_GameRoom GameRoom { get; private set; } = null;

        public void SetGameRoom(ST_GameRoom gameRoom)
        {
            GameRoom = gameRoom;
        }

        public void GameUserReady(long userNo)
        {
            if (GameRoom == null)
                return;

            ST_GameRoomUser userInfo = null;
            for (int i = 0; i < GameRoom.Users.Count; ++i)
            {
                var user = GameRoom.Users[i];
                if (user.UserNo != userNo)
                    continue;

                userInfo = user;
                break;
            }

            userInfo.UserState = ENUM_USER_STATE.UserStateReady;
        }
    }
}