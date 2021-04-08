using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour {
    AudioManager audioManager;
    PlayerController controller;
    void Awake() {
        audioManager = FindObjectOfType<AudioManager>();
        controller = FindObjectOfType<PlayerController>();
    }

    public void PlayFootstepAudio() {
        if (controller.grounded) {
            audioManager.PlayIfNotPlaying("footstep");
        }
    }
}
