using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class UserData
    {
        public static NetAccountInfo AccountInfo { get; private set; }
        public static NetRoomInfo RoomInfo { get; private set; } = new NetRoomInfo();
        public static NetTimeInfo TimeInfo { get; private set; }


        public static void SetAccountInfo(NetAccountInfo accountInfo)
        {
            AccountInfo = accountInfo;
        }

        public static void SetTimeInfo(long serverTimeMillis)
        {
            TimeInfo = new NetTimeInfo();
            TimeInfo.SetServerTime(serverTimeMillis);
        }

        public static void Clear()
        {
            AccountInfo = null;
            TimeInfo = null;
            RoomInfo = new NetRoomInfo();
        }
    }
}