using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseMenu : MonoBehaviour {
    public static bool paused = false;
    public GameObject pauseMenuUI;
    private TextMeshProUGUI pauseMenuPrompt;
    private AudioManager am;
    private const string promptMenuOn = "[Esc] Pause/Controls";
    private const string promptMenuOff = "[Esc] Resume";

    void Awake() {
        pauseMenuPrompt = GameObject.Find("MenuPrompt").GetComponent<TextMeshProUGUI>();
        pauseMenuPrompt.text = promptMenuOn;

        am = GameObject.Find("AudioManager").GetComponent<AudioManager>();
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
        paused = false;
    }

    void Pause() {
        pauseMenuUI.SetActive(true);
        pauseMenuPrompt.text = promptMenuOff;
        Time.timeScale = 0f;
        am.SetMasterVolumne(0.5f);
        paused = true;
    }
}
