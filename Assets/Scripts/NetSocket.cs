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
    const int HEAD_SIZE =  2;

    private Socket clientSocket;
    private int sendId;
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

        if (status != SocketState.Connected) {
            return;
        }

        byte[] data = ms1.ToArray ();
        var netByteBuf = new NetByteBuf(data.Length + 2);
        netByteBuf.WriteShort ((short)(data.Length + 1));
        byte cmd = (byte)EventName.GetEventCmd (eventName);

        Debug.Log("begin send:" + eventName + " cmd " + cmd);

        netByteBuf.WriteByte (cmd);
        netByteBuf.WriteBytes (data);

        byte[] sendData = netByteBuf.GetRaw ();
        sendId+=1;

        Debug.Log("begin send:" + sendId);
        clientSocket.BeginSend (sendData,
                                0,
                                sendData.Length,
                                SocketFlags.None,
                                new AsyncCallback(send_cb), sendId);

        // clientSocket.Send(sendData, sendData.Length, SocketFlags.None);

    }

    public void send_cb(IAsyncResult re) {
        int sendId = (int)re.AsyncState;
        Debug.Log("end send:" + sendId);
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

        Debug.Log("connected_cb ");

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
            int payload_length = buf.GetShort(0);
            //Debug.Log ("payload_length" + payload_length);

            Debug.Assert(payload_length < BUFF_SIZE);

            int want = payload_length;
            int lenAll = 0 ;
            buf.Clear();

            SocketError socketError;

            while (want>0) {
                len = clientSocket.Receive(buf.GetRaw(), lenAll, want,
                                           SocketFlags.None, out socketError);
                want -= len;
                lenAll += len;
            }


            // Debug.Log("receive length:" + lenAll+ " payload_length："
            //           + payload_length);


            Debug.Assert(payload_length == lenAll);
            var ms_byte = new byte[payload_length];
            Array.Copy(buf.GetRaw(), 0, ms_byte, 0,  payload_length);
            var ms2 = new MemoryStream(ms_byte, 0, payload_length);
            int cmd = ms2.ReadByte ();
            string eventName = EventName.GetEventName (cmd);
            // ShowBytes("bytes:"+eventName+":", buf.GetRaw(), payload_length);
            NetEvent.FireOut(eventName, new object[]{ ms2});

            /*
              var protoPacket = ProtoBuf.Serializer.Deserialize<packet>(ms2);
              Debug.Log("receive packet.cmd:"+protoPacket.cmd);
              var eventname = MessageMap.GetEventName(protoPacket.cmd);
              Event.FireOut(eventname, new object[]{protoPacket.payload});
            */


        }
    }

    public static void ShowBytes(string header, byte[] data, int length) {
        string result = "";
        for(int i=0;i<length;i++){
            result+= data[i].ToString("X2");
        }

        Debug.Log(header+":"+result);
    }



    // 关闭Socket
    public void Close() {
        if (clientSocket != null && clientSocket.Connected) {
            clientSocket.Shutdown(SocketShutdown.Both);
        }
        clientSocket.Close();
    }

};
