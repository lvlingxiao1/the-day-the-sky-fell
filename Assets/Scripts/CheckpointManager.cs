using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour {
    public Vector3 respawnPositionOffset;
    private Sentence[] checkpointMessage = {
        new Sentence("", "Stamina recovered", null)
    };
    private Vector3 respawnPosition;
    private Vector3 respawnRotation;
    private bool handled = false;

    private void Awake() {
        respawnPosition = transform.position + transform.rotation * respawnPositionOffset;
        respawnRotation = transform.eulerAngles;
        respawnRotation.y += 180;
    }

    public void Respawn(MotionController controller, DialogueManager dialogueManager) {
        if (handled) return;
        handled = true;
        dialogueManager.BlackScreen();
        StartCoroutine(respawnCoroutine(controller));
    }

    private IEnumerator respawnCoroutine(MotionController controller) {
        yield return new WaitForSeconds(0.7f);
        controller.transform.position = respawnPosition;
        controller.modelTransform.eulerAngles = respawnRotation;
        controller.SetStateNormal();
        CameraController cameraController = FindObjectOfType<CameraController>();
        cameraController.ResetCamera(respawnRotation);
        handled = false;
    }

    public void TriggerDialogue(DialogueManager manager) {
        manager.StartDialogue(checkpointMessage);
    }
}
