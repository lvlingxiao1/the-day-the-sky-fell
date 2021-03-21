using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HintTextDetector : MonoBehaviour {
    private TextMeshPro hint;

    private void Awake() {
        hint = gameObject.GetComponent<TextMeshPro>();
    }

    void OnTriggerEnter(Collider col) {
        if (col.CompareTag("Player")) {
            StartCoroutine(FadeInText());
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.CompareTag("Player")) {
            StartCoroutine(FadeOutText());
        }
    }

    private IEnumerator FadeInText() {
        while (hint.color.a < 1.0f)
        {
            hint.color = new Color(hint.color.r, hint.color.g, hint.color.b, hint.color.a + (Time.deltaTime * 0.8f));
            yield return null;
        }
    }

    private IEnumerator FadeOutText() {
        while (hint.color.a > 0f)
        {
            hint.color = new Color(hint.color.r, hint.color.g, hint.color.b, hint.color.a - (Time.deltaTime * 0.8f));
            yield return null;
        }
    }

}
