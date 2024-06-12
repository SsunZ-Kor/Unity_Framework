using BubbleFighter.Network.Protocol;
using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Game
{
    public partial class NetworkManager
    {
        public FrameSyncUDPStream UdpStream { get; private set; }

        public void ConnectUDPStream(ST_GameRoom roomInfo, bool test)
        {
            UdpStream = new FrameSyncUDPStream();
            NetProcess.AddUDPReciver(UdpStream);
            UdpStream.Start(test);
        }

        public void DisconnectUDPStream()
        {
            if (UdpStream != null)
            {
                NetProcess.RemoveUDPReciver(UdpStream);
                UdpStream = null;
            }
        }

        public void SendUDP(string address, int port, ENUM_UDP_MSGID msgID, IMessage msg)
        {
            UdpStream.SendTo(address, port, msgID, msg);
        }
        
        public void udpParse()
        {
            if(UdpStream == null )
            {
                return;
            }

            UdpStream.parse();
        }
    }
}
