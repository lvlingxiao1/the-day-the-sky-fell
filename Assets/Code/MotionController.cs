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
    bool interactBtnDown;
    float groundCheckDistance;
    Rigidbody rb;
    Transform cameraTransform;
    Camera mainCamera;
    bool cameraMoved = false;
    Animator animator;
    AudioManager audioManager;
    Text debugText;
    Transform modelTransform;

    const int NORMAL = 0;
    const int CLIMBING = 1;
    int status = NORMAL;

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
        interactBtnDown = Input.GetButtonDown("Interact");
        zooming = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetMouseButton(0)) {
            Vector3 cameraRotation = cameraTransform.eulerAngles;
            cameraRotation.y += Input.GetAxis("Mouse X") * cameraSpeed;
            cameraRotation.x -= Input.GetAxis("Mouse Y");
            cameraTransform.eulerAngles = cameraRotation;
            cameraMoved = true;
        } else if (Input.GetMouseButton(1)) {
            if (status == NORMAL) {
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
        }
        if (zooming != 0) {
            mainCamera.fieldOfView += zooming * 8;
            if (mainCamera.fieldOfView > 80) mainCamera.fieldOfView = 80;
            if (mainCamera.fieldOfView < 30) mainCamera.fieldOfView = 30;
        }

        moveMagnitude = Mathf.Sqrt(goingForward * goingForward + goingRight * goingRight);
        animator.SetFloat("forward", moveMagnitude);
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


        bool hit = Physics.Raycast(rb.position + Vector3.up, transform.forward, out RaycastHit hitInfo, groundCheckDistance);
        if (hit && hitInfo.collider.tag == "climbable") {
            if (interactBtnDown) {
                if (status == NORMAL) {
                    rb.useGravity = false;
                    status = CLIMBING;
                    animator.SetBool("climb", true);
                } else if (status == CLIMBING) {
                    rb.useGravity = true;
                    status = NORMAL;
                    animator.SetBool("climb", false);
                }
            }
        }



        Vector3 newVelocity;
        switch (status) {
            case NORMAL:
                if (moveMagnitude > 0.1) {
                    modelTransform.forward = goingForward * transform.forward + goingRight * transform.right;
                    if (grounded) {
                        audioManager.PlayIfNotPlaying("walk");
                    }
                } else {
                    audioManager.Stop("walk");
                }
                newVelocity = forwardSpeed * goingForward * transform.forward + forwardSpeed * goingRight * transform.right;
                newVelocity.y = rb.velocity.y;
                rb.velocity = newVelocity;
                break;
            case CLIMBING:
                newVelocity = 2.0f * goingForward * Vector3.up;
                rb.velocity = newVelocity;
                break;
        }
        debugText.text = $"{grounded} {jumpPressed} v={rb.velocity} {status}";
        Debug.DrawRay(rb.position + Vector3.up, transform.forward * 0.5f, new Color(1, 0, 0));

        //rb.MovePosition(rb.position + forawrdSpeed * goingForward * transform.forward
        //   + forawrdSpeed * goingRight * transform.right);

        //rb.position += modelTransform.forward * moveMagnitude * forwardSpeed * Time.fixedDeltaTime;
    }
}
