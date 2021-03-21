using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LivesUI {
    Transform ui;
    private const int LIVES_MAX = 3;
    private Color DISPLAY = new Color(1, 1, 1, 1);
    private Color NONE = new Color(1, 1, 1, 0);
    public LivesUI(int n) {
        ui = GameObject.Find("LivesUI").transform;
        SetLives(n);
    }

    public void SetLives(int n) {
        for (int i = 0; i < LIVES_MAX; i++) {
            Image img = ui.GetChild(i).GetComponent<Image>();
            if (i < n) {
                img.color = DISPLAY;
            } else {
                img.color = NONE;
            }
        }
    }
}
