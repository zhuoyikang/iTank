using UnityEngine;


// 进行摇杆操做
public class TankJoystick : MonoBehaviour {

    void OnEnable() {
        EasyJoystick.On_JoystickMove += OnJoystickMove;
        EasyJoystick.On_JoystickMoveEnd += OnJoystickMoveEnd;
    }


    void OnJoystickMoveEnd(MovingJoystick move) {
        if (move.joystickName != "MoveJoystick") {
            return;
        }

        GetComponent<TankEntity> ().SetMoveStatus (TankMoveStatus.Stopped);
    }

    void OnJoystickMove(MovingJoystick move) {

        if (move.joystickName != "MoveJoystick") {
            return;
        }

        GetComponent<TankEntity> ().SetMoveStatus (TankMoveStatus.Moving);

        //获取摇杆中心偏移的坐标
        float joyPositionX = move.joystickAxis.x;
        float joyPositionY = move.joystickAxis.y;

        if (joyPositionY != 0 || joyPositionX != 0) {
            GetComponent<TankEntity>().
                SetMoveDirect(new Vector3(joyPositionX, 0, joyPositionY));
        }
    }

}
