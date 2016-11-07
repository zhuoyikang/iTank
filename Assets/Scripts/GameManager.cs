using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log ("GameManager Start");

        NetSocket.Instance.Connect ("127.0.0.1", 4001);
        EventName.Install ();

        NetSocket.Instance.Send<Product> ("e_test", new Product ());
       

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
