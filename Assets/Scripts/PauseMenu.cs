using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static bool paused = false;
    public GameObject pauseMenuUI;
    public TextMeshProUGUI pauseMenuPrompt;

    // Update is called once per frame
    void Update()
    {
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
        pauseMenuPrompt.text = "Esc for Controls";
        Time.timeScale = 1f;
        paused = false;
    }

    void Pause() {
        pauseMenuUI.SetActive(true);
        pauseMenuPrompt.text = "Esc to Resume";
        Time.timeScale = 0f;
        paused = true;
    }
}
