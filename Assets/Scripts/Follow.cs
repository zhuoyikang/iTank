using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour {

    public Transform tank;
    public Vector3 offset;

    // Use this for initialization
    void Start () {
        offset = transform.position - tank.position;
    }

    // Update is called once per frame
    void Update () {
        if(tank == null) {
            return;
        }

        transform.position = tank.position+offset;
    }
}
