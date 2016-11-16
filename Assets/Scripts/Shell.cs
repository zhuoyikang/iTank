using UnityEngine;
using System.Collections;

public class Shell : MonoBehaviour {

    public GameObject shellExplosionPrefeb;
    // public AudioSource audio;

    // Use this for initialization
    void Start () {
//        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update () {

    }


    public void OnTriggerEnter(Collider collider) {
        GameObject.Instantiate (shellExplosionPrefeb, transform.position,
                                transform.rotation);
        GameObject.Destroy(this.gameObject);

        // audio.Play();

        if(collider.tag == "Tank") {
            collider.SendMessage("SetDamage", 1);
        }

    }

}
