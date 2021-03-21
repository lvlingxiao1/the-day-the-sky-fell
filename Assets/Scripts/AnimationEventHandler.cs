using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour {
    AudioManager audioManager;
    MotionController controller;
    void Awake() {
        audioManager = FindObjectOfType<AudioManager>();
        controller = FindObjectOfType<MotionController>();
    }

    public void PlayFootstepAudio() {
        if (controller.grounded) {
            audioManager.PlayIfNotPlaying("footstep");
        }
    }
}
