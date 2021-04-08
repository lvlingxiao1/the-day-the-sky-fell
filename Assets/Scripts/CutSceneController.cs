using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneController : MonoBehaviour {
    Camera mainCamera;
    Camera cutSceneCamera;
    Animator animator;
    void Awake() {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        cutSceneCamera = GetComponentInChildren<Camera>();
        animator = GetComponent<Animator>();
    }

    public void StartCutScene(string cutSceneName) {
        animator.SetTrigger(cutSceneName);
        UIController.Hide();
        cutSceneCamera.enabled = true;
        mainCamera.enabled = false;
    }

    public void EndCutScene() {
        mainCamera.enabled = true;
        cutSceneCamera.enabled = false;
        UIController.Show();
    }

    public void Credits() {
        animator.SetTrigger("credits");
    }

    public void ResetGame() {
        var originalPosition = GameObject.Find("PlayerStartPos").transform;
        var player = GameObject.Find("Player").transform;
        player.position = originalPosition.position;
        player.rotation = originalPosition.rotation;
        mainCamera.transform.position = new Vector3(-22.25908f, -15.89375f, -82.46664f);
        mainCamera.transform.eulerAngles = new Vector3(-5.708f, 93.74001f, 0);
        UIController.Restart();
    }
}
