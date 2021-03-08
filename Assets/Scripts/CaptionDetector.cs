﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptionDetector : MonoBehaviour {
    public string[] captionComments;
    public string sfx;
    public float numTriggers = Mathf.Infinity;

    IEnumerator OnTriggerEnter(Collider col) {
        if (numTriggers > 0 && col.CompareTag("Player")) {
            numTriggers -= 1;
            DialogueManager manager = FindObjectOfType<DialogueManager>();
            manager.DisplayCaption(captionComments, sfx);
            yield return new WaitForSeconds(3);
            manager.RemoveCaption();
        }
    }
}
