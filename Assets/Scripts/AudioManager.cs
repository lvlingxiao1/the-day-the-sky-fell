using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Audio[] audios;

    // Start is called before the first frame update
    void Awake()
    {
        foreach(Audio i in audios) {
            i.source = gameObject.AddComponent<AudioSource>();
            i.source.clip = i.clip;
            i.source.volume = i.volume;
            i.source.pitch = i.pitch;
            i.source.loop = i.loop;
        }

        Play("wind_ambient");
    }

    public void Play(string id) {
        foreach (Audio i in audios) {
            if (i.id == id) {
                i.source.Play();
                break;
            }
        }
    }

    public void PlayIfNotPlaying(string id) {
        foreach (Audio i in audios) {
            if (i.id == id && !i.source.isPlaying) {
                i.source.Play();
                break;
            }
        }
    }

    public void Stop(string id) {
        foreach (Audio i in audios) {
            if (i.id == id) {
                i.source.Stop();
                break;
            }
        }
    }

    public void SetMasterVolumne(float vol)
    {
        AudioListener.volume = Mathf.Clamp(vol, 0, 1);
    }
}
