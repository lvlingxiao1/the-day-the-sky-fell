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
    public Image caption;
    private TextMeshProUGUI captionText;
    private int displayingCaption = 0;

    private void Awake() {
        audioManager = FindObjectOfType<AudioManager>();
        caption.enabled = false;
        captionText = caption.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        captionText.text = "";
    }

    public void StartDialogue(Sentence[] dialogue) {
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

    public void displayCaption(string[] captionComments, string sfx) {
        displayingCaption++;
        int index = Random.Range(0, captionComments.Length);
        caption.enabled = true;
        captionText.text = captionComments[index];
        audioManager.Play(sfx);

        Color temp = caption.color;
        temp.a = 0.45f;
        caption.color = temp;
    }

    public void removeCaption() {
        displayingCaption--;
        if (displayingCaption == 0) {
            captionText.text = "";
            StartCoroutine(FadeOut());
        }
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
