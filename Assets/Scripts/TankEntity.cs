using UnityEngine;
using UnityEngine.UI;


// 一个坦克的移动包含以下属性:
// 1.是否移动 m_MoveStatus;
// 2.移动方向 m_MoveDirect
//
public class TankEntity : MonoBehaviour {


    private int _hpTotal = 100;
    private Transform firePos;
    public GameObject shellPrefeb;
    public GameObject tankExplosionPrefeb;
    public AudioClip tankExplosionAudio;
    public AudioClip shotAudio;
    public Slider hpSlider;
    public float shellSpeed = 15;
    public int _hp=100;


    public int _id;
    private Vector3 m_MoveDirect;
    private TankMoveStatus m_MoveStatus = TankMoveStatus.Stopped;
    private TankRole m_Role = TankRole.Main;

    public void Start() {
        _id = Random.Range(0, 10000);
        firePos = transform.Find("FirePos");
        //hpSlider = GetComponent<Slider>();
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

    public void SyncDamage() {
        if(m_Role != TankRole.Main) {
            return;
        }
        var s = new Damage();
        s.Id = _id;
        s.Hp = _hp;
        NetSocket.Instance.Send<Damage> ("SyncDamage", s);
    }


    public void SyncDie() {
        if(m_Role != TankRole.Main) {
            return;
        }

        var s = new Die();
        s.Id = _id;
        NetSocket.Instance.Send<Die> ("SyncDie", s);
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
        AudioSource.PlayClipAtPoint(shotAudio, transform.position);
        SyncShot();
    }


    public void SetTankId(int id) {
        _id = id;
    }

    public void SetTankRole(TankRole role) {
        m_Role = role;
    }

    // 设置死亡
    public void SetDie(int i) {
        GameObject.Instantiate (tankExplosionPrefeb, transform.position + Vector3.up,
                                transform.rotation);

        AudioSource.PlayClipAtPoint(tankExplosionAudio, transform.position);
        if(m_Role == TankRole.Net) {
            GameObject.Destroy(this.gameObject);
        }else{
            SyncDie();
            GameObject.Destroy(this.gameObject);
        }

    }


    public void SetDamage(int i) {
        Debug.Log("damage: "+_id);
        _hp -= 10;
        hpSlider.value = (float)_hp / (_hpTotal);

        if(m_Role == TankRole.Net) {
            return;
        } else {
            if (_hp <=0 ){
                SetDie(1);
            } else {
                SyncDamage();
            }
        }
    }

    // 固定移动
    public void FixedUpdate() {
        if (m_MoveStatus == TankMoveStatus.Moving) {
            transform.LookAt(transform.position + m_MoveDirect);
            transform.Translate(Vector3.forward * Time.deltaTime *5);
        }
    }

}
