using UnityEngine;
using System.Collections;

public class Tank {

    // Id
    public int Id {get ; set;}

    // 当前位置和朝向
    public Vector3 Position {get ; set ;}
    public Vector3 Rotation{get ; set ;}

    // 当前运动状态
    public int MoveStatus{ get ; set;}

    // 移动朝向
    public Vector3 MoveDirect{get ; set;}

    public Tank() {
        Position = new Vector3();
        Rotation = new Vector3();
        MoveDirect = new Vector3();
        MoveStatus = (int)TankMoveStatus.Moving;
    }

    public override string ToString() {
        return "Tank:"+Id;
    }



}
