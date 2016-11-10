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
    }


    // 同步tank数据，如果没有则新建，有则同步.
    public void SyncTank(MemoryStream ms) {

        var serializer = MessagePackSerializer.Get<Tank> ();
        Tank tank = serializer.Unpack(ms);
        Debug.Log("get fire "+ tank);

        GameObject go;
        Debug.Log("tank size:"+ _tankMap.Count);

        if(_tankMap.TryGetValue(tank.Id, out go)) {

            go.transform.position = tank.Position;
            go.GetComponent<TankEntity>().SendMessage("SetTankId", tank.Id);
            go.GetComponent<TankEntity>().SendMessage("SetTankRole", TankRole.Net);
            go.GetComponent<TankEntity>().SendMessage("SetMoveStatus", tank.MoveStatus);
            go.GetComponent<TankEntity>().SendMessage("SetMoveDirect", tank.MoveDirect);


        } else {
            go = GameObject.Instantiate(_tankPrefeb,
                                        tank.Position,
                                        Quaternion.identity) as GameObject;
            go.transform.SetParent(_tankHolder.transform);
            _tankMap.Add(tank.Id, go);
        }

    }

    void Start () {
        // _tankPrefeb = Resources.Load("Tank",typeof(GameObject))
        //     as GameObject;
        _tankHolder = new GameObject("tankHolder");
    }


}
