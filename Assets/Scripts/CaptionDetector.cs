using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptionDetector : MonoBehaviour {
    public string[] captionComments;
    public string sfx;
    public float numTriggers = Mathf.Infinity;
    private float initialTriggers;

    private void Awake() {
        initialTriggers = numTriggers;
    }

    void OnTriggerEnter(Collider col) {
        if (numTriggers > 0 && col.CompareTag("Player")) {
            numTriggers -= 1;
            FindObjectOfType<DialogueManager>().StartCaption(captionComments, sfx, 3);
        }
    }

    public void ResetTriggers() {
        numTriggers = initialTriggers;
    }
}
