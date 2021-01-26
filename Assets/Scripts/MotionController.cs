using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotionController : MonoBehaviour {
    public float forwardSpeed = 10f;
    public float jumpSpeed = 4f;
    public float cameraSpeed = 3f;
    public float interactDetectDistance = 1.0f;

    // user inputs
    float goingForward;
    float goingRight;
    bool jumpPressed;
    bool interactBtnDown;
    bool cancelBtnDown;

    // attributes
    float moveMagnitude;
    bool grounded;
    float groundCheckDistance;
    bool cameraMoved = false;
    enum States {
        NORMAL,
        LADDER_ENTER,
        ON_LADDER,
        LADDER_EXIT
    }
    States state = States.NORMAL;
    Ladder currentLadder;

    // components
    Rigidbody rb;
    Transform cameraTransform;
    Camera mainCamera;
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

    void Update() {
        goingForward = Input.GetAxis("Vertical");
        goingRight = Input.GetAxis("Horizontal");
        jumpPressed = Input.GetButton("Jump");
        interactBtnDown = Input.GetButtonDown("Interact");
        cancelBtnDown = Input.GetButtonDown("Cancel");

        // camera control
        if (Input.GetMouseButton(0)) {
            Vector3 cameraRotation = cameraTransform.eulerAngles;
            cameraRotation.y += Input.GetAxis("Mouse X") * cameraSpeed;
            cameraRotation.x -= Input.GetAxis("Mouse Y");
            cameraTransform.eulerAngles = cameraRotation;
            cameraMoved = true;
        } else if (Input.GetMouseButton(1)) {
            if (state == States.NORMAL) {
                Vector3 rotation = transform.eulerAngles;
                Vector3 cameraRotation = cameraTransform.eulerAngles;

                if (cameraMoved) {
                    rotation.y = cameraRotation.y + Input.GetAxis("Mouse X") * cameraSpeed;
                    modelTransform.forward = transform.forward;
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

        moveMagnitude = Mathf.Sqrt(goingForward * goingForward + goingRight * goingRight);
        grounded = Physics.Raycast(transform.position + Vector3.up, Vector3.down, groundCheckDistance);
        Debug.DrawRay(transform.position + Vector3.up, Vector3.down * groundCheckDistance, new Color(1, 0, 0));

        bool interactDetected = Physics.Raycast(transform.position + Vector3.up, modelTransform.forward, out RaycastHit hitInfo, groundCheckDistance);
        Debug.DrawRay(transform.position + Vector3.up, modelTransform.forward * 1.0f, new Color(1, 0, 0));

        if (interactBtnDown) {
            if (state == States.NORMAL) {
                if (interactDetected && hitInfo.collider.tag == "ladder") {
                    rb.useGravity = false;
                    state = States.LADDER_ENTER;
                    animator.SetBool("climb", true);
                    currentLadder = hitInfo.collider.GetComponentInParent<Ladder>();
                    currentLadder.GetOntoLadder(transform, forwardSpeed);
                }
            }


            if (state == States.ON_LADDER) {
                state = States.NORMAL;
                rb.useGravity = true;
                animator.SetBool("climb", false);
            }
        }

        if (cancelBtnDown) {
            if (state == States.ON_LADDER) {
                state = States.NORMAL;
                rb.useGravity = true;
                animator.SetBool("climb", false);
            }
        }



        animator.SetFloat("forward", moveMagnitude);
        animator.SetBool("grounded", grounded);

        debugText.text = $"{grounded} {jumpPressed} v={rb.velocity} {state} interact={(interactDetected ? hitInfo.collider.name : "")}";
    }

    private void FixedUpdate() {
        Vector3 newVelocity;
        switch (state) {
            case States.NORMAL:
                if (grounded) {
                    if (jumpPressed) {
                        rb.AddForce(new Vector3(0, jumpSpeed, 0), ForceMode.VelocityChange);
                        audioManager.Play("jump");
                    }
                }
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
            case States.ON_LADDER:
                Vector3 newPos = rb.position + 2.0f * goingForward * Vector3.up * Time.fixedDeltaTime;
                if (goingForward > 0) {
                    if (newPos.y < currentLadder.TopY - 1.4f) {
                        rb.position = newPos;
                    } else {
                        state = States.LADDER_EXIT;
                        animator.SetBool("climb", false);
                        currentLadder.ExitLadderFromTop(transform);
                    }
                } else if (goingForward < 0) {
                    if (newPos.y > currentLadder.BaseY) {
                        rb.position = newPos;
                    } else {
                        state = States.NORMAL;
                        rb.useGravity = true;
                        animator.SetBool("climb", false);
                    }
                }
                break;
        }
    }

    public void setStateOnLadder() {
        state = States.ON_LADDER;
    }
    public void setStateNormal() {
        state = States.NORMAL;
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
    }
}
