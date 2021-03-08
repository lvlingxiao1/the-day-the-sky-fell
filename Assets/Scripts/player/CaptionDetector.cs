using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CaptionDetector : MonoBehaviour
{
    public Image caption;
    public string[] captionComments;
    public string sfx;
    private bool firstTime;
    private AudioManager audioManager;
    private TextMeshProUGUI captionText;

    private void Awake() {
        audioManager = FindObjectOfType<AudioManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        caption.enabled = false;
        captionText = caption.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        captionText.text = "";
        firstTime = true;
    }

    IEnumerator OnTriggerEnter(Collider col) {

        displayCaption(col.tag);

        yield return new WaitForSeconds(3);

        removeCaption();
    }

    public void displayCaption(string tag) {
        if (tag == "Player" && firstTime) {
            firstTime = false;
            int index = Random.Range(0, captionComments.Length);
            caption.enabled = true;
            captionText.text = captionComments[index];
            audioManager.Play(sfx);

            Color temp = caption.color;
            temp.a = 0.45f;
            caption.color = temp;
        }
    }

    public void removeCaption() {
        captionText.text = "";
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut () {
        while (caption.color.a > 0) {
            Color temp = caption.color;
            temp.a -= Time.deltaTime / 2;
            caption.color = temp;
            yield return null;
        }
        caption.enabled = false;
        yield return null;
    }
}
