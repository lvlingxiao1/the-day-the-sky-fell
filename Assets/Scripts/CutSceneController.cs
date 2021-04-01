using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneController : MonoBehaviour {
    Camera mainCamera;
    Camera cutSceneCamera;
    Animator animator;
    Canvas UI;
    void Awake() {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        cutSceneCamera = GetComponentInChildren<Camera>();
        animator = GetComponent<Animator>();
        UI = GameObject.Find("UI").GetComponent<Canvas>();
    }

    public void StartCutScene(string cutSceneName) {
        animator.SetTrigger(cutSceneName);
        UI.enabled = false;
        cutSceneCamera.enabled = true;
        mainCamera.enabled = false;
    }

    public void EndCutScene() {
        mainCamera.enabled = true;
        cutSceneCamera.enabled = false;
        UI.enabled = true;
    }
}
