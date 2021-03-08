using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaptionDetector : MonoBehaviour
{
    public string[] captionComments;
    public string sfx;
    private bool firstTime;

    private void Awake() {
        firstTime = true;
    }

    IEnumerator OnTriggerEnter(Collider col) {
        if (firstTime && col.tag == "Player") {
            firstTime = false;

            FindObjectOfType<DialogueManager>().displayCaption(captionComments, sfx);

            yield return new WaitForSeconds(3);

            FindObjectOfType<DialogueManager>().removeCaption();
        }
    }
}
