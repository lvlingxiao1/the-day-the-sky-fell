﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CheckpointManager : MonoBehaviour {
    public Vector3 respawnPositionOffset;
    public Sprite getDrink;
    private Sentence[] checkpointMessage = {null};
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
        checkpointMessage[0] = new Sentence("", "Drinks replenished", getDrink, "vending machine");
    }

    public void HandleInteract(PlayerController controller, DialogueManager dialogueManager) {
        if (firstTime && controller.livesMax < LIVES_MAX) {
            controller.livesMax++;
        }
        firstTime = false;
        controller.lives = controller.livesMax;
        controller.livesUI.SetLives(controller.lives);
        dialogueManager.StartDialogue(checkpointMessage);
    }

    public void Respawn(PlayerController controller) {
        if (handled) return;
        audioManager.Play("falling");
        handled = true;
        UIController.BlackScreen();
        StartCoroutine(respawnCoroutine(controller));
    }

    private IEnumerator respawnCoroutine(PlayerController controller) {
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
        CaptionTrigger caption = GetComponentInChildren<CaptionTrigger>();
        if (caption) caption.ResetTriggers();
        handled = false;
    }
}
