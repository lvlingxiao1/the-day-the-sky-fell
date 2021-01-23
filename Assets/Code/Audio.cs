using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Audio {
    public string id;
    public AudioClip clip;
    [Range(0, 1)]
    public float volume = 1;
    [Range(0.3f, 3)]
    public float pitch = 1;
    public bool loop = false;
    [HideInInspector]
    public AudioSource source;
}
