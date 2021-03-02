using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Climbing;

public class MotionController : MonoBehaviour {
    public float forwardSpeed = 5f;
    public float jumpSpeed = 8f;
    public float interactDetectDistance = 1.0f;
    public float ledgeSpeed = 3f;
    public float climbGridSpeed = 3f;

    public string defaultCheckpoint;

    // user inputs
    bool jumpPending = false;

    // attributes
    public bool grounded;
    bool prevGrounded = true;
    bool frontDetected;
    float speedFactor;
    int grabStuckSecondChance;
    enum States {
        Normal,
        LadderEnter,
        OnLadder,
        LadderExit,
        Grab,
        GrabStable,
        LedgeClimbUp,
        OnClimbGrid,
        Dialogue
    }
    States state = States.Normal;
    Ladder currentLadder;
    Vector3 newVelocity;
    Vector3 rootMotionDelta;
    enum InteractType {
        None,
        Ladder,
        LedgeBelow,
        LedgeAbove,
        Dialogue,
        Checkpoint
    }
    InteractType interact;
    RaycastHit hitInfo;

    // components
    Rigidbody rb;
    Animator animator;
    AudioManager audioManager;
    Text debugText;
    [HideInInspector] public Transform modelTransform;
    GroundDetector groundDetector;
    InteractDetector interactDetector;
    LedgeDetector ledgeDetector;
    BoxCollider hangCollider;
    BoxCollider grabArmCollider;
    CapsuleCollider mainCollider;
    Text interactHintText;
    PlayerInput input;
    new CameraController camera;
    CheckpointManager currentCheckpoint;
    ClimbController climbController;
    DialogueManager dialogueManager;


    void Awake() {
        input = GetComponent<PlayerInput>();
        camera = FindObjectOfType<CameraController>();
        audioManager = FindObjectOfType<AudioManager>();

        rb = GetComponent<Rigidbody>();
        mainCollider = GetComponent<CapsuleCollider>();

        modelTransform = GameObject.Find("PlayerModel").transform;
        animator = modelTransform.GetComponent<Animator>();

        //debugText = GameObject.Find("debugText").GetComponent<Text>();
        hangCollider = GameObject.Find("HangCollider").GetComponent<BoxCollider>();
        grabArmCollider = GameObject.Find("GrabArmCollider").GetComponent<BoxCollider>();
        interactHintText = GameObject.Find("InteractHint").GetComponent<Text>();

        groundDetector = new GroundDetector(transform);
        interactDetector = new InteractDetector(transform, modelTransform);
        ledgeDetector = new LedgeDetector(transform, modelTransform);

        climbController = GetComponent<ClimbController>();

        dialogueManager = FindObjectOfType<DialogueManager>();

        currentCheckpoint = GameObject.Find(defaultCheckpoint).GetComponent<CheckpointManager>();
    }

    void Update() {
        grounded = groundDetector.IsOnGround();
        if (grounded && !prevGrounded) audioManager.Play("footstep");
        prevGrounded = grounded;
        frontDetected = interactDetector.DetectFront(out hitInfo);
        FindInteractType();

        switch (state) {
            case States.Normal:
                if (grounded) {
                    if (input.slowBtnHold) {
                        speedFactor = Mathf.Lerp(speedFactor, 1, 0.3f);
                    } else {
                        speedFactor = Mathf.Lerp(speedFactor, 2, 0.3f);
                    }

                    if (input.moveMagnitude > 0.1) {
                        Vector3 targetDirection = input.goingForward * camera.forward + input.goingRight * camera.right;
                        modelTransform.forward = Vector3.Slerp(modelTransform.forward, targetDirection, 0.4f);
                        //audioManager.PlayIfNotPlaying("walk");
                    } else {
                        //audioManager.Stop("walk");
                    }

                    if (input.jumpPressed) {
                        jumpPending = true;
                        //audioManager.Play("jump");
                        audioManager.Play("footstep");
                    }
                } else {
                    audioManager.Stop("walk");
                    if (input.grabBtnDown) {
                        animator.SetBool("on_ledge", true);
                        hangCollider.enabled = true;
                        grabArmCollider.enabled = true;
                        grabStuckSecondChance = 1;  // velocity becomes 0 once when jumping up 
                        state = States.Grab;
                    }
                }

                if (input.interactBtnDown) {
                    if (interact == InteractType.Ladder) {
                        rb.useGravity = false;
                        animator.SetBool("climb", true);
                        currentLadder = hitInfo.collider.GetComponentInParent<Ladder>();
                        currentLadder.GetOntoLadder(transform, modelTransform, forwardSpeed);
                        state = States.LadderEnter;
                    } else if (interact == InteractType.LedgeBelow) {
                        hangCollider.enabled = true;
                        grabArmCollider.enabled = true;
                        grounded = false;
                        animator.SetBool("on_ledge", true);
                        ledgeDetector.EnterLedge();
                        camera.ResetCamera(modelTransform.eulerAngles);
                        state = States.Grab;
                    } else if (interact == InteractType.Dialogue) {
                        // trigger dialogue
                        DialogueTrigger trigger = hitInfo.collider.GetComponentInParent<DialogueTrigger>();
                        trigger.TriggerDialogue();
                        rb.isKinematic = true;
                        state = States.Dialogue;
                    } else if (interact == InteractType.Checkpoint) {
                        currentCheckpoint = hitInfo.collider.GetComponentInParent<CheckpointManager>();
                        currentCheckpoint.TriggerDialogue(dialogueManager);
                        rb.isKinematic = true;
                        state = States.Dialogue;
                    }
                }

                break;


            case States.OnLadder:
                if (input.releaseBtnDown || input.interactBtnDown) {
                    SetStateNormal();
                }
                if (transform.position.y >= currentLadder.TopY - 1.70f) {
                    transform.position = new Vector3(transform.position.x, currentLadder.TopY - 1.65f, transform.position.z);
                    state = States.LadderExit;
                    animator.SetTrigger("ladder_top");
                } else if (grounded) {
                    SetStateNormal();
                }
                break;

            case States.GrabStable:
                if (Mathf.Abs(rb.velocity.y) > 0.1) {
                    state = States.Grab;
                    break;
                }
                ledgeDetector.AdjustFacingToLedge();
                if (input.grabBtnDown) {  // teleport up
                    transform.position += modelTransform.forward * ledgeDetector.hangOffsetZ * 2 + new Vector3(0, -ledgeDetector.hangOffsetY, 0);
                    SetStateNormal();
                } else if (input.releaseBtnDown || input.goingForward < 0) {
                    SetStateNormal();
                }
                break;

            case States.OnClimbGrid:
                if (input.slowBtnHold) {
                    climbController.speed_linear = Mathf.Lerp(climbController.speed_linear, climbGridSpeed, 0.3f);
                } else {
                    climbController.speed_linear = Mathf.Lerp(climbController.speed_linear, climbGridSpeed * 2, 0.3f);
                }
                break;

            case States.Dialogue:
                if (input.jumpPressed || Input.GetMouseButtonDown(0)) {
                    dialogueManager.DisplayNextSentence();
                }
                input.moveMagnitude = 0;
                break;
        }


        animator.SetFloat("forward", input.moveMagnitude * speedFactor);
        animator.SetBool("grounded", grounded);
        animator.SetFloat("vertical_speed", rb.velocity.y);

        if (transform.position.y < -80) {
            currentCheckpoint.Respawn(this, dialogueManager);
        }

        //debugText.text = $"{state} {rb.velocity}";
    }

