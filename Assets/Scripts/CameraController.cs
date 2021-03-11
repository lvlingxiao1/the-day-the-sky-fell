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
    public Vector3 targetLocalPosition;
    public Vector3 targetRotation;
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

        targetLocalPosition = defaultLocalPosition;

        collissionTest();

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

    void collissionTest() {
        localDistance = targetLocalPosition.magnitude;
        bool hit = Physics.Raycast(transform.position, transform.rotation * targetLocalPosition,
                out RaycastHit hitInfo, localDistance, environment);
        if (hit) {
            cameraHandle.localPosition = Vector3.Lerp(cameraHandle.localPosition,
                (hitInfo.distance - 0.1f) / localDistance * targetLocalPosition, 0.3f);
            if (hitInfo.distance < 1f) {
                player_renderer1.enabled = false;
                player_renderer2.enabled = false;
                player_renderer3.enabled = false;
                return;
            }
        } else {
            cameraHandle.localPosition = Vector3.Lerp(cameraHandle.localPosition, targetLocalPosition, 0.3f);
        }
        player_renderer1.enabled = true;
        player_renderer2.enabled = true;
        player_renderer3.enabled = true;
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
