using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour {
    public Text nameText;
    public Text dialogueText;
    public Image speakerSprite;
    public Animator dialogueAnimator;
    public Animator blackScreenAnimator;
    private Sentence[] dialogue;
    private int index;
    private AudioManager audioManager;
    private string prevAudio = "";

    // caption variables
    private TextMeshProUGUI captionText;
    private int displayingCaption = 0;

    private void Awake() {
        audioManager = FindObjectOfType<AudioManager>();
        captionText = GameObject.Find("CaptionText").GetComponent<TextMeshProUGUI>();
        captionText.enabled = false;
    }

    public void StartDialogue(Sentence[] dialogue) {
        RemoveCaptionInstant();
        dialogueAnimator.SetBool("isOpen", true);

        index = 0;
        this.dialogue = dialogue;

        DisplayNextSentence();
    }

    public void DisplayNextSentence() {
        if (index >= dialogue.Length) {
            EndDialogue();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(TypeSentence(dialogue[index].content));
        nameText.text = dialogue[index].name;
        if (dialogue[index].sprite) {
            speakerSprite.sprite = dialogue[index].sprite;
            speakerSprite.color = Color.white;
        } else {
            speakerSprite.color = Color.clear;
        }
        string audio = dialogue[index].audio;
        if (audio != null && audio != "") {
            audioManager.Stop(prevAudio);
            audioManager.Play(audio);
            prevAudio = audio;
        }
        index++;
    }

    IEnumerator TypeSentence(string sentence) {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray()) {
            dialogueText.text += letter;
            yield return null;
        }
    }

    void EndDialogue() {
        dialogueAnimator.SetBool("isOpen", false);
        FindObjectOfType<MotionController>().SetStateNormal();
    }

    public void BlackScreen() {
        blackScreenAnimator.SetTrigger("start");
    }

    public void DisplayCaption(string[] captionComments, string sfx) {
        displayingCaption++;
        int index = Random.Range(0, captionComments.Length);
        captionText.text = captionComments[index];
        captionText.enabled = true;
        audioManager.Play(sfx);
    }

    public void RemoveCaption() {
        displayingCaption--;
        if (displayingCaption <= 0) {
            StartCoroutine(FadeOut());
        }
    }

    public void RemoveCaptionInstant() {
        displayingCaption = 0;
        captionText.text = "";
    }

    IEnumerator FadeOut() {
        displayingCaption = 0;
        Color currentColour = captionText.color;
        Color originalColour = currentColour;
        while (captionText.color.a > 0.001) {
            currentColour.a = Mathf.Lerp(currentColour.a, 0, 0.2f);
            captionText.color = currentColour;
            yield return null;
        }
        captionText.enabled = false;
        captionText.color = originalColour;
    }
}
