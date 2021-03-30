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
    private AudioManager audioManager;

    private void Awake() {
        respawnPosition = transform.position + transform.rotation * respawnPositionOffset;
        respawnRotation = transform.eulerAngles;
        respawnRotation.y += 180;
        audioManager = FindObjectOfType<AudioManager>();
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
        audioManager.Play("falling");
        handled = true;
        dialogueManager.BlackScreen();
        StartCoroutine(respawnCoroutine(controller));
    }

    private IEnumerator respawnCoroutine(MotionController controller) {
        yield return new WaitForSeconds(0.9f);
        CameraController cameraController = FindObjectOfType<CameraController>();
        if (controller.lives > 0) {
            controller.lives--;
            controller.transform.position = controller.lastSafePosition;
            controller.modelTransform.forward = cameraController.forward;
            cameraController.ResetVertical();
        } else {
            controller.lives = controller.livesMax;
            controller.transform.position = respawnPosition;
            controller.modelTransform.eulerAngles = respawnRotation;
            cameraController.ResetCamera(respawnRotation);
        }
        controller.livesUI.SetLives(controller.lives);
        controller.SetStateNormal();
        // // I think reset all captions is too much... I changed it to just reset the caption of the current vending machine
        //CaptionDetector[] captionTriggers = FindObjectsOfType<CaptionDetector>();
        //foreach(CaptionDetector trigger in captionTriggers) {
        //    trigger.ResetTriggers();
        //}
        CaptionTrigger caption = GetComponentInChildren<CaptionTrigger>();
        if (caption) caption.ResetTriggers();
        handled = false;
    }
}
