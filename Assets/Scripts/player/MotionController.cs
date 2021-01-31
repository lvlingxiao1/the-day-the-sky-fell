using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotionController : MonoBehaviour {
    public float forwardSpeed = 10f;
    public float jumpSpeed = 4f;
    public float cameraSpeedX = 180f;
    public float cameraSpeedY = 60f;
    public float interactDetectDistance = 1.0f;
    public float ledgeSpeed = 3f;

    // user inputs
    float goingForward;
    float goingRight;
    bool jumpPressed;
    bool interactBtnDown;
    bool interactBtnHold;
    bool cancelBtnDown;
    bool inputEnabled = true;

    // attributes
    Vector3 cameraRotation;
    float moveMagnitude;
    bool grounded;
    enum States {
        NORMAL,
        LADDER_ENTER,
        ON_LADDER,
        LADDER_EXIT,
        ON_LEDGE,
        LEDGE_CLIMB_UP
    }
    States state = States.NORMAL;
    Ladder currentLadder;
    Vector3 newVelocity;
    Vector3 cameraForward;
    Vector3 impulseVelocity;
    Vector3 rootMotionDelta;
    //string interactHint;
    enum InteractType {
        None,
        Ladder,
        LedgeBelow,
        LedgeAbove
    }
    InteractType interact;
    RaycastHit hitInfo;

    // components
    Rigidbody rb;
    Transform cameraTransform;
    Animator animator;
    AudioManager audioManager;
    Text debugText;
    Transform modelTransform;
    GroundDetector groundDetector;
    InteractDetector interactDetector;
    LedgeDetector ledgeDetector;
    BoxCollider hangCollider;
    CapsuleCollider mainCollider;
    Text interactHintText;


    void Awake() {
        rb = GetComponent<Rigidbody>();
        cameraTransform = GameObject.Find("CameraTransform").transform;
        modelTransform = GameObject.Find("PlayerModel").transform;
        animator = modelTransform.GetComponent<Animator>();
        audioManager = FindObjectOfType<AudioManager>();
        debugText = GameObject.Find("debugText").GetComponent<Text>();
        cameraRotation = cameraTransform.eulerAngles;
        cameraForward = transform.forward;
        groundDetector = new GroundDetector(new Vector3(0, 0.3f, 0), 0.4f);
        interactDetector = new InteractDetector(transform, modelTransform);
        ledgeDetector = new LedgeDetector(transform, modelTransform);
        hangCollider = GameObject.Find("HangCollider").GetComponent<BoxCollider>();
        mainCollider = GetComponent<CapsuleCollider>();
        interactHintText = GameObject.Find("InteractHint").GetComponent<Text>();
    }

    void Update() {
        float verticleInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        if (inputEnabled) {
            jumpPressed = Input.GetButtonDown("Jump");
            interactBtnDown = Input.GetButtonDown("Interact");
            cancelBtnDown = Input.GetButtonDown("Cancel");
            interactBtnHold = Input.GetButton("Interact");

            // elliptical grid mapping: https://arxiv.org/ftp/arxiv/papers/1509/1509.06344.pdf
            goingForward = verticleInput * Mathf.Sqrt(1 - horizontalInput * horizontalInput * 0.5f);
            goingRight = horizontalInput * Mathf.Sqrt(1 - verticleInput * verticleInput * 0.5f);
            moveMagnitude = Mathf.Sqrt(goingForward * goingForward + goingRight * goingRight);
        } else {
            jumpPressed = false;
            interactBtnDown = false;
            cancelBtnDown = false;
            goingForward = 0;
            goingRight = 0;
            moveMagnitude = 0;
        }


        // camera control
        if (Input.GetMouseButton(0)) {
            cameraRotation.y += Input.GetAxis("Mouse X") * cameraSpeedX * Time.deltaTime;
            cameraRotation.x -= Input.GetAxis("Mouse Y") * cameraSpeedY * Time.deltaTime;
            cameraRotation.x = Mathf.Clamp(cameraRotation.x, -40, 70);
            cameraTransform.eulerAngles = cameraRotation;
            cameraForward.x = cameraTransform.forward.x;
            cameraForward.z = cameraTransform.forward.z;
            cameraForward.Normalize();
        }


        grounded = groundDetector.IsOnGround(transform.position);

        interact = InteractType.None;
        interactHintText.text = "";
        if (state == States.NORMAL) {
            if (grounded) {
                if (interactDetector.DetectFront(out hitInfo)) {
                    if (hitInfo.collider.tag == "ladder") {
                        interact = InteractType.Ladder;
                        interactHintText.text = "Press F to Climb Ladder";
                    }
                } else {
                    if (interactDetector.DetectLedgeBelow()) {
                        interact = InteractType.LedgeBelow;
                        interactHintText.text = "Press F to Climb Ledge Below";
                    }
                }
            } else {
                if (interactDetector.DetectLedgeAbove()) {
                    interact = InteractType.LedgeAbove;
                    interactHintText.text = "Press F to Climb Ledge Above";
                }
            }
        }

        switch (state) {
            case States.NORMAL:
                if (jumpPressed && grounded) {
                    impulseVelocity.y = jumpSpeed;
                    audioManager.Play("jump");
                }

                if (interactBtnDown) {
                    if (interact == InteractType.Ladder) {
                        rb.useGravity = false;
                        animator.SetBool("climb", true);
                        currentLadder = hitInfo.collider.GetComponentInParent<Ladder>();
                        currentLadder.GetOntoLadder(transform, modelTransform, forwardSpeed);
                        state = States.LADDER_ENTER;
                    } else if (interact == InteractType.LedgeBelow) {
                        //rb.useGravity = false;
                        hangCollider.enabled = true;
                        grounded = false;
                        animator.SetBool("on_ledge", true);
                        ledgeDetector.EnterLedge();
                        cameraRotation = modelTransform.eulerAngles + new Vector3(17, 0, 0);
                        cameraTransform.eulerAngles = cameraRotation;
                        state = States.ON_LEDGE;
                    }
                }
                if (interactBtnHold) {
                    if (interact == InteractType.LedgeAbove) {
                        animator.SetBool("on_ledge", true);
                        hangCollider.enabled = true;
                        grounded = false;
                        state = States.ON_LEDGE;
                    }
                }

                if (grounded) {
                    if (moveMagnitude > 0.1) {
                        Vector3 targetDirection = goingForward * cameraForward + goingRight * cameraTransform.right;
                        modelTransform.forward = Vector3.Slerp(modelTransform.forward, targetDirection, 0.3f);
                        audioManager.PlayIfNotPlaying("walk");
                    } else {
                        audioManager.Stop("walk");
                    }
                }

                break;


            case States.ON_LADDER:
                if (cancelBtnDown || interactBtnDown) {
                    state = States.NORMAL;
                    rb.useGravity = true;
                    animator.SetBool("climb", false);
                }
                if (rb.position.y >= currentLadder.TopY - 1.75f) {
                    state = States.LADDER_EXIT;
                    animator.SetTrigger("ladder_top");
                } else if (grounded) {
                    setStateNormal();
                }
                break;

            case States.ON_LEDGE:
                ledgeDetector.AdjustFacingToLedge();
                break;
        }


        animator.SetFloat("forward", moveMagnitude);
        animator.SetBool("grounded", grounded);
        animator.SetFloat("vertical_speed", rb.velocity.y);

        debugText.text = $"{state}";
    }

    private void FixedUpdate() {
        rb.position += rootMotionDelta;
        switch (state) {
            case States.NORMAL:
                if (grounded) {
                    newVelocity = forwardSpeed * goingForward * cameraForward + forwardSpeed * goingRight * cameraTransform.right;
                    newVelocity.y = rb.velocity.y;
                    rb.velocity = newVelocity + impulseVelocity;
                }
                break;
            case States.ON_LADDER:
                Vector3 newPos = rb.position + 2.0f * goingForward * Vector3.up * Time.fixedDeltaTime;
                if (newPos.y < currentLadder.TopY - 1.7f) {
                    rb.position = newPos;
                }
                break;


            case States.ON_LEDGE:
                newVelocity = ledgeSpeed * goingRight * modelTransform.right;
                newVelocity.y = rb.velocity.y;
                rb.velocity = newVelocity;
                if (grounded) {
                    hangCollider.enabled = false;
                    setStateNormal();
                    break;
                }
                if (goingForward > 0.9) {
                    hangCollider.enabled = false;
                    ledgeDetector.ClimbUp(rb);
                    setStateNormal();
                } else if (goingForward < 0) {
                    hangCollider.enabled = false;
                    setStateNormal();
                }

                animator.SetFloat("vertical_speed", rb.velocity.y);
                break;
        }
        impulseVelocity = Vector3.zero;
        rootMotionDelta = Vector3.zero;
    }

    public void setStateOnLadder() {
        state = States.ON_LADDER;
        rb.velocity = Vector3.zero;
    }
    public void setStateNormal() {
        state = States.NORMAL;
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        mainCollider.enabled = true;
        animator.SetBool("climb", false);
        animator.SetBool("on_ledge", false);
    }
    public void addRootMotionDelta(Vector3 delta) {
        if (state == States.NORMAL || state == States.ON_LADDER || state == States.ON_LEDGE) return;
        rootMotionDelta += delta;
    }
}
