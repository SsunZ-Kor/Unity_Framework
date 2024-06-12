using BubbleFighter.Network.Protocol;
using GameAnvil;
using GameAnvil.User;
using Google.Protobuf;
using Google.Protobuf.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static partial class NetProcess
    {
        public static void AddListener(UserAgent user)
        {
            if (user == null)
            {
                Debug.LogError("NetworkProcess->AddListener :: UserAgent Is Null");
                return;
            }
#if UNITY_EDITOR
            Debug.Log("NetProcess->AddListener");
#endif
            user.AddListener<NOTIFY_JoinGameRoom>(Notify_JoinGameRoom);
            user.AddListener<NOTIFY_ReadyGameRoom>(Notify_ReadyGameRoom);
            user.AddListener<NOTIFY_LeaveGameRoom>(Notify_LeaveGameRoom);
            user.AddListener<NOTIFY_StartLoadingRoom>(Notify_StartLoadingRoom);
            user.AddListener<NOTIFY_StartGameRoom>(Notify_StartGameRoom);
            user.AddListener<NOTIFY_FinishGameRoom>(Notify_FinishGameRoom);

            user.AddListener<NOTIFY_Battle_Move>(Notify_BattleMove);
            user.AddListener<NOTIFY_Battle_Action>(Notify_BattleAction);
            user.AddListener<NOTIFY_BattleHitEffect>(Notify_BattleHitEffect);
            user.AddListener<NOTIFY_Battle_Projectile_Generate>(Notify_BattleGenerateProj);
            user.AddListener<NOTIFY_Battle_Projectile_Release>(Notify_BattleReleaseProj);

            user.AddListener<NOTIFY_UDPInfo>(Notify_UDPInfo); //ALEX work 2020.10.21
        }

        public static void RemoveListener(UserAgent user)
        {
            if (user == null)
            {
                Debug.LogError("NetProcess->RemoveListener :: UserAgent Is Null");
                return;
            }

#if UNITY_EDITOR
            Debug.Log("NetProcess->RemoveListener");
#endif
            user.RemoveAllListeners();
        }

        public static void AddUDPReciver(FrameSyncUDPStream udpStream)
        {
            if (udpStream == null)
            {
                Debug.LogError("NetworkProcess->AddUDPReciver :: udpStream Is Null");
                return;
            }
#if UNITY_EDITOR
            Debug.Log("NetProcess->AddUDPReciver");
#endif
            udpStream.AddReciver<UDP_Battle_Move>(ENUM_UDP_MSGID.MsgidBattleMove, UDP_Recive_BattleMove);
            udpStream.AddReciver<UDP_Battle_Action>(ENUM_UDP_MSGID.MsgidBattleAction, UDP_Recive_BattleAction);
            udpStream.AddReciver<UDP_BattleHitEffect>(ENUM_UDP_MSGID.MsgidDamageEffect, UDP_Recive_BattleHitEffect);
            udpStream.AddReciver<UDP_Battle_Projectile_Generate>(ENUM_UDP_MSGID.MsgidBattleProjectileGenerate, UDP_Recive_BattleGenerateProj);
            udpStream.AddReciver<UDP_Battle_Projectile_Release>(ENUM_UDP_MSGID.MsgidBattleProjectileRelease, UDP_Recive_BattleReleaseProj);

            udpStream.AddReciver<UDP_PING>(ENUM_UDP_MSGID.MsgidPing, UDP_Receive_Ping);
            udpStream.AddReciver<UDP_PONG>(ENUM_UDP_MSGID.MsgidPong, UDP_Receive_Pong);
        }

        public static void RemoveUDPReciver(FrameSyncUDPStream udpStream)
        {
            if (udpStream == null)
            {
                Debug.LogError("NetProcess->RemoveUDPReciver :: udpStream Is Null");
                return;
            }

#if UNITY_EDITOR
            Debug.Log("NetProcess->RemoveUDPReciver");
#endif
            udpStream.RemoveAllReciver();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <returns>에러 상황일 경우 True</returns>
        public static bool CheckError(RES_Header header)
        {
            // Todo :: 일단 헤더 없이도 작동하도록 수정
            // 2020. 10. 05. 클라 김선재, 서버 김대규
            if (header == null)
                //return true;
                return false; 

            if (header.ErrorCode == ENUM_ErrorCode.ErrSuccess)
            {
                return false;
            }

            Managers.UI.EnqueuePopup(
                "알림",
                $"ServerError\n\nCODE : {header.ErrorCode}",
                Managers.GameRestart,
                WindowManager.PopupType.System
                );


            return true;
        }

        public static Payload CreatePayload(IMessage message)
        {
            var packet = new Packet(message);
            var payload = new Payload();
            payload.add(packet);
            return payload;
        }
    }
}
