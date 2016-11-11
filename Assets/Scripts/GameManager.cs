using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    //private static ModAgent _modAgent = new ModAgent();

    // Use this for initialization
    void Start () {

        Debug.Log ("GameManager Start");
        NetSocket.Instance.Connect ("127.0.0.1", 4001);
        EventName.Install ();
        GetComponent<ModAgent>().RegisterEvent();
        Application.runInBackground = true;

    }

    // Update is called once per frame
    void Update () {
        NetEvent.ProcessOut();
    }

    void Fire () {
        Debug.Log("get fire");
    }

}
