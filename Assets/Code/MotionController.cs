using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotionController : MonoBehaviour {
    public float forwardSpeed = 10f;
    public float jumpSpeed = 25f;
    public float cameraSpeed = 3f;

    float goingForward;
    float goingRight;
    float moveMagnitude;
    float zooming;
    bool jumpPressed;
    bool grounded;
    float groundCheckDistance;
    Rigidbody rb;
    Transform cameraTransform;
    Camera mainCamera;
    bool cameraMoved = false;
    Animator animator;
    AudioManager audioManager;
    Text debugText;
    Transform modelTransform;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        cameraTransform = GameObject.Find("CameraTransform").transform;
        mainCamera = cameraTransform.gameObject.GetComponentInChildren<Camera>();
        modelTransform = GameObject.Find("ybot").transform;
        animator = modelTransform.GetComponent<Animator>();
        groundCheckDistance = GetComponent<CapsuleCollider>().height / 2 + 0.1f;
        audioManager = FindObjectOfType<AudioManager>();
        debugText = GameObject.Find("debugText").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update() {
        goingForward = Input.GetAxis("Vertical");
        goingRight = Input.GetAxis("Horizontal");
        jumpPressed = Input.GetButton("Jump");
        zooming = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetMouseButton(0)) {
            Vector3 cameraRotation = cameraTransform.eulerAngles;
            cameraRotation.y += Input.GetAxis("Mouse X") * cameraSpeed;
            cameraRotation.x -= Input.GetAxis("Mouse Y");
            cameraTransform.eulerAngles = cameraRotation;
            cameraMoved = true;
        } else if (Input.GetMouseButton(1)) {
            Vector3 rotation = transform.eulerAngles;
            Vector3 cameraRotation = cameraTransform.eulerAngles;

            if (cameraMoved) {
                rotation.y = cameraRotation.y + Input.GetAxis("Mouse X") * cameraSpeed;
                cameraMoved = false;
            } else {
                rotation.y += Input.GetAxis("Mouse X") * cameraSpeed;
            }

            cameraRotation.x -= Input.GetAxis("Mouse Y");
            cameraRotation.y = 0f;
            transform.eulerAngles = rotation;
            cameraTransform.localEulerAngles = cameraRotation;
        }

        if (zooming != 0) {
            mainCamera.fieldOfView += zooming * 8;
            if (mainCamera.fieldOfView > 80) mainCamera.fieldOfView = 80;
            if (mainCamera.fieldOfView < 30) mainCamera.fieldOfView = 30;
        }

        moveMagnitude = Mathf.Sqrt(goingForward * goingForward + goingRight * goingRight);
        animator.SetFloat("forward", moveMagnitude);
        if (moveMagnitude > 0.1) {
            modelTransform.forward = goingForward * transform.forward + goingRight * transform.right;
            if (grounded) {
                audioManager.PlayIfNotPlaying("walk");
            }
        } else {
            audioManager.Stop("walk");
        }
    }

    private void FixedUpdate() {
        grounded = Physics.Raycast(rb.position + Vector3.up, Vector3.down, groundCheckDistance);
        debugText.text = $"{grounded} {jumpPressed} v={rb.velocity} q={rb.position}";
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, new Color(1, 0, 0));

        if (grounded) {
            if (jumpPressed) {
                rb.AddForce(new Vector3(0, jumpSpeed, 0), ForceMode.VelocityChange);
                audioManager.Play("jump");
            }
        }
        //rb.MovePosition(rb.position + forawrdSpeed * goingForward * transform.forward
        //   + forawrdSpeed * goingRight * transform.right);
        Vector3 newVelocity = forwardSpeed * goingForward * transform.forward + forwardSpeed * goingRight * transform.right;
        newVelocity.y = rb.velocity.y;
        rb.velocity = newVelocity;
        //rb.position += modelTransform.forward * moveMagnitude * forwardSpeed * Time.fixedDeltaTime;
    }
}
