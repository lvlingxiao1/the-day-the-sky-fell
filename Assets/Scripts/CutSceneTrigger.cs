using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneTrigger : MonoBehaviour
{
    public string cutSceneName;
    private bool triggered;
    private void OnTriggerEnter(Collider other) {
        if (triggered) return;
        FindObjectOfType<CutSceneController>().StartCutScene(cutSceneName);
        triggered = true;
    }
}
