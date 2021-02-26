using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {
    public Text nameText;
    public Text dialogueText;
    public Image speakerSprite;
    public Animator dialogueAnimator;
    public Animator blackScreenAnimator;
    private Sentence[] dialogue;
    private int index;

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
}
