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

    List<string> vendingMachine = new List<string>
            { "I just realized... this might be the last Cola I ever drink",
            "Looks like I can take a break here.",
            "Goodbye tooth rotting drink. May I never get another cavity."
            };

    // Start is called before the first frame update
    void Start()
    {
        caption.enabled = false;
        captionText.text = "";
        displaying = 0;
    }

    IEnumerator OnTriggerEnter(Collider col) {
        if (col.tag == "VendingMachineCaption") {
            displaying++;
            Destroy(col);
            int index = Random.Range(0, vendingMachine.Count);
            caption.enabled = true;
            captionText.text = vendingMachine[index];

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
