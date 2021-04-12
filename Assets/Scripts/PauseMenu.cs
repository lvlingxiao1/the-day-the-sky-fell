using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseMenu : MonoBehaviour {
    public static bool paused = false;
    public GameObject pauseMenuUI;
    private AudioManager am;
    MusicMenuController musicPlayer;
    private bool shouldResumeBGM = false;

    void Awake() {
        am = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        musicPlayer = FindObjectOfType<MusicMenuController>();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Pause")) {
            if (paused) {
                Resume();
            } else {
                Pause();
            }
        }
    }

    void Resume() {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        am.SetMasterVolumne(1f);
        if (shouldResumeBGM) musicPlayer.Play();
        paused = false;
    }

    void Pause() {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        am.SetMasterVolumne(0f);
        shouldResumeBGM = musicPlayer.isPlaying;
        if (shouldResumeBGM) musicPlayer.Pause();
        paused = true;
    }
}
