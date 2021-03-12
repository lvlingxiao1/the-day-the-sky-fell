using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour {
    public float cameraSpeedMouseX = 180;
    public float cameraSpeedMouseY = 60;
    public float cameraSpeedKeyboardX = 180;
    public float cameraSpeedKeyboardY = 60;
    public float peakOffset;

    [Header("Camera Deadzone Settings")]
    public float deadZoneRadius;
    public int deadZoneSteps;

    PlayerInput input;
    Vector3 defaultLocalPosition;
    Vector3 defaultRotation;
    Vector3 targetLocalPosition;
    Vector3 targetRotation;
    Vector3 nextRotation;
    float localDistance;
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

    readonly Vector3[] deadZoneDirs = {Vector3.left, Vector3.right}; // Seems we don't have issues with cliping into terrain for up and down, so I removed those 2 directions for performance gain

    //Text debugText;

    void Awake() {
        input = FindObjectOfType<PlayerInput>();
        mainCamera = GameObject.Find("Main Camera").transform;
        cameraHandle = GameObject.Find("CameraHandle").transform;
        mainCamera.position = cameraHandle.position;
        mainCamera.rotation = cameraHandle.rotation;

        defaultLocalPosition = cameraHandle.localPosition;
        defaultRotation = transform.localEulerAngles;
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

        targetLocalPosition = defaultLocalPosition + Vector3.right * peakOffset * input.peakRight;

        CollissionTest(deadZoneSteps);

        ComputeCameraForward();
        right = transform.right;

        mainCamera.position = Vector3.Lerp(mainCamera.position, cameraHandle.position, 0.2f);
        nextRotation = Vector3.Slerp(nextRotation, targetRotation, 0.2f);
        mainCamera.eulerAngles = nextRotation;

        // // without transition
        //mainCamera.position = cameraHandle.position;
        //mainCamera.eulerAngles = targetRotation;

        //debugText.text = $"{transform.rotation * cameraRestPosition} {transform.position} {restDistance} {hit}";
    }

    void CollissionTest(int deadZoneSteps) {
        localDistance = targetLocalPosition.magnitude;

        Debug.DrawRay(transform.position, transform.rotation * targetLocalPosition, Color.cyan);
        bool zoomHit = Physics.Raycast(transform.position, transform.rotation * targetLocalPosition,
            out RaycastHit zoomHitInfo, localDistance, environment);

        float stepSize = localDistance / deadZoneSteps;
        int maxStep = zoomHit ? Mathf.FloorToInt(zoomHitInfo.distance / localDistance * deadZoneSteps) : deadZoneSteps;

        // Each zoom segment or zoom step
        Vector3 newCamWorldPosition = transform.TransformPoint(Vector3.Lerp(cameraHandle.localPosition, targetLocalPosition, 0.3f));
        for (int i = maxStep; i >= 0; i--)
        {
            newCamWorldPosition = Vector3.Lerp(transform.position,
                transform.TransformPoint(targetLocalPosition), i / (float) deadZoneSteps - 0.05f);
            Debug.DrawLine(transform.TransformPoint(cameraHandle.localPosition), newCamWorldPosition, Color.red);

            bool outsideDeadZone = true;
            foreach (Vector3 dir in deadZoneDirs)
            {
                if (Physics.Raycast(newCamWorldPosition, transform.rotation * dir, out RaycastHit deadZoneHitInfo, deadZoneRadius, environment))
                {
                    outsideDeadZone = false;
                    if (i == 0)
                    {
                        // Push it out
                        newCamWorldPosition -= (deadZoneRadius - deadZoneHitInfo.distance) * (transform.rotation * dir);
                    }
                    Debug.DrawLine(newCamWorldPosition, newCamWorldPosition + transform.rotation * dir * deadZoneRadius, Color.red);
                    break;
                }
                else
                {
                    Debug.DrawLine(newCamWorldPosition, newCamWorldPosition + transform.rotation * dir * deadZoneRadius, Color.blue);
                }
            }

            if (outsideDeadZone)
            {
                break;
            }
        }

        cameraHandle.position = newCamWorldPosition;

        if (Vector3.Distance(newCamWorldPosition, transform.position) < 0.4f)
        {
            player_renderer1.enabled = false;
            player_renderer2.enabled = false;
            player_renderer3.enabled = false;
        }
        else
        {
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
