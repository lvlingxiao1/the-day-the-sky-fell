using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour {
    public Vector3 respawnPositionOffset;
    private Vector3 respawnPosition;
    private Vector3 respawnRotation;

    private void Awake() {
        respawnPosition = transform.position + transform.rotation * respawnPositionOffset;
        respawnRotation = transform.eulerAngles;
        respawnRotation.y += 180;
    }

    public void Respawn(MotionController controller) {
        controller.transform.position = respawnPosition;
        controller.modelTransform.eulerAngles = respawnRotation;
        controller.SetStateNormal();
        CameraController cameraController = FindObjectOfType<CameraController>();
        cameraController.ResetCamera(respawnRotation);
    }
}
