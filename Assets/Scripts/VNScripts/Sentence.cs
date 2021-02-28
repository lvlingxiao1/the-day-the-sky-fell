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
