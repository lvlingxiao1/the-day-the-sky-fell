using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseMenu : MonoBehaviour {
    public static bool paused = false;
    public GameObject pauseMenuUI;
    private TextMeshProUGUI pauseMenuPrompt;
    private AudioManager am;
    MusicMenuController musicPlayer;
    private const string promptMenuOn = "[Esc] Pause/Controls";
    private const string promptMenuOff = "[Esc] Resume";
    private bool shouldResumeBGM = false;

    void Awake() {
        pauseMenuPrompt = GameObject.Find("MenuPrompt").GetComponent<TextMeshProUGUI>();
        pauseMenuPrompt.text = promptMenuOn;

        am = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        musicPlayer = FindObjectOfType<MusicMenuController>();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (paused) {
                Resume();
            } else {
                Pause();
            }
        }
    }

    void Resume() {
        pauseMenuUI.SetActive(false);
        pauseMenuPrompt.text = promptMenuOn;
        Time.timeScale = 1f;
        am.SetMasterVolumne(1f);
        if (shouldResumeBGM) musicPlayer.Play();
        paused = false;
    }

    void Pause() {
        pauseMenuUI.SetActive(true);
        pauseMenuPrompt.text = promptMenuOff;
        Time.timeScale = 0f;
        am.SetMasterVolumne(0f);
        shouldResumeBGM = musicPlayer.isPlaying;
        if (shouldResumeBGM) musicPlayer.Pause();
        paused = true;
    }
}
