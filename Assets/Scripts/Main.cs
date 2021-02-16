using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {
    GameObject player;
    public int livesInitial = 2;
    Text youDiedText;
    //bool respawning;
    AudioManager audioManager;

    void Awake() {
        player = GameObject.Find("Player");
        audioManager = FindObjectOfType<AudioManager>();
        youDiedText = GameObject.Find("YouDiedText").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update() {
        //if (lives >= 0 && player.transform.position.y < -100 && !respawning) {
        //    respawning = true;
        //    youDiedText.enabled = true;
        //    StartCoroutine(PlayerDie());
        //} else if (respawning) {
        //    Color newColor = youDiedText.color;
        //    newColor.a += 0.02f;
        //    youDiedText.color = newColor;
        //}
        if (player.transform.position.y < -80) {
            player.transform.position = new Vector3(0, -11.47f, -10);
            player.transform.eulerAngles = new Vector3(0, 0, 0);
            MotionController controller = player.GetComponent<MotionController>();
            controller.SetStateNormal();
            //controller.ResetCamera();
        }
    }

    IEnumerator PlayerDie() {
        audioManager.Play("death");
        Transform cameraTransform = GameObject.Find("CameraTransform").transform;
        cameraTransform.parent = null;

        yield return new WaitForSeconds(3f);

        player.transform.position = new Vector3(0, -11.47f, -10);
        player.transform.eulerAngles = new Vector3(0, 0, 0);
        Animator animator = player.GetComponentInChildren<Animator>();
        player.GetComponent<MotionController>().SetStateNormal();

        cameraTransform.parent = player.transform;
        cameraTransform.localPosition = new Vector3(0, 1.3f, 0);
        //respawning = false;
        youDiedText.enabled = false;
        Color newColor = youDiedText.color;
        newColor.a = 0;
        youDiedText.color = newColor;
    }
}

