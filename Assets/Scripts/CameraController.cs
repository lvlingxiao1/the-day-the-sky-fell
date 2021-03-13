using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour {
    public float cameraSpeedMouseX = 180;
    public float cameraSpeedMouseY = 60;
    public float cameraSpeedKeyboardX = 180;
    public float cameraSpeedKeyboardY = 60;

    [Header("Camera Deadzone Settings")]
    public float deadZoneRadius;
    public int deadZoneSteps;

    PlayerInput input;
    Vector3 defaultLocalPosition;
    Vector3 defaultRotation;
    float restDistance;
    public Vector3 targetRotation;
    Vector3 nextRotation;
    int environment;
    SkinnedMeshRenderer player_renderer1;
    SkinnedMeshRenderer player_renderer2;
    SkinnedMeshRenderer player_renderer3;
    Transform mainCamera;
    Transform cameraHandle;

    [HideInInspector]
    public Vector3 forward;
    [HideInInspector]
    public Vector3 right;

    //Text debugText;

    void Awake() {
        input = FindObjectOfType<PlayerInput>();
        mainCamera = GameObject.Find("Main Camera").transform;
        cameraHandle = GameObject.Find("CameraHandle").transform;
        mainCamera.position = cameraHandle.position;
        mainCamera.rotation = cameraHandle.rotation;

        defaultLocalPosition = cameraHandle.localPosition;
        defaultRotation = transform.localEulerAngles;
        restDistance = defaultLocalPosition.magnitude;
        targetRotation = transform.eulerAngles;
        nextRotation = targetRotation;

        environment = LayerMask.GetMask("environment");
        player_renderer1 = GameObject.Find("BodyMesh").GetComponent<SkinnedMeshRenderer>();
        player_renderer2 = GameObject.Find("BackpackMesh").GetComponent<SkinnedMeshRenderer>();
        player_renderer3 = GameObject.Find("HairMesh").GetComponent<SkinnedMeshRenderer>();

        //debugText = GameObject.Find("debugText").GetComponent<Text>();
    }

    void FixedUpdate() {
        if (Mathf.Abs(input.cameraHorizontal) < 0.01 && Mathf.Abs(input.cameraVertical) < 0.01) {
            targetRotation.y += input.mouseMoveX * cameraSpeedMouseX * Time.fixedDeltaTime;
            targetRotation.x -= input.mouseMoveY * cameraSpeedMouseY * Time.fixedDeltaTime;
        } else {
            targetRotation.y += input.cameraHorizontal * cameraSpeedKeyboardX * Time.fixedDeltaTime;
            targetRotation.x -= input.cameraVertical * cameraSpeedKeyboardY * Time.fixedDeltaTime;
        }
        targetRotation.x = Mathf.Clamp(targetRotation.x, -40, 70);
        transform.eulerAngles = targetRotation;

<<<<<<< HEAD
        CollissionTest();
=======
        targetLocalPosition = defaultLocalPosition;

        collissionTest();
>>>>>>> master

        ComputeCameraForward();
        right = transform.right;

        mainCamera.position = Vector3.Lerp(mainCamera.position, cameraHandle.position, 10 * Time.fixedDeltaTime);
        nextRotation = Vector3.Slerp(nextRotation, targetRotation, 10 * Time.fixedDeltaTime);
        mainCamera.eulerAngles = nextRotation;

        // // without transition
        //mainCamera.position = cameraHandle.position;
        //mainCamera.eulerAngles = targetRotation;

        //debugText.text = $"{transform.rotation * cameraRestPosition} {transform.position} {restDistance} {hit}";
    }

    void CollissionTest() {
        Debug.DrawRay(transform.position, transform.rotation * defaultLocalPosition, Color.cyan);
        bool hit = Physics.Raycast(transform.position, transform.rotation * defaultLocalPosition,
            out RaycastHit hitInfo, restDistance, environment);

        int maxSteps = hit ? Mathf.FloorToInt(hitInfo.distance / restDistance * deadZoneSteps) : deadZoneSteps;
        float stepSize = 1f / deadZoneSteps;

        // Each zoom segment or zoom step
        Vector3 newCamWorldPosition = Vector3.zero;
        Vector3[] deadZoneDirs = { transform.right, -transform.right }; // Seems we don't have issues with cliping into terrain for up and down, so I removed those 2 directions for performance gain
        for (int i = maxSteps; i >= 0; i--) {
            newCamWorldPosition = Vector3.Lerp(transform.position,
                transform.TransformPoint(defaultLocalPosition), i * stepSize);
            Debug.DrawLine(transform.TransformPoint(cameraHandle.localPosition), newCamWorldPosition, Color.red);

            bool ok = true;
            foreach (Vector3 dir in deadZoneDirs) {
                if (Physics.Raycast(newCamWorldPosition, dir, deadZoneRadius, environment)) {
                    ok = false;
                    break;
                }
            }
            if (ok) break;
        }

        cameraHandle.position = Vector3.Lerp(cameraHandle.position, newCamWorldPosition, 15 * Time.fixedDeltaTime);

        if (Vector3.SqrMagnitude(newCamWorldPosition - transform.position) < 0.16f) {
            player_renderer1.enabled = false;
            player_renderer2.enabled = false;
            player_renderer3.enabled = false;
        } else {
            player_renderer1.enabled = true;
            player_renderer2.enabled = true;
            player_renderer3.enabled = true;
        }
    }

    public void ResetCamera(Vector3 modelRotation) {
        targetRotation = modelRotation + defaultRotation;
        transform.eulerAngles = targetRotation;
        ComputeCameraForward();
    }

    void ComputeCameraForward() {
        forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
    }
}