    private void FixedUpdate() {
        rb.position += rootMotionDelta;
        switch (state) {
            case States.Normal:
                if (grounded) {
                    newVelocity = (forwardSpeed * input.goingForward * camera.forward + forwardSpeed * input.goingRight * camera.right) * speedFactor;
                    newVelocity.y = rb.velocity.y;
                    if (jumpPending) {
                        newVelocity.y += jumpSpeed;
                        jumpPending = false;
                    }
                    rb.velocity = newVelocity;
                }
                break;
            case States.OnLadder:
                Vector3 newPos = rb.position + 2.0f * input.goingForward * Vector3.up * Time.fixedDeltaTime;
                if (newPos.y < currentLadder.TopY - 1.65f) {
                    rb.position = newPos;
                }
                break;

            case States.Grab:
                if (Mathf.Abs(rb.velocity.y) < 1e-4) {
                    if (ledgeDetector.AdjustFacingToLedge())
                        state = States.GrabStable;
                    else if (grabStuckSecondChance > 0) {
                        grabStuckSecondChance--;
                    } else {
                        SetStateNormal();
                    }
                } else if (grounded) {
                    SetStateNormal();
                }
                break;

            case States.GrabStable:
                if ((input.goingRight > 0 && ledgeDetector.CanMoveRight())
                    || (input.goingRight < 0 && ledgeDetector.CanMoveLeft())) {
                    newVelocity = ledgeSpeed * input.goingRight * modelTransform.right;
                } else {
                    newVelocity = Vector3.zero;
                }
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

    void FindInteractType() {
        interact = InteractType.None;
        interactHintText.text = "";
        if (state == States.Normal) {
            if (frontDetected) {
                if (hitInfo.collider.CompareTag("ladder")) {
                    interact = InteractType.Ladder;
                    interactHintText.text = "Press F to Climb Ladder";
                    return;
                } else if (hitInfo.collider.CompareTag("DialogueTrigger")) {
                    interact = InteractType.Dialogue;
                    interactHintText.text = "Press F to Talk";
                    return;
                } else if (hitInfo.collider.CompareTag("checkpoint")) {
                    interact = InteractType.Checkpoint;
                    interactHintText.text = "Press F to Take a Break at Checkpoint";
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
        } else if (state == States.GrabStable) {
            interactHintText.text = "Press R to Climb Up, X to Drop Down";
        } else if (state == States.OnClimbGrid) {
            interactHintText.text = "Press X to Drop Down";
        }
    }

    public void SetStateOnLadder() {
        state = States.OnLadder;
        rb.velocity = Vector3.zero;
    }
    public void SetStateNormal() {
        state = States.Normal;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        mainCollider.enabled = true;
        hangCollider.enabled = false;
        grabArmCollider.enabled = false;
        animator.SetBool("climb", false);
        animator.SetBool("on_ledge", false);
    }
    public void AddRootMotionDelta(Vector3 delta) {
        if (state != States.LadderEnter && state != States.LadderExit && state != States.LedgeClimbUp) return;
        rootMotionDelta += delta;
    }

    public void SetStateOnClimbGrid() {
        rb.isKinematic = true;
        rb.useGravity = false;
        mainCollider.enabled = false;
        hangCollider.enabled = false;
        grabArmCollider.enabled = false;
        state = States.OnClimbGrid;
    }

    public bool IsInGrabState() {
        return state == States.Grab;
    }
}
