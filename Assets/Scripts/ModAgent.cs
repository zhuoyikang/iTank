using UnityEngine;
using System.IO;
using MsgPack.Serialization;
using System.Collections.Generic;

public class ModAgent : MonoBehaviour {


    public Dictionary<int, GameObject> _tankMap =
        new Dictionary<int, GameObject>();

    public GameObject _tankPrefeb;
    GameObject _tankHolder;


    public void RegisterEvent() {
        NetEvent.RegisterOut("SyncTank", this, "SyncTank");
        NetEvent.RegisterOut("SyncShot", this, "SyncShot");
        NetEvent.RegisterOut("SyncDamage", this, "SyncDamage");
        NetEvent.RegisterOut("SyncDie", this, "SyncDie");
    }


    // 同步tank数据，如果没有则新建，有则同步.
    public void SyncTank(MemoryStream ms) {

        var serializer = MessagePackSerializer.Get<Tank> ();
        Tank tank = serializer.Unpack(ms);
        GameObject go;

        if(_tankMap.TryGetValue(tank.Id, out go)) {

            go.transform.position = tank.Position;
            go.GetComponent<TankEntity>().SendMessage("SetMoveStatus", tank.MoveStatus);
            go.GetComponent<TankEntity>().SendMessage("SetMoveDirect", tank.MoveDirect);

        } else {
            go = GameObject.Instantiate(_tankPrefeb, tank.Position,
                                        Quaternion.identity) as GameObject;
            go.transform.SetParent(_tankHolder.transform);
            go.GetComponent<TankEntity>().SendMessage("SetTankRole", TankRole.Net);
            go.GetComponent<TankEntity>().SendMessage("SetTankId", tank.Id);
            go.GetComponent<TankEntity>().SendMessage("SetMoveStatus", tank.MoveStatus);
            go.GetComponent<TankEntity>().SendMessage("SetMoveDirect", tank.MoveDirect);
            _tankMap.Add(tank.Id, go);
        }

    }

    // 同步tank数据，如果没有则新建，有则同步.
    public void SyncShot(MemoryStream ms) {
        // byte[] data =  ms.ToArray();
        // NetSocket.ShowBytes("SyncShot ox", data, data.Length);

        var serializer = MessagePackSerializer.Get<Shot> ();
        Shot shot = serializer.Unpack(ms);
        GameObject go;

        if(!_tankMap.TryGetValue(shot.Id, out go)) {
            return;
        }

        go.GetComponent<TankEntity>().SendMessage("Shot", 1);
    }

    public void SyncDamage(MemoryStream ms) {
        Debug.Log("SyncDamage  ...");

        var serializer = MessagePackSerializer.Get<Damage> ();
        Damage damage = serializer.Unpack(ms);
        GameObject go;

        if(!_tankMap.TryGetValue(damage.Id, out go)) {
            return;
        }

        go.GetComponent<TankEntity>().SendMessage("SetDamage", 1);

    }

    public void SyncDie(MemoryStream ms) {
        Debug.Log("SyncDie  ...");

        var serializer = MessagePackSerializer.Get<Die> ();
        Die die = serializer.Unpack(ms);
        GameObject go;

        if(!_tankMap.TryGetValue(die.Id, out go)) {
            return;
        }

        go.GetComponent<TankEntity>().SendMessage("SetDie", 1);

    }

    void Start () {
        // _tankPrefeb = Resources.Load("Tank",typeof(GameObject))
        //     as GameObject;
        _tankHolder = new GameObject("tankHolder");
    }


}
