using UnityEngine;


// 一个坦克的移动包含以下属性:
// 1.是否移动 m_MoveStatus;
// 2.移动方向 m_MoveDirect
//
public class TankMovement : MonoBehaviour {

    private Vector3 m_MoveDirect;
    private TankMoveStatus m_MoveStatus = TankMoveStatus.Stopped;

    // 设置移动状态
    public void SetMoveStatus(TankMoveStatus status) {
        m_MoveStatus = status;
    }

    // 设置移动方向
    public void SetMoveDirect(Vector3 direct) {
        m_MoveDirect = direct;
    }

    // 固定移动
    public void FixedUpdate() {
        if (m_MoveStatus == TankMoveStatus.Moving) {
            transform.LookAt(transform.position + m_MoveDirect);
            transform.Translate(Vector3.forward * Time.deltaTime *5);
        }
    }

}
