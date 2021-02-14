﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotionController : MonoBehaviour {
    public float forwardSpeed = 5f;
    public float jumpSpeed = 8f;
    public float interactDetectDistance = 1.0f;
    public float ledgeSpeed = 3f;

    // user inputs
    bool jumpPending = false;

    // attributes
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
    Vector3 rootMotionDelta;
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
    PlayerInput input;
    new CameraController camera;


    void Awake() {
        input = FindObjectOfType<PlayerInput>();
        camera = FindObjectOfType<CameraController>();
        audioManager = FindObjectOfType<AudioManager>();

        rb = GetComponent<Rigidbody>();
        mainCollider = GetComponent<CapsuleCollider>();

        modelTransform = GameObject.Find("PlayerModel").transform;
        animator = modelTransform.GetComponent<Animator>();

        debugText = GameObject.Find("debugText").GetComponent<Text>();
        hangCollider = GameObject.Find("HangCollider").GetComponent<BoxCollider>();
        grabArmCollider = GameObject.Find("GrabArmCollider").GetComponent<BoxCollider>();
        interactHintText = GameObject.Find("InteractHint").GetComponent<Text>();

        groundDetector = new GroundDetector(transform);
        interactDetector = new InteractDetector(transform, modelTransform);
        ledgeDetector = new LedgeDetector(transform, modelTransform);
    }

    void Update() {
        grounded = groundDetector.IsOnGround();
        frontDetected = interactDetector.DetectFront(out hitInfo);
        FindInteractType();

        switch (state) {
            case States.NORMAL:
                if (grounded) {
                    if (input.moveMagnitude > 0.1) {
                        Vector3 targetDirection = input.goingForward * camera.forward + input.goingRight * camera.right;
                        modelTransform.forward = Vector3.Slerp(modelTransform.forward, targetDirection, 0.4f);
                        audioManager.PlayIfNotPlaying("walk");
                    } else {
                        audioManager.Stop("walk");
                    }

                    if (input.jumpPressed) {
                        jumpPending = true;
                        audioManager.Play("jump");
                    }
                } else {
                    if (input.grabBtnDown) {
                        animator.SetBool("on_ledge", true);
                        hangCollider.enabled = true;
                        grabArmCollider.enabled = true;
                        grabStuckSecondChance = 1;  // velocity becomes 0 once when jumping up 
                        state = States.GRAB;
                    }
                }

                if (input.interactBtnDown) {
                    if (interact == InteractType.Ladder) {
                        rb.useGravity = false;
                        animator.SetBool("climb", true);
                        currentLadder = hitInfo.collider.GetComponentInParent<Ladder>();
                        currentLadder.GetOntoLadder(transform, modelTransform, forwardSpeed);
                        state = States.LADDER_ENTER;
                    } else if (interact == InteractType.LedgeBelow) {
                        hangCollider.enabled = true;
                        grabArmCollider.enabled = true;
                        grounded = false;
                        animator.SetBool("on_ledge", true);
                        ledgeDetector.EnterLedge();
                        camera.ResetCamera(modelTransform.eulerAngles);
                        state = States.GRAB;
                    }
                }

                break;


            case States.ON_LADDER:
                if (input.cancelBtnDown || input.interactBtnDown) {
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
                if (input.grabBtnDown) {  // teleport up
                    transform.position += modelTransform.forward * ledgeDetector.hangOffsetZ * 2 + new Vector3(0, -ledgeDetector.hangOffsetY, 0);
                    SetStateNormal();
                } else if (input.goingForward < 0) {
                    SetStateNormal();
                }
                break;
        }


        animator.SetFloat("forward", input.moveMagnitude);
        animator.SetBool("grounded", grounded);
        animator.SetFloat("vertical_speed", rb.velocity.y);

        debugText.text = $"{state} {rb.velocity}";
    }

    private void FixedUpdate() {
        rb.position += rootMotionDelta;
        switch (state) {
            case States.NORMAL:
                if (grounded) {
                    newVelocity = forwardSpeed * input.goingForward * camera.forward + forwardSpeed * input.goingRight * camera.right;
                    newVelocity.y = rb.velocity.y;
                    if (jumpPending) {
                        newVelocity.y += jumpSpeed;
                        jumpPending = false;
                    }
                    rb.velocity = newVelocity;
                }
                break;
            case States.ON_LADDER:
                Vector3 newPos = rb.position + 2.0f * input.goingForward * Vector3.up * Time.fixedDeltaTime;
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
                newVelocity = ledgeSpeed * input.goingRight * modelTransform.right;
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
        if (state == States.NORMAL) {
            if (frontDetected) {
                if (hitInfo.collider.CompareTag("ladder")) {
                    interact = InteractType.Ladder;
                    interactHintText.text = "Press F to Climb Ladder";
                    return;
                }
            }
            if (frontDetected) {
                if (hitInfo.collider.CompareTag("DialogueTrigger")) {
                    interactHintText.text = "Press F to Interact";
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
            interactHintText.text = "Press R to Climb Up";
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