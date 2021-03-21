using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sentence {
    public string name;
    [TextArea(3, 10)]
    public string content;
    public Sprite sprite;
    public string audio;
    public Sentence(string name, string content, Sprite sprite = null, string audio = "") {
        this.name = name; this.content = content; this.sprite = sprite; this.audio = audio;
    }
}

[System.Serializable]
public class CaptionSentence {
    public string content;
    public string audio;
    public float duration = 3;
    public CaptionSentence(string content, string audio = null, float duration = 3) {
        this.content = content; this.audio = audio; this.duration = duration;
    }
}

/**
 * Unity does not support serialize nested arrays... but using a wrapper class works
 */
[System.Serializable]
public class CaptionSentences {
    public CaptionSentence[] sentences;
}
