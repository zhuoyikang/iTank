using UnityEngine;
using System.Collections;

public class Tank {

    // Id
    public long Id {get ; set;}

    // 当前位置和朝向
    public Vector3 Position {get ; set ;}
    public Vector3 Rotation{get ; set ;}

    // 当前运动状态
    public int MoveStatus{ get ; set;}

    // 移动朝向
    public Vector3 MoveDirect{get ; set;}

    public Tank() {
        Id = 1;
        Position = new Vector3();
        Rotation = new Vector3();
        MoveDirect = new Vector3();
        MoveStatus = (int)TankMoveStatus.Moving;
    }



}
