using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CheckpointManager : MonoBehaviour {
    public Vector3 respawnPositionOffset;
    private Sentence[] checkpointMessage = {
        new Sentence("", "Stamina recovered", null, "vending machine")
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
        yield return new WaitForSeconds(0.9f);
        if (controller.lives > 0) {
            controller.lives--;
            controller.transform.position = controller.lastSafePosition;
        } else {
            controller.lives = controller.livesMax;
            controller.transform.position = respawnPosition;
            controller.modelTransform.eulerAngles = respawnRotation;
        }
        GameObject.Find("LivesText").GetComponent<TextMeshProUGUI>().text = $"Drinks: {controller.lives}";
        controller.SetStateNormal();
        CameraController cameraController = FindObjectOfType<CameraController>();
        cameraController.ResetCamera(respawnRotation);
        CaptionDetector[] captionTriggers = FindObjectsOfType<CaptionDetector>();
        foreach(CaptionDetector trigger in captionTriggers) {
            trigger.ResetTriggers();
        }
        handled = false;
    }

    public void TriggerDialogue(DialogueManager manager) {
        manager.StartDialogue(checkpointMessage);
    }
}
