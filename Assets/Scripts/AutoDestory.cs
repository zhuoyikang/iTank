using UnityEngine;
using System.Collections;

public class AutoDestory : MonoBehaviour {

    public float time = 2;

    // Use this for initialization
    void Start () {
        DestroyObject(this.gameObject, time);
    }

    // Update is called once per frame
    void Update () {

    }
}
