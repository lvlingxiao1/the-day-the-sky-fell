using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour {
    // Start is called before the first frame update
    public int coins;
    public Material emptyBox;
    Main main;

    void Start() {
        main = FindObjectOfType<Main>();
    }

    // Update is called once per frame
    void Update() {

    }

    private void OnCollisionEnter(Collision other) {
        if (coins > 0) {
            coins--;
            main.AddScore();
            if (coins == 0) {
                transform.parent.GetComponent<Renderer>().material = emptyBox;
            }
        }
    }
}
