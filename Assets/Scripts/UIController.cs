using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour {
    private static Animator animator;
    public static bool gameStarted = false;
    private void Awake() {
        animator = GetComponent<Animator>();
    }
    private void Update() {
        if (gameStarted) return;
        if (Input.anyKeyDown) {
            animator.SetTrigger("start");
            gameStarted = true;
        }
    }
    public static void BlackScreen() {
        animator.SetTrigger("black_screen");
    }
    public static void Hide() {
        animator.SetBool("ui", false);
    }
    public static void Show() {
        animator.SetBool("ui", true);
    }
    public static void Restart() {
        gameStarted = false;
        animator.SetTrigger("restart");
    }
}
