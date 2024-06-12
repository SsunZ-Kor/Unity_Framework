using BubbleFighter.Network.Protocol;
using Game;
using Google.Protobuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class FrameSyncUDPStream
{
    private SortedDictionary<ENUM_UDP_MSGID, Action<IMessage>> m_dic_Reciver = new SortedDictionary<ENUM_UDP_MSGID, Action<IMessage>>();
    private SortedDictionary<ENUM_UDP_MSGID, MessageParser> m_dic_Parser = new SortedDictionary<ENUM_UDP_MSGID, MessageParser>();

    Socket m_socket = null;
    EndPoint m_any = null;
    //string strServerAddr = "10.77.11.41";
    //int serverUdpPort = 11390;
    private const int bufSize = 8 * 1024;
    private State state = new State();
    private AsyncCallback recv = null;
    //private Queue<byte[]> packetQueue;

    public CircularBuffer circularBuffer = new CircularBuffer(8192);
    private static object bufferLock = new object();
    public byte[] tempBuffer = new byte[512];

    public string myAddress = "";
    public int myPort = 0;

    public class State
    {
        public byte[] buffer = new byte[bufSize];
    }

    public void Start(bool test)
    {
        m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        m_any = new IPEndPoint(IPAddress.Any, 0);
        circularBuffer.Reset();
        Receive();
        UdpServerLogin(test);
    }

    public void UdpServerLogin(bool testValue)
    {
        var req_UdpLogin = new UDP_LOGIN()
        {
            UserNo = UserData.AccountInfo.UserNo,
            Address = "",
            Port = 0,
            Test = testValue,
        };
        //Debug.Log($"sendUdpLogin :: {msgId}");
        SendTo(Managers.Net.CrrServerInfo.ip, Managers.Net.CrrServerInfo.udp_port, ENUM_UDP_MSGID.MsgidLogin, req_UdpLogin);
    }

    public void SendTo(string address, int port, ENUM_UDP_MSGID msgID, IMessage msg)
    {
        byte[] data = msg.ToByteArray();
        byte[] req = new byte[data.Length + 4];

        System.Buffer.BlockCopy(BitConverter.GetBytes((short)data.Length), 0, req, 0, 2);
        System.Buffer.BlockCopy(BitConverter.GetBytes((short)msgID), 0, req, 2, 2);
        System.Buffer.BlockCopy(data, 0, req, 4, data.Length);

        Debug.Log($"UDP_Send :: {req.Length}, TO :: {address}, {port}, {msgID}");
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse(address), port);
        m_socket.SendTo(req, req.Length, SocketFlags.None, ep);
    }

    /*
    public void Send(short msgId, IMessage msg)
    {
        byte[] data = msg.ToByteArray();
        byte[] req = new byte[data.Length + 2];

        System.Buffer.BlockCopy(BitConverter.GetBytes(msgId), 0, req, 0, 2);
        System.Buffer.BlockCopy(data, 0, req, 2, data.Length);
        Send(req);
    }

    public void Send(byte[] data)
    {
        m_socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
        {
            State so = (State)ar.AsyncState;
            int bytes = m_socket.EndSend(ar);
            Debug.Log($"UDP_Send :: {data.Length}, {data}");
        }, state);
    }
    */

    private void Receive()
    {
        m_socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref m_any, recv = (ar) =>
        {
            State so = (State)ar.AsyncState;
            int bytes = m_socket.EndReceiveFrom(ar, ref m_any);
            Debug.Log($"UDP_RECV :: {bytes} , FROM :: {m_any}");

            lock (bufferLock)
            {
                circularBuffer.PutData(state.buffer, bytes);
            }

            m_socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref m_any, recv, so);
        }, state);
    }

    public void parse()
    {
        lock (bufferLock)
        {
            int receivedData = circularBuffer.GetValidCount();
            int minValue = 4;
            if (receivedData > minValue)
            {

            }
            else
            {
                return;
            }
        }

        while (true)
        {
            bool bGetCompletedPacket = false;
            int packetLength = 0;
            int packetNo = 0;
            Array.Clear(tempBuffer, 0, tempBuffer.Length);//ZeroMemory

            lock (bufferLock)
            {
                bGetCompletedPacket = circularBuffer.GetCompletedPacket(tempBuffer, ref packetLength, ref packetNo);
            }

            if (bGetCompletedPacket == true)
            {
                parse((short)packetNo, (short)packetLength);
            }
            else
            {
                break;
            }
        }
    }

    private void parse(short msgId, short length)
    {
        try
        {
            if (msgId == (short)ENUM_UDP_MSGID.MsgidLogin)
            {
                UDP_LOGIN data = UDP_LOGIN.Parser.ParseFrom(tempBuffer, 0, length);
                myAddress = data.Address;
                myPort = data.Port;

                if (data.Test == false)
                {
                    // 룸에 등록 시도
                    var send_packet = new SEND_UDPInfo()
                    {
                        Header = Managers.Net.CreateHeader(),
                        UserNo = UserData.AccountInfo.UserNo,
                        Address = data.Address,
                        Port = data.Port,
                    };

                    Debug.Log($"SEND_UDPInfo :: {data.Address} , {data.Port}");
                    Managers.Net.Send(send_packet);
                }
                else
                {
                    // 서버에서 udp 테스터로 등록된 상황
                    // 일단 암것도 안함.

                }
            }
            else
            {
                // 리시버에 등록된 콜백 실행
                var nMsgId = (int)msgId;
                var eMsgId = (ENUM_UDP_MSGID)nMsgId;

                var reciver = m_dic_Reciver.GetOrNull(eMsgId);
                if (reciver == null)
                {
                    Debug.LogError($"Revice {eMsgId}, but Not Found Reciver");
                    return;
                }

                var parser = m_dic_Parser.GetOrNull(eMsgId);
                if (parser == null)
                {
                    Debug.LogError($"Revice {eMsgId}, but Not Found Parser");
                    return;
                }

                var msg = parser.ParseFrom(tempBuffer, 0, length);
                reciver.Invoke(msg);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ERROR_UDPparse :: {e}");
        }
    }
    /*
    public void parse(byte[] buffer)
    {
        try
        {
            short length = BitConverter.ToInt16(buffer, 0);
            short msgId = BitConverter.ToInt16(buffer, 2);
            if (msgId == (short)ENUM_UDP_MSGID.MsgidLogin)
            {
                UDP_LOGIN data = UDP_LOGIN.Parser.ParseFrom(buffer, 4, length);
                var send_packet = new SEND_UDPInfo()
                {
                    Header = Managers.Net.CreateHeader(),
                    UserNo = UserData.AccountInfo.UserNo,
                    Address = data.Address,
                    Port = data.Port,
                };

                Debug.Log($"SEND_UDPInfo :: {data.Address} , {data.Port}");
                Managers.Net.Send(send_packet);
            }
            else
            {
                // 리시버에 등록된 콜백 실행
                var nMsgId = (int)msgId;
                var eMsgId = (ENUM_UDP_MSGID)nMsgId;

                var reciver = m_dic_Reciver.GetOrNull(eMsgId);
                if (reciver == null)
                {
                    Debug.LogError($"Revice {eMsgId}, but Not Found Reciver");
                    return;
                }

                var parser = m_dic_Parser.GetOrNull(eMsgId);
                if (parser == null)
                {
                    Debug.LogError($"Revice {eMsgId}, but Not Found Parser");
                    return;
                }

                var msg = parser.ParseFrom(state.buffer, 4, length);
                reciver.Invoke(msg);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ERROR_UDPparse :: {e}");
        }
    }
    */

    public void AddReciver<T>(ENUM_UDP_MSGID msgId, System.Action<T> reciver) where T : class, IMessage<T>, new()
    {
        m_dic_Parser.Add(msgId, new MessageParser<T>(() => new T()));
        m_dic_Reciver.Add(msgId, (msg) => reciver(msg as T));
    }

    public void RemoveAllReciver()
    {
        m_dic_Reciver.Clear();
        m_dic_Parser.Clear();
        circularBuffer.Reset();
    }
}