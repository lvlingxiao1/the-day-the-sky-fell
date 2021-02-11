using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotionController : MonoBehaviour {
    public float forwardSpeed = 5f;
    public float jumpSpeed = 8f;
    public float cameraSpeedX = 120f;
    public float cameraSpeedY = 60f;
    public float interactDetectDistance = 1.0f;
    public float ledgeSpeed = 3f;
    public bool inputEnabled = true;

    // user inputs
    float goingForward;
    float goingRight;
    bool jumpPressed;
    bool jumpPending = false;
    bool interactBtnDown;
    bool grabBtnDown;
    bool cancelBtnDown;

    // attributes
    Vector3 cameraRotation;
    float moveMagnitude;
    bool grounded;
    bool frontDetected;
    int grabStuckSecondChance;
    enum States {
        NORMAL,
        LADDER_ENTER,
        ON_LADDER,
        LADDER_EXIT,
        GRAB,
        GRAB_STABLE,
        LEDGE_CLIMB_UP
    }
    States state = States.NORMAL;
    Ladder currentLadder;
    Vector3 newVelocity;
    Vector3 cameraForward;
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
    BoxCollider grabArmCollider;
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
        groundDetector = new GroundDetector(transform);
        interactDetector = new InteractDetector(transform, modelTransform);
        ledgeDetector = new LedgeDetector(transform, modelTransform);
        hangCollider = GameObject.Find("HangCollider").GetComponent<BoxCollider>();
        grabArmCollider = GameObject.Find("GrabArmCollider").GetComponent<BoxCollider>();
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
            grabBtnDown = Input.GetButtonDown("Grab");

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
            ComputeCameraForward();
        }


        grounded = groundDetector.IsOnGround();
        frontDetected = interactDetector.DetectFront(out hitInfo);
        FindInteractType();

        switch (state) {
            case States.NORMAL:
                if (grounded) {
                    if (moveMagnitude > 0.1) {
                        Vector3 targetDirection = goingForward * cameraForward + goingRight * cameraTransform.right;
                        modelTransform.forward = Vector3.Slerp(modelTransform.forward, targetDirection, 0.4f);
                        audioManager.PlayIfNotPlaying("walk");
                    } else {
                        audioManager.Stop("walk");
                    }

                    if (jumpPressed) {
                        jumpPending = true;
                        audioManager.Play("jump");
                    }
                } else {
                    if (grabBtnDown) {
                        animator.SetBool("on_ledge", true);
                        hangCollider.enabled = true;
                        grabArmCollider.enabled = true;
                        grabStuckSecondChance = 1;  // velocity becomes 0 once when jumping up 
                        state = States.GRAB;
                    }
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
                        grabArmCollider.enabled = true;
                        grounded = false;
                        animator.SetBool("on_ledge", true);
                        ledgeDetector.EnterLedge();
                        ResetCamera();
                        state = States.GRAB;
                    }
                }

                break;


            case States.ON_LADDER:
                if (cancelBtnDown || interactBtnDown) {
                    state = States.NORMAL;
                    rb.useGravity = true;
                    animator.SetBool("climb", false);
                }
                if (transform.position.y >= currentLadder.TopY - 1.70f) {
                    transform.position = new Vector3(transform.position.x, currentLadder.TopY - 1.65f, transform.position.z);
                    state = States.LADDER_EXIT;
                    animator.SetTrigger("ladder_top");
                } else if (grounded) {
                    SetStateNormal();
                }
                break;

            case States.GRAB_STABLE:
                if (Mathf.Abs(rb.velocity.y) > 0.1) {
                    state = States.GRAB;
                    break;
                }
                ledgeDetector.AdjustFacingToLedge();
                if (grabBtnDown) {
                    transform.position += modelTransform.forward * ledgeDetector.hangOffsetZ * 2 + new Vector3(0, -ledgeDetector.hangOffsetY, 0);
                    SetStateNormal();
                } else if (goingForward < 0) {
                    SetStateNormal();
                }
                break;
        }


        animator.SetFloat("forward", moveMagnitude);
        animator.SetBool("grounded", grounded);
        animator.SetFloat("vertical_speed", rb.velocity.y);

        debugText.text = $"{state} {rb.velocity}";
    }

    private void FixedUpdate() {
        rb.position += rootMotionDelta;
        switch (state) {
            case States.NORMAL:
                if (grounded) {
                    newVelocity = forwardSpeed * goingForward * cameraForward + forwardSpeed * goingRight * cameraTransform.right;
                    newVelocity.y = rb.velocity.y;
                    if (jumpPending) {
                        newVelocity.y += jumpSpeed;
                        jumpPending = false;
                    }
                    rb.velocity = newVelocity;
                }
                break;
            case States.ON_LADDER:
                Vector3 newPos = rb.position + 2.0f * goingForward * Vector3.up * Time.fixedDeltaTime;
                if (newPos.y < currentLadder.TopY - 1.65f) {
                    rb.position = newPos;
                }
                break;

            case States.GRAB:
                if (Mathf.Abs(rb.velocity.y) < 1e-4) {
                    if (ledgeDetector.AdjustFacingToLedge())
                        state = States.GRAB_STABLE;
                    else if (grabStuckSecondChance > 0) {
                        grabStuckSecondChance--;
                    } else {
                        SetStateNormal();
                    }
                } else if (grounded) {
                    SetStateNormal();
                }
                break;

            case States.GRAB_STABLE:
                newVelocity = ledgeSpeed * goingRight * modelTransform.right;
                newVelocity.y = rb.velocity.y;
                rb.velocity = newVelocity;
                if (grounded) {
                    SetStateNormal();
                    break;
                }

                animator.SetFloat("vertical_speed", rb.velocity.y);
                break;
        }
        rootMotionDelta = Vector3.zero;
    }

    public void ResetCamera() {
        cameraRotation = modelTransform.eulerAngles + new Vector3(17, 0, 0);
        cameraTransform.eulerAngles = cameraRotation;
        ComputeCameraForward();
    }

    public void ComputeCameraForward() {
        cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
    }

    void FindInteractType() {
        interact = InteractType.None;
        interactHintText.text = "";
        if (state == States.NORMAL) {
            if (frontDetected) {
                if (hitInfo.collider.CompareTag("ladder")) {
                    interact = InteractType.Ladder;
                    interactHintText.text = "Press F to Climb Ladder";
                    return;
                }
            }
            if (grounded) {
                if (!frontDetected) {
                    if (interactDetector.DetectLedgeBelow()) {
                        interact = InteractType.LedgeBelow;
                        interactHintText.text = "Press F to Climb Ledge Below";
                    }
                }
            } else {    // not on ground
                //if (interactDetector.DetectLedgeAbove()) {
                //    interact = InteractType.LedgeAbove;
                //    interactHintText.text = "Press F to Climb Ledge Above";
                //}
            }
        } else if (state == States.GRAB_STABLE) {
            interactHintText.text = "Press Q to Climb Up";
        }
    }

    public void SetStateOnLadder() {
        state = States.ON_LADDER;
        rb.velocity = Vector3.zero;
    }
    public void SetStateNormal() {
        state = States.NORMAL;
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        mainCollider.enabled = true;
        hangCollider.enabled = false;
        grabArmCollider.enabled = false;
        animator.SetBool("climb", false);
        animator.SetBool("on_ledge", false);
    }
    public void AddRootMotionDelta(Vector3 delta) {
        if (state == States.NORMAL || state == States.ON_LADDER || state == States.GRAB) return;
        rootMotionDelta += delta;
    }
}
