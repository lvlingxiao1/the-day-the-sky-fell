using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public Text nameText;
    public Text dialogueText;
    public Image speakerSprite;
    public Animator animator;
    private PlayerInput input;
    private Sentence[] sentences;
    private int index;

    // Start is called before the first frame update
    void Start()
    {
        input = FindObjectOfType<PlayerInput>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        input.inputEnabled = false;
        animator.SetBool("isOpen", true);

        index = 0;
        sentences = dialogue.sentences;

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (index >= sentences.Length)
        {
            EndDialogue();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentences[index].content));
        nameText.text = sentences[index].name;
        speakerSprite.sprite = sentences[index].sprite;
        index++;
    }

    IEnumerator TypeSentence (string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    void EndDialogue()
    {
        animator.SetBool("isOpen", false);
        input.inputEnabled = true;
    }
}
