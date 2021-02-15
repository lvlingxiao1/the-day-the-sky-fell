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
    private Queue<string> names;
    private Queue<string> sentences;
    private Queue<Sprite> sprites;

    // Start is called before the first frame update
    void Start()
    {
        names = new Queue<string>();
        sentences = new Queue<string>();
        sprites = new Queue<Sprite>();
        input = FindObjectOfType<PlayerInput>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        input.inputEnabled = false;
        animator.SetBool("isOpen", true);

        names.Clear();
        sentences.Clear();
        sprites.Clear();

        foreach (Sentence sentence in dialogue.sentences)
        {
            names.Enqueue(sentence.name);
            sentences.Enqueue(sentence.content);
            sprites.Enqueue(sentence.sprite);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string name = names.Dequeue();
        string sentence = sentences.Dequeue();
        Sprite sprite = sprites.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
        nameText.text = name;
        speakerSprite.sprite = sprite;
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
