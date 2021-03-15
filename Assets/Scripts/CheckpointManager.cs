using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CheckpointManager : MonoBehaviour {
    public Vector3 respawnPositionOffset;
    private Sentence[] checkpointMessage = {
        new Sentence("", "Drinks replenished", null, "vending machine")
    };
    private Vector3 respawnPosition;
    public Vector3 respawnRotation;
    private bool handled = false;
    private bool firstTime = true;
    private int LIVES_MAX = 3;

    private void Awake() {
        respawnPosition = transform.position + transform.rotation * respawnPositionOffset;
        respawnRotation = transform.eulerAngles;
        respawnRotation.y += 180;
    }

    public void HandleInteract(MotionController controller, DialogueManager dialogueManager) {
        if (firstTime && controller.livesMax < LIVES_MAX) {
            controller.livesMax++;
        }
        firstTime = false;
        controller.lives = controller.livesMax;
        controller.livesUI.SetLives(controller.lives);
        dialogueManager.StartDialogue(checkpointMessage);
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
        controller.livesUI.SetLives(controller.lives);
        controller.SetStateNormal();
        CameraController cameraController = FindObjectOfType<CameraController>();
        cameraController.ResetCamera(respawnRotation);
        // // I think reset all captions is too much... I changed it to just reset the caption of the current vending machine
        //CaptionDetector[] captionTriggers = FindObjectsOfType<CaptionDetector>();
        //foreach(CaptionDetector trigger in captionTriggers) {
        //    trigger.ResetTriggers();
        //}
        CaptionDetector caption = GetComponentInChildren<CaptionDetector>();
        if (caption) caption.ResetTriggers();
        handled = false;
    }
}
