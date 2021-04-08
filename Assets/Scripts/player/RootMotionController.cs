using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionController : MonoBehaviour {
    PlayerController controller;
    Animator animator;

    void Awake() {
        controller = GameObject.Find("Player").GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
    }
    private void OnAnimatorMove() {
        controller.AddRootMotionDelta(animator.deltaPosition);
    }
}
