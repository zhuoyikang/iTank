using System.IO;
using System;
using UnityEngine;
//using proto.packet;
using System.Net.Sockets;
using System.Threading;
using MsgPack.Serialization;

public enum SocketState {
    Closed = 1,
    Connected = 2,
    TryConnected = 3
};


public class NetSocket
{
    const int BUFF_SIZE =  65535;
    const int HEAD_SIZE =  4;

    private Socket clientSocket;
    private readonly NetByteBuf buf = new NetByteBuf(BUFF_SIZE);
    private SocketState status = SocketState.Closed;


    // singleton
    private volatile static NetSocket _instance;
    private static readonly object lockHelper = new object();

    public NetSocket() {
        _instance = null;
    }

    public static NetSocket Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (lockHelper)
                {
                    if (_instance == null)
                    {
                        _instance = new NetSocket();
                    }
                }
            }
            return _instance;
        }
    }


    // 将数据打包成2进制
    public void Send<T>(string eventName, T message)  {
        var serializer = MessagePackSerializer.Get<T> ();
        var ms1 = new MemoryStream ();
        serializer.Pack (ms1, message);

        if (status == SocketState.Closed) {
            return;
        }

        byte[] data = ms1.ToArray ();
        var netByteBuf = new NetByteBuf (data.Length + 4);
        netByteBuf.WriteShort ((short)(data.Length + 1));
        byte cmd = (byte)EventName.GetEventCmd (eventName);

        netByteBuf.WriteByte (cmd);
        netByteBuf.WriteBytes (data);

        byte[] sendData = netByteBuf.GetRaw ();
        Debug.Log("Send:" + data.Length +"_"+ sendData.Length);

        clientSocket.BeginSend (sendData,
                                0,
                                sendData.Length,
                                SocketFlags.None,
                                null, clientSocket);
    }


    public void Connect(string ip, int port) {

        if(status != SocketState.Closed) {
            return;
        }

        status = SocketState.TryConnected;
        clientSocket = new Socket(AddressFamily.InterNetwork,
                                  SocketType.Stream,
                                  ProtocolType.Tcp);

        clientSocket.NoDelay = true;
        var linger = new LingerOption(false, 0);
        clientSocket.LingerState = linger;
        clientSocket.BeginConnect(ip, port, connected_cb, this);
    }

    //连接成功回调
    private void connected_cb(IAsyncResult ar) {

        clientSocket.EndConnect(ar);
        if (!clientSocket.Connected){
            return;
        }

        status = SocketState.Connected;

        var threadSocket = new Thread(new ThreadStart(waitSocket));
        threadSocket.IsBackground = true;
        threadSocket.Start();

        var threadProcess = new Thread(new ThreadStart(waitProcess));
        threadProcess.IsBackground = true;
        threadProcess.Start();

    }

    private void waitProcess() {
        while (true) {
            NetEvent.ProcessIn();
            Thread.Sleep(100);
        }
    }

    // 异步收包线程.
    private void waitSocket() {
        Debug.Log("receiver");
        int len;

        while(true)
        {
            if (!clientSocket.Poll(-1, SelectMode.SelectRead)){
                Debug.Log("poll error");
                Close();
            }

            len = clientSocket.Receive(buf.GetRaw(), 0, HEAD_SIZE,
                                       SocketFlags.None);

            if(len<0) {
                Debug.Log("length error");
                Close();
                return;
            }

            Debug.Assert(len == HEAD_SIZE);
            int payload_length = buf.GetInt(0);

            Debug.Assert(payload_length < BUFF_SIZE);
            int want = payload_length;
            int lenAll= 0 ;
            SocketError socketError;

            while (want>0) {
                len = clientSocket.Receive(buf.GetRaw(), lenAll, want,
                                           SocketFlags.None, out socketError);
                want -= len;
                lenAll += len;
            }

            Debug.Log("receive length:" + lenAll+ " payload_length："
                      + payload_length);

            Debug.Assert(payload_length == lenAll);
            var ms2 = new MemoryStream (buf.GetRaw (), 0, payload_length);
            int cmd = ms2.ReadByte ();
            string eventName = EventName.GetEventName (cmd);
            NetEvent.FireOut(eventName, new object[]{ ms2});


            /*
              var protoPacket = ProtoBuf.Serializer.Deserialize<packet>(ms2);
              Debug.Log("receive packet.cmd:"+protoPacket.cmd);
              var eventname = MessageMap.GetEventName(protoPacket.cmd);
              Event.FireOut(eventname, new object[]{protoPacket.payload});
            */

        }
    }


    // 关闭Socket
    public void Close() {
        if (clientSocket != null && clientSocket.Connected) {
            clientSocket.Shutdown(SocketShutdown.Both);
        }
        clientSocket.Close();
    }

};
