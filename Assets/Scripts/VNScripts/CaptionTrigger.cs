using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptionTrigger : MonoBehaviour {
    public CaptionSentences[] captionsCollection;
    public float numTriggers = Mathf.Infinity;
    private float initialTriggers;

    private void Awake() {
        initialTriggers = numTriggers;
    }

    void OnTriggerEnter(Collider col) {
        if (numTriggers > 0 && col.CompareTag("Player")) {
            numTriggers -= 1;
            FindObjectOfType<CaptionManager>().StartCaption(captionsCollection[Random.Range(0, captionsCollection.Length)].sentences);
        }
    }

    public void ResetTriggers() {
        numTriggers = initialTriggers;
    }
}
