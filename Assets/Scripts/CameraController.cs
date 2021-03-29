using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour {
    public float cameraSpeedMouseX = 180;
    public float cameraSpeedMouseY = 60;
    public float cameraSpeedKeyboardX = 180;
    public float cameraSpeedKeyboardY = 60;

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
    float fovOffset;
    Transform cameraHandle;
    MotionController motionController;

    [HideInInspector]
    public Vector3 forward;
    [HideInInspector]
    public Vector3 right;

    //Text debugText;

    void Awake() {
        input = FindObjectOfType<PlayerInput>();
        motionController = FindObjectOfType<MotionController>();
        mainCamera = GameObject.Find("Main Camera").transform;
        fovOffset = Mathf.Tan(0.5f * Camera.main.fieldOfView * Mathf.Deg2Rad) * Camera.main.nearClipPlane * Camera.main.aspect + 0.1f;
        cameraHandle = GameObject.Find("CameraHandle").transform;
        mainCamera.position = cameraHandle.position;
        mainCamera.rotation = cameraHandle.rotation;

        defaultLocalPosition = cameraHandle.localPosition;
        defaultRotation = transform.localEulerAngles;
        restDistance = defaultLocalPosition.magnitude;
        targetRotation = transform.eulerAngles;
        nextRotation = targetRotation;

        environment = LayerMask.GetMask("environment") | LayerMask.GetMask("Default");
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
        if (motionController.ShouldRotateCamera()) {
            targetRotation.y += input.goingRight * 0.5f * cameraSpeedKeyboardX * Time.fixedDeltaTime;
        }
        targetRotation.x = Mathf.Clamp(targetRotation.x, -40, 70);
        transform.eulerAngles = targetRotation;

        CollissionTest();

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

    void CollissionTest() {
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(transform.position, transform.rotation * defaultLocalPosition,
            out hitInfo, restDistance, environment);

        Vector3 newCamWorldPosition = hit ? hitInfo.point : transform.TransformPoint(defaultLocalPosition);
        Vector3 cameraPlaneCenter = newCamWorldPosition - (transform.rotation * defaultLocalPosition).normalized * Camera.main.nearClipPlane;
        Debug.DrawLine(transform.position, cameraPlaneCenter, Color.cyan);

        Vector3[] checkDirs = { transform.right, -transform.right, transform.up, -transform.up };
        float lerpRate = 0.3f;
        foreach (Vector3 dir in checkDirs)
        {
            Debug.DrawLine(cameraPlaneCenter, cameraPlaneCenter + dir * fovOffset, Color.red);
            if (Physics.Raycast(cameraPlaneCenter, dir, out hitInfo, fovOffset, environment))
            {
                lerpRate = 0.7f;
                newCamWorldPosition += (hitInfo.point - (cameraPlaneCenter + dir * fovOffset));
            }
        }
        cameraHandle.position = Vector3.Lerp(cameraHandle.position, newCamWorldPosition, lerpRate);

        if (Vector3.SqrMagnitude(newCamWorldPosition - transform.position) < 0.36f) {
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
        targetRotation = modelRotation + defaultRotation;   // model rotation has only Y component, default rotation only has X component
        transform.eulerAngles = targetRotation;
        ComputeCameraForward();
    }

    void ComputeCameraForward() {
        forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
    }

    public void ResetVertical() {
        targetRotation.x = defaultRotation.x;
    }
}
