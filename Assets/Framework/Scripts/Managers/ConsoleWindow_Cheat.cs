using BubbleFighter.Network.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public partial class ConsoleWindow
    {
        [Header("Cheat Buttons")]
        [SerializeField]
        private ButtonEx _btn_UDPLogin = null;
        [SerializeField]
        private ButtonEx _btn_UDPNetworkTest = null;

        private void OnAwake_Cheat()
        {
            _btn_UDPLogin.onClick.Subscribe(OnClick_UDPLogin);
            _btn_UDPNetworkTest.onClick.Subscribe(OnClick_UDPNetworkTest);
        }

        private void OnClick_UDPLogin()
        {
            Debug.Log("UDP Test Login");
            Managers.Net.ConnectUDPStream(null, true);
        }

        private void OnClick_UDPNetworkTest()
        {
            Debug.Log("UDP Network Test");
            var req = new REQ_UDP_LIST()
            {
                Header = Managers.Net.CreateHeader(),
            };

            Managers.Net.Request<RES_UDP_LIST>(req, (res) =>
            {
                Debug.Log("UDP Network Test Received");
                var req_ping = new UDP_PING()
                {
                    Info = UserData.AccountInfo.NickName,
                    Address = Managers.Net.UdpStream.myAddress,
                    Port = Managers.Net.UdpStream.myPort,
                };

                foreach (ST_UDP_INFO data in res.List)
                {
                    if (data.UserNo != UserData.AccountInfo.UserNo)
                    {
                        Debug.Log($"UDP Network Test : {data.UserNo}:{data.UserName}:{data.Address}:{data.Port}");
                        Managers.Net.SendUDP(data.Address, data.Port, ENUM_UDP_MSGID.MsgidPing, req_ping);
                    }
                }
            });
        }
    }
}
