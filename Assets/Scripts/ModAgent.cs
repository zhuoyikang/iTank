using UnityEngine;
using System.IO;
using System.Net.Sockets;
using MsgPack.Serialization;

public class ModAgent {

    public void RegisterEvent() {
        NetEvent.RegisterOut("e_test", this, "LoginAck");
    }

    public void LoginAck(MemoryStream ms) {
        var serializer = MessagePackSerializer.Get<Tank> ();
        Tank tank = serializer.Unpack(ms);
        Debug.Log("get fire "+ tank);
    }

    public void LoginReq(string name) {
        // var agentLogin = new agent_login_req();
        // ServerMessage.Instance.Send<agent_login_req>(1001, agentLogin);

    }

    //public void LoginAck(byte[] ackBin) {
    // var ms2 = new MemoryStream(ackBin, 0, ackBin.Length);
    // var agentLoginAck = ProtoBuf.Serializer.
    //     Deserialize<agent_login_ack>(ms2);
    // GameManager._userId = agentLoginAck.player_id;
    //}

}
