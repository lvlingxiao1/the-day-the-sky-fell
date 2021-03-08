using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CaptionDetector : MonoBehaviour
{
    public TextMeshProUGUI captionText;
    public Image caption;
    public int displaying;
    public string[] captionDialogue;
    private bool firstTime;

    // Start is called before the first frame update
    void Start()
    {
        caption.enabled = false;
        captionText.text = "";
        displaying = 0;
        firstTime = true;
    }

    IEnumerator OnTriggerEnter(Collider col) {
        if (col.tag == "Player" && firstTime) {
            firstTime = false;
            displaying++;
            int index = Random.Range(0, captionDialogue.Length);
            caption.enabled = true;
            captionText.text = captionDialogue[index];

            Color temp = caption.color;
            temp.a = 0.45f;
            caption.color = temp;
        }


        yield return new WaitForSeconds(3);
        displaying--;

        if (displaying == 0) {
            removeCaption();
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
