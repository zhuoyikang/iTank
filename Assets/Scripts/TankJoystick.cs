using UnityEngine;


// 进行摇杆操做
public class TankJoystick : MonoBehaviour {


    private float oldJoyPositionY;
    private float oldJoyPositionX;


    void Start() {
    }

    void OnEnable() {
        EasyJoystick.On_JoystickMoveStart += OnJoystickMoveStart;
        EasyJoystick.On_JoystickMove += OnJoystickMove;
        EasyJoystick.On_JoystickMoveEnd += OnJoystickMoveEnd;

        EasyButton.On_ButtonDown += OnButtonDown;
    }


    void OnJoystickMoveStart(MovingJoystick move) {
        Debug.Log(move.joystickName);
        if (move.joystickName != "MoveJoystick") {
            return;
        }

        GetComponent<TankEntity> ().SetMoveStatus (TankMoveStatus.Moving);
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

        //获取摇杆中心偏移的坐标
        float joyPositionX = move.joystickAxis.x;
        float joyPositionY = move.joystickAxis.y;

        if (joyPositionY != oldJoyPositionY || joyPositionX != oldJoyPositionX) {
            oldJoyPositionY = joyPositionY;
            oldJoyPositionX = joyPositionX;

            GetComponent<TankEntity>().
                SetMoveDirect(new Vector3(joyPositionX, 0, joyPositionY));
        }
    }

    void OnButtonDown(string name) {
        GetComponent<TankEntity> ().Shot(1);
    }


    void Update (){
        if(Input.GetKeyDown(KeyCode.Space)){
            GetComponent<TankEntity> ().Shot(1);
        }
    }


}
