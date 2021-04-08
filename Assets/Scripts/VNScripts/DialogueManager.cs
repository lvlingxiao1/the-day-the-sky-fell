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
    private Sentence[] dialogue;
    private int index;
    private AudioManager audioManager;
    private string prevAudio = "";
    private CaptionManager captionManager;


    private void Awake() {
        audioManager = FindObjectOfType<AudioManager>();
        captionManager = FindObjectOfType<CaptionManager>();
    }

    public void StartDialogue(Sentence[] dialogue) {
        captionManager.RemoveCaption();
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
        FindObjectOfType<PlayerController>().SetStateNormal();
    }
}
