using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {
    public bool inputEnabled = true;

    [HideInInspector] public float goingForward;
    [HideInInspector] public float goingRight;
    [HideInInspector] public float cameraHorizontal;
    [HideInInspector] public float cameraVertical;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool interactBtnDown;
    [HideInInspector] public bool grabBtnDown;
    [HideInInspector] public bool cancelBtnDown;
    [HideInInspector] public bool releaseBtnDown;
    [HideInInspector] public bool runBtnHold;

    [HideInInspector] public float mouseMoveX;
    [HideInInspector] public float mouseMoveY;

    [HideInInspector] public float moveMagnitude;

    private float verticleInputRaw;
    private float horizontalInputRaw;

    private Coroutine currentLockCoroutine;

    private void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        mouseMoveX = Input.GetAxis("Mouse X");
        mouseMoveY = Input.GetAxis("Mouse Y");
        cameraHorizontal = Input.GetAxis("CameraHorizontal");
        cameraVertical = Input.GetAxis("CameraVertical");

        if (inputEnabled) {
            verticleInputRaw = Input.GetAxis("Vertical");
            horizontalInputRaw = Input.GetAxis("Horizontal");
            jumpPressed = Input.GetButtonDown("Jump");
            interactBtnDown = Input.GetButtonDown("Interact");
            cancelBtnDown = Input.GetButtonDown("Cancel");
            grabBtnDown = Input.GetButtonDown("Grab");
            releaseBtnDown = Input.GetButton("Release");
            runBtnHold = Input.GetButton("Run");

            // elliptical grid mapping: https://arxiv.org/ftp/arxiv/papers/1509/1509.06344.pdf
            goingForward = verticleInputRaw * Mathf.Sqrt(1 - horizontalInputRaw * horizontalInputRaw * 0.5f);
            goingRight = horizontalInputRaw * Mathf.Sqrt(1 - verticleInputRaw * verticleInputRaw * 0.5f);
        } else {
            jumpPressed = false;
            interactBtnDown = false;
            cancelBtnDown = false;
            goingForward = 0;
            goingRight = 0;
            runBtnHold = false;
        }

        moveMagnitude = Mathf.Sqrt(goingForward * goingForward + goingRight * goingRight);
    }

    public void LockInputForSeconds(float seconds) {
        if (currentLockCoroutine != null) StopCoroutine(currentLockCoroutine);
        currentLockCoroutine = StartCoroutine(LockInputCoroutine(seconds));
    }

    public IEnumerator LockInputCoroutine(float seconds) {
        inputEnabled = false;
        yield return new WaitForSeconds(seconds);
        inputEnabled = true;
    }
}
