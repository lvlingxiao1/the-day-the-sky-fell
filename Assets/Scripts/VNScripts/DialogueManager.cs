using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {
    public Text nameText;
    public Text dialogueText;
    public Image speakerSprite;
    public Animator animator;
    private PlayerInput input;
    private Sentence[] dialogue;
    private int index;
    private bool isInDialogue = false;

    void Awake() {
        input = FindObjectOfType<PlayerInput>();
    }

    public void StartDialogue(Sentence[] dialogue) {
        input.inputEnabled = false;
        animator.SetBool("isOpen", true);

        index = 0;
        this.dialogue = dialogue;
        isInDialogue = true;

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
        speakerSprite.sprite = dialogue[index].sprite;
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
        animator.SetBool("isOpen", false);
        input.inputEnabled = true;
        isInDialogue = false;
    }

    private void Update() {
        if (isInDialogue) {
            if (Input.GetButtonDown("Jump") || Input.GetMouseButtonDown(0)) {
                DisplayNextSentence();
            }
        }
    }
}
