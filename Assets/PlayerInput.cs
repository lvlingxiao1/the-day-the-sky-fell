using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {
    public bool inputEnabled = true;

    public float goingForward;
    public float goingRight;
    public float peakRight;
    public bool jumpPressed;
    public bool interactBtnDown;
    public bool grabBtnDown;
    public bool cancelBtnDown;
    
    public float mouseMoveX;
    public float mouseMoveY;

    public float moveMagnitude;

    private float verticleInputRaw;
    private float horizontalInputRaw;

    // Update is called once per frame
    void Update() {
        mouseMoveX = Input.GetAxis("Mouse X");
        mouseMoveY = Input.GetAxis("Mouse Y");
        peakRight = Input.GetAxis("Peak");

        if (inputEnabled) {
            verticleInputRaw = Input.GetAxis("Vertical");
            horizontalInputRaw = Input.GetAxis("Horizontal");
            jumpPressed = Input.GetButtonDown("Jump");
            interactBtnDown = Input.GetButtonDown("Interact");
            cancelBtnDown = Input.GetButtonDown("Cancel");
            grabBtnDown = Input.GetButtonDown("Grab");

            // elliptical grid mapping: https://arxiv.org/ftp/arxiv/papers/1509/1509.06344.pdf
            goingForward = verticleInputRaw * Mathf.Sqrt(1 - horizontalInputRaw * horizontalInputRaw * 0.5f);
            goingRight = horizontalInputRaw * Mathf.Sqrt(1 - verticleInputRaw * verticleInputRaw * 0.5f);
        } else {
            jumpPressed = false;
            interactBtnDown = false;
            cancelBtnDown = false;
            goingForward = 0;
            goingRight = 0;
        }

        moveMagnitude = Mathf.Sqrt(goingForward * goingForward + goingRight * goingRight);
    }
}
