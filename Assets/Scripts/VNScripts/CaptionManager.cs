using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CaptionManager : MonoBehaviour {
    private CaptionSentence[] sentences;
    private int index;
    private AudioManager audioManager;
    private string prevAudio = "";
    private Animator animator;
    private TextMeshProUGUI captionText;
    void Awake() {
        animator = GetComponent<Animator>();
        captionText = GetComponentInChildren<TextMeshProUGUI>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void StartCaption(CaptionSentence[] sentences) {
        StopAllCoroutines();
        this.sentences = sentences;
        index = 0;
        animator.SetBool("open", true);
        StartCoroutine(CaptionCoroutine());
    }

    IEnumerator CaptionCoroutine() {
        CaptionSentence sentence = sentences[index];
        captionText.text = sentence.content;
        string audio = sentence.audio;
        if (audio != null && audio != "") {
            audioManager.Stop(prevAudio);
            audioManager.Play(audio);
            prevAudio = audio;
        }
        yield return new WaitForSeconds(sentence.duration);
        index++;
        if (index < sentences.Length) {
            StartCoroutine(CaptionCoroutine());
        } else {
            animator.SetBool("open", false);
        }
    }

    public void RemoveCaption() {
        animator.SetBool("open", false);
        StopAllCoroutines();
    }
}
