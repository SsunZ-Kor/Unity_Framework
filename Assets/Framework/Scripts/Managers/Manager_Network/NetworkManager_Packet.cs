using BubbleFighter.Network.Protocol;
using GameAnvil;
using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public partial class NetworkManager
    {
        private struct PacketPair
        {
            public Packet packet;
            public Response resp;
        }
        private abstract class Response
        {
            public abstract void ExcuteCallback(Packet resp_Packet);
        }

        private class Response<T> : Response where T : IMessage
        {
            System.Action<T> _callback;

            public Response(System.Action<T> callback)
            {
                _callback = callback;
            }

            public override void ExcuteCallback(Packet resp_Packet)
            {
                if (_callback == null)
                    return;

                var resp = resp_Packet.GetMessage<T>();
                _callback.Invoke(resp);
            }
        }

        private ulong txId = 0;
        private bool IsRequesting = false;
        private Queue<PacketPair> _queue_PendingRequestMessage = new Queue<PacketPair>();
        private List<IMessage> _list_PendingSendMessage = new List<IMessage>();

        public REQ_Header CreateHeader()
        {
            return new REQ_Header()
            {
                TxId = txId++
            };
        }

        public void Send(IMessage send)
        {
            if (IsConnected)
            {
                userAgent.Send(send);
                return;
            }

            _list_PendingSendMessage.Add(send);

            // 이미 연결 시도 중인지 체크
            if (_list_PendingSendMessage.Count > 1)
                return;

            AutoConnect(false);
            StartCoroutine(WaitForConnect(() =>
            { 
                for (int i = 0; i < _list_PendingSendMessage.Count; ++i)
                    userAgent.Send(_list_PendingSendMessage[i]);

                _list_PendingSendMessage.Clear();
            }));
        }

        public bool SendByUDP(ENUM_UDP_MSGID msgID, IMessage send)
        {
            int targetCount = UserData.RoomInfo.GameRoom.Users.Count;
            int curCount = 0;
            for (int i = 0; i < UserData.RoomInfo.GameRoom.Users.Count; ++i)
            {
                var user = UserData.RoomInfo.GameRoom.Users[i];
                if (user.UserNo == UserData.AccountInfo.UserNo)
                {
                    curCount++;
                    continue;
                }

                if( user.UdpPort != 0 )
                {
                    //Debug.Log($"SendUDP :: {msgID}");
                    curCount++;
                    SendUDP(user.UdpAddress, user.UdpPort, msgID, send);
                }
            }

            if(curCount >= targetCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void HybridSend(ENUM_UDP_MSGID msgID, IMessage send)
        {
            // 먼저 UDP로 전송
            bool sendAll = SendByUDP(msgID, send);
            if(sendAll == true)
            {
                // UDP로 다 보냈으면 끝
                return;
            }
            
            Send(send);
        }

        public void Request<T>(IMessage req, System.Action<T> callback) where T : IMessage
        {
            PacketPair pair = new PacketPair();

            pair.packet = new Packet(req);
            if (callback != null)
                pair.resp = new Response<T>(callback);

            _queue_PendingRequestMessage.Enqueue(pair);

            if (!IsRequesting)
                CheckAndRequest();
        }

        private void CheckAndRequest()
        {
            // 보류 중인 패킷이 체크
            if (_queue_PendingRequestMessage.Count == 0)
            {
                IsRequesting = false;
                Managers.UI.SetActiveNetBlock(false);
                return;
            }

            if (IsRequesting == false)
            {
                IsRequesting = true;
                Managers.UI.SetActiveNetBlock(true);
            }

            // 연결 상태 체크
            var bConnected = IsConnected;
            if (!bConnected)
            {
                AutoConnect(false);
                StartCoroutine(WaitForConnect(CheckAndRequest));
                return;
            }

            // Callback에 따라 Send, Request 분기 하여 처리
            var pair =  _queue_PendingRequestMessage.Dequeue();
            if (pair.resp != null)
            {
                userAgent.Request(pair.packet, (agent, resp_Packet) =>
                {
                    pair.resp.ExcuteCallback(resp_Packet);
                    CheckAndRequest();
                });
            }
            else
            {
                userAgent.Send(pair.packet);
                CheckAndRequest();
            }
        }

        private IEnumerator WaitForConnect(System.Action callback)
        {
            if (callback == null)
                yield break;

            yield return new WaitUntil(() => this.State == ConnectState.Done);

            callback.Invoke();
        }
    }
}