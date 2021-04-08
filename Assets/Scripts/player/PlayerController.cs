using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Climbing;
using TMPro;

public class PlayerController : MonoBehaviour {
    public float forwardSpeed = 5f;
    public float jumpSpeed = 8f;
    public float interactDetectDistance = 1.0f;
    public float ledgeSpeed = 3f;
    public float climbGridSpeed = 3f;
    public int livesMax = 0;
    public int lives = 0;
    public Vector3 lastSafePosition;

    public string defaultCheckpoint;

    // user inputs
    bool jumpPending = false;
    bool jumped = false;
    bool autoClimbDown = false;

    // attributes
    public bool grounded;
    int inAir;
    bool onTelephoneWire;
    bool prevGrounded = true;
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
        Dialogue,
        Checkpoint,
        Item
    }
    InteractType interact;
    RaycastHit hitInfo;
    bool ledgeBelow = false;

    // components
    Rigidbody rb;
    Animator animator;
    AudioManager audioManager;
    Text debugText;
    [HideInInspector] public Transform modelTransform;
    GroundDetector groundDetector;
    InteractDetector interactDetector;
    LedgeDetector ledgeDetector;
    //BoxCollider hangCollider;
    //BoxCollider grabArmCollider;
    DelayedCollider hangCollider;
    DelayedCollider grabArmCollider;
    CapsuleCollider mainCollider;
    Text interactHintText;
    PlayerInput input;
    new CameraController camera;
    CheckpointManager currentCheckpoint;
    ClimbController climbController;
    DialogueManager dialogueManager;
    public LivesUI livesUI;


    void Awake() {
        input = GetComponent<PlayerInput>();
        camera = FindObjectOfType<CameraController>();
        audioManager = FindObjectOfType<AudioManager>();

        rb = GetComponent<Rigidbody>();
        mainCollider = GetComponent<CapsuleCollider>();

        modelTransform = GameObject.Find("PlayerModel").transform;
        animator = modelTransform.GetComponent<Animator>();

        debugText = GameObject.Find("debugText").GetComponent<Text>();
        hangCollider = GameObject.Find("HangCollider").GetComponent<DelayedCollider>();
        grabArmCollider = GameObject.Find("GrabArmCollider").GetComponent<DelayedCollider>();
        interactHintText = GameObject.Find("InteractHint").GetComponent<Text>();

        groundDetector = new GroundDetector(transform);
        interactDetector = new InteractDetector(transform, modelTransform);
        ledgeDetector = new LedgeDetector(transform, modelTransform);

        climbController = GetComponent<ClimbController>();

        dialogueManager = FindObjectOfType<DialogueManager>();

        currentCheckpoint = GameObject.Find(defaultCheckpoint).GetComponent<CheckpointManager>();

        livesUI = new LivesUI(lives);
    }

    void Update() {
        if (!UIController.gameStarted) return;

        grounded = groundDetector.IsOnGround();
        if (grounded && !prevGrounded) {
            audioManager.Play("footstep");
            autoClimbDown = true;
        }
        prevGrounded = grounded;
        if (grounded) {
            inAir = 0;
            jumped = false;
        } else inAir++;

        FindInteractType();

        switch (state) {
            case States.Normal:
                ledgeBelow = ledgeDetector.DetectLedgeBelow();
                if (grounded) {
                    lastSafePosition = transform.position;
                    if (input.runBtnHold) {
                        speedFactor = Mathf.Lerp(speedFactor, 1.5f, 18 * Time.deltaTime);
                        autoClimbDown = false;
                    } else {
                        speedFactor = Mathf.Lerp(speedFactor, 1, 18 * Time.deltaTime);
                        autoClimbDown = true;
                    }

                    if (input.moveMagnitude > 0.1) {
                        Vector3 targetDirection = input.goingForward * camera.forward + input.goingRight * camera.right;
                        modelTransform.forward = Vector3.Slerp(modelTransform.forward, targetDirection, 24 * Time.deltaTime);
                    }
                } else {
                    //audioManager.Stop("walk");
                    if (autoClimbDown && ledgeBelow) {
                        if (ledgeDetector.EnterLedge()) {
                            SetStateGrab();
                            rb.velocity = new Vector3(0, 0, 0);
                            camera.targetRotation.x = 70;
                            input.LockInputForSeconds(0.5f);
                            break;
                        }
                    }
                    if (input.moveMagnitude > 0.1) {
                        Vector3 targetDirection = input.goingForward * camera.forward + input.goingRight * camera.right;
                        modelTransform.forward = Vector3.Slerp(modelTransform.forward, targetDirection, 6 * Time.deltaTime);
                    }
                    if (input.grabBtnDown) {
                        animator.SetBool("on_ledge", true);
                        hangCollider.TurnOnCollider();
                        grabArmCollider.TurnOnCollider();
                        grabStuckSecondChance = 5;  // velocity becomes 0 once when jumping up 
                        state = States.Grab;
                    }
                }

                if (input.jumpPressed && inAir < 6 && !jumpPending && !jumped) {      // left ground for less than 0.1 seconds
                    jumpPending = true;
                    audioManager.Play($"jump{Random.Range(1, 3)}");
                }

                if (input.interactBtnDown) {
                    if (interact == InteractType.Ladder) {
                        rb.useGravity = false;
                        animator.SetBool("climb", true);
                        currentLadder = hitInfo.collider.GetComponentInParent<Ladder>();
                        currentLadder.GetOntoLadder(transform, modelTransform, forwardSpeed);
                        state = States.LadderEnter;
                    } else if (interact == InteractType.Dialogue) {
                        // trigger dialogue
                        DialogueTrigger trigger = hitInfo.collider.GetComponentInParent<DialogueTrigger>();
                        trigger.TriggerDialogue();
                        rb.isKinematic = true;
                        state = States.Dialogue;
                    } else if (interact == InteractType.Checkpoint) {
                        currentCheckpoint = hitInfo.collider.GetComponentInParent<CheckpointManager>();
                        currentCheckpoint.HandleInteract(this, dialogueManager);
                        rb.isKinematic = true;
                        state = States.Dialogue;
                    } else if (interact == InteractType.Item) {
                        hitInfo.collider.GetComponent<IItem>().PickUp();
                    }
                }

                break;


            case States.OnLadder:
                if (input.interactBtnDown) {
                    autoClimbDown = false;
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
                if (Mathf.Abs(rb.velocity.y) > 0.5f) {
                    state = States.Grab;
                    break;
                }
                ledgeDetector.AdjustFacingToLedge(out onTelephoneWire);
                if (input.runBtnHold) {
                    speedFactor = Mathf.Lerp(speedFactor, 1.2f, 0.3f);
                } else {
                    speedFactor = Mathf.Lerp(speedFactor, 1, 0.3f);
                }
                if (input.goingForward > 0) {  // teleport up
                    //rb.isKinematic = true;
                    //animator.SetTrigger("ledge_climb_up");
                    ledgeDetector.ClimbUpLedge(onTelephoneWire);
                    SetStateNormal();
                    camera.ResetVertical();
                    input.LockInputForSeconds(0.5f);
                } else if (input.goingForward < 0) {
                    SetStateNormal();
                    audioManager.Play($"land{Random.Range(1, 3)}");
                    input.LockInputForSeconds(0.5f);
                }
                animator.SetFloat("ledge_speed", input.goingRight * speedFactor);
                break;

            case States.Grab:
                animator.SetFloat("ledge_speed", 0);
                break;

            case States.OnClimbGrid:
                if (input.runBtnHold) {
                    climbController.speed_linear = Mathf.Lerp(climbController.speed_linear, climbGridSpeed * 1.5f, 18 * Time.deltaTime);
                } else {
                    climbController.speed_linear = Mathf.Lerp(climbController.speed_linear, climbGridSpeed, 18 * Time.deltaTime);
                }
                break;

            case States.Dialogue:
                if (input.jumpPressed || Input.GetMouseButtonDown(0)) {
                    dialogueManager.DisplayNextSentence();
                }
                input.moveMagnitude = 0;
                break;
        }


        animator.SetFloat("ground_speed", input.moveMagnitude * speedFactor);
        animator.SetBool("grounded", grounded);
        animator.SetFloat("vertical_speed", rb.velocity.y);

        if (transform.position.y < -80) {
            currentCheckpoint.Respawn(this);
        }

        debugText.text = $"{state} {rb.velocity}";



        /////////// CHEATS /////////////
        if (Input.GetKeyDown(KeyCode.Keypad1)) {
            transform.position = new Vector3(-6.139141f, -18.53121f, -102.8717f);
            SetStateNormal();
        } else if (Input.GetKeyDown(KeyCode.Keypad2)) {
            transform.position = new Vector3(110.465f, -13.794f, -282.967f);
            SetStateNormal();
        } else if (Input.GetKeyDown(KeyCode.Keypad3)) {
            transform.position = new Vector3(239.36f, -32.12f, -393.23f);
            SetStateNormal();
        } else if (Input.GetKeyDown(KeyCode.Keypad4)) {
            transform.position = new Vector3(319.6365f, -16.09863f, -437.5951f);
            SetStateNormal();
        } else if (Input.GetKeyDown(KeyCode.Keypad5)) {
            transform.position = new Vector3(468.11f, -3.24f, -451.02f);
            SetStateNormal();
        } else if (Input.GetKeyDown(KeyCode.Keypad6)) {
            transform.position = new Vector3(498.3731f, -1.98288f, -475.4491f);
            SetStateNormal();
        } else if (Input.GetKeyDown(KeyCode.Keypad7)) {
            transform.position = new Vector3(532.3205f, -7.150647f, -504.4659f);
            SetStateNormal();
        } else if (Input.GetKeyDown(KeyCode.Keypad8)) {
            transform.position = new Vector3(564.0289f, -17.41118f, -551.7151f);
            SetStateNormal();
        } else if (Input.GetKeyDown(KeyCode.Keypad9)) {
            transform.position = new Vector3(574.7895f, -20.05804f, -564.5358f);
            SetStateNormal();
        } else if (Input.GetKeyDown(KeyCode.KeypadMultiply)) {
            var mmc = FindObjectOfType<MusicMenuController>();
            for (int i = 0; i < mmc.musicItems.Length; i++) {
                if (!mmc.musicItems[i].collected) { mmc.AddToCollection(i); break; }
            }
        } else if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
            livesMax++;
        } else if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
            livesMax--;
        } else if (Input.GetKeyDown(KeyCode.KeypadPeriod)) {
            lives = 9999999;
        }
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
                        autoClimbDown = false;
                        jumped = true;
                    }
                    rb.velocity = newVelocity;
                } else {
                    newVelocity = forwardSpeed * input.moveMagnitude * modelTransform.forward * speedFactor;
                    newVelocity = Vector3.Lerp(rb.velocity, newVelocity, 5 * Time.fixedDeltaTime);
                    newVelocity.y = rb.velocity.y;
                    if (jumpPending) {
                        inAir += 999;
                        newVelocity.y += jumpSpeed;
                        jumpPending = false;
                        autoClimbDown = false;
                        jumped = true;
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
                bool detected = ledgeDetector.AdjustFacingToLedge(out onTelephoneWire);
                if (Mathf.Abs(rb.velocity.y) < 5e-4) {
                    if (detected) {
                        input.LockInputForSeconds(0.5f);
                        state = States.GrabStable;
                        autoClimbDown = true;
                    } else if (grabStuckSecondChance > 0) {
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
                    newVelocity = ledgeSpeed * input.goingRight * modelTransform.right * speedFactor;
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
        bool frontDetected = interactDetector.DetectFront(out hitInfo);
        interact = InteractType.None;
        interactHintText.text = "";
        if (state == States.Normal) {
            if (frontDetected) {
                if (hitInfo.collider.CompareTag("ladder")) {
                    interact = InteractType.Ladder;
                    interactHintText.text = "Press [E] to Climb Ladder";
                    return;
                } else if (grounded && hitInfo.collider.CompareTag("DialogueTrigger")) {
                    interact = InteractType.Dialogue;
                    interactHintText.text = "Press [E] to Talk";
                    return;
                } else if (grounded && hitInfo.collider.CompareTag("checkpoint")) {
                    interact = InteractType.Checkpoint;
                    interactHintText.text = "Press [E] to Take a Break at Checkpoint";
                    return;
                } else if (hitInfo.collider.CompareTag("Item") && !hitInfo.collider.GetComponent<IItem>().IsPickedUp()) {
                    interact = InteractType.Item;
                    interactHintText.text = hitInfo.collider.GetComponent<IItem>().GetInteractMessage();
                }
            }
        } else if (state == States.GrabStable) {
            interactHintText.text = "Press [W] to Climb Up\nPress [S] to Drop Down";
        }
    }

    public void SetStateGrab() {
        animator.SetBool("on_ledge", true);
        grabStuckSecondChance = 5;  // velocity becomes 0 once when jumping up 
        grounded = false;
        state = States.Grab;
        hangCollider.TurnOnCollider();
        grabArmCollider.TurnOnCollider();
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
        hangCollider.TurnOffCollider();
        grabArmCollider.TurnOffCollider();
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
        hangCollider.TurnOffCollider();
        grabArmCollider.TurnOffCollider();
        state = States.OnClimbGrid;
    }

    public bool IsInGrabState() {
        return state == States.Grab || state == States.GrabStable;
    }

    public bool ShouldRotateCamera() {
        return state != States.Grab && state != States.GrabStable && state != States.OnClimbGrid;
    }
}
