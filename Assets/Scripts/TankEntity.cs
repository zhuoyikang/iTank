using UnityEngine;


// 一个坦克的移动包含以下属性:
// 1.是否移动 m_MoveStatus;
// 2.移动方向 m_MoveDirect
//
public class TankEntity : MonoBehaviour {

    private Transform firePos;
    public GameObject shellPrefeb;
    public float shellSpeed = 15;

    public int _id;
    private Vector3 m_MoveDirect;
    private TankMoveStatus m_MoveStatus = TankMoveStatus.Stopped;
    private TankRole m_Role = TankRole.Main;

    public void Start() {
        _id = Random.Range(0, 10000);
        firePos = transform.Find("FirePos");
    }

    public void SyncTank() {

        if(m_Role != TankRole.Main) {
            return;
        }

        Debug.Log("sync net:" + _id);

        var t = new Tank();
        t.Id = _id;
        t.Position = transform.position;
        t.MoveStatus = (int)m_MoveStatus;
        t.MoveDirect = m_MoveDirect;
        NetSocket.Instance.Send<Tank> ("SyncTank", t);
    }

    public void SyncShot() {
        if(m_Role != TankRole.Main) {
            return;
        }

        var s = new Shot();
        s.Id = _id;
        NetSocket.Instance.Send<Shot> ("SyncShot", s);
    }

    // 设置移动状态
    public void SetMoveStatus(TankMoveStatus status) {
        m_MoveStatus = status;

        if(m_Role == TankRole.Net) {
            return;
        }

        SyncTank();
    }

    // 设置移动方向
    public void SetMoveDirect(Vector3 direct) {
        m_MoveDirect = direct;

        if(m_Role == TankRole.Net) {
            return;
        }

        SyncTank();
    }

    // 发射字段
    public void Shot(int i) {
        var go = GameObject.Instantiate (shellPrefeb, firePos.position,
                                         firePos.rotation) as GameObject;
        go.GetComponent<Rigidbody>().velocity = go.transform.forward * shellSpeed;
        SyncShot();
    }


    public void SetTankId(int id) {
        _id = id;
    }

    public void SetTankRole(TankRole role) {
        m_Role = role;
    }

    // 固定移动
    public void FixedUpdate() {
        if (m_MoveStatus == TankMoveStatus.Moving) {
            transform.LookAt(transform.position + m_MoveDirect);
            transform.Translate(Vector3.forward * Time.deltaTime *5);
        }
    }

}
