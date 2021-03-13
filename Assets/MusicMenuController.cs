using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class MusicItem {
    public string name;
    public AudioClip clip;
    public bool collected = false;
}

public class MusicMenuController : MonoBehaviour {
    const int MUSIC_TOTAL = 6;

    public MusicItem[] musicItems = new MusicItem[MUSIC_TOTAL];
    public int currentTrack = 0;
    public bool isPlaying = true;
    public Sprite playIcon;
    public Sprite pauseIcon;
    public Color activeColour;
    Color defaultColour = new Color(0, 0, 0);

    private bool show = false;
    private Animator animator;
    private Transform menu;
    private AudioSource source;
    private TextMeshProUGUI[] menuItems = new TextMeshProUGUI[MUSIC_TOTAL];
    private Image playPauseButton;

    private void Awake() {
        source = GetComponent<AudioSource>();
        playPauseButton = GameObject.Find("PlayPause").GetComponent<Image>();
        menu = transform.GetChild(2).transform;
        animator = GetComponent<Animator>();
        for (int i = 0; i < MUSIC_TOTAL; i++) {
            menuItems[i] = menu.GetChild(i).GetComponent<TextMeshProUGUI>();
            string title = musicItems[i].collected ? musicItems[i].name : "??????";
            menuItems[i].text = $"{i + 1}.  {title}";
            if (currentTrack == i) {
                menuItems[i].color = activeColour;
            } else {
                menuItems[i].color = defaultColour;
            }
        }
        source.clip = musicItems[currentTrack].clip;
        source.Play();
    }

    void Update() {
        if (Input.GetButtonDown("Menu")) {
            if (show) {
                animator.SetBool("show", false);
                show = false;
                Cursor.lockState = CursorLockMode.Locked;
            } else {
                animator.SetBool("show", true);
                show = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void PlayPause() {
        if (isPlaying) {
            source.Pause();
            isPlaying = false;
            playPauseButton.sprite = playIcon;
        } else {
            source.Play();
            isPlaying = true;
            playPauseButton.sprite = pauseIcon;
        }
    }

    public void NextTrack() {
        int next = (currentTrack + 1) % MUSIC_TOTAL;
        while (!musicItems[next].collected) {
            next = (next + 1) % MUSIC_TOTAL;
        }
        menuItems[currentTrack].color = defaultColour;
        menuItems[next].color = activeColour;
        currentTrack = next;
        source.clip = musicItems[next].clip;
        source.Play();
        isPlaying = true;
        playPauseButton.sprite = playIcon;
    }

    public void PrevTrack() {
        int prev = currentTrack;
        do {
            prev--;
            if (prev < 0) prev += MUSIC_TOTAL;
        } while (!musicItems[prev].collected);
        menuItems[currentTrack].color = defaultColour;
        menuItems[prev].color = activeColour;
        currentTrack = prev;
        source.clip = musicItems[prev].clip;
        source.Play();
        isPlaying = true;
        playPauseButton.sprite = playIcon;
    }

    public void SwitchTrack(int i) {
        if (!musicItems[i].collected) return;
        menuItems[currentTrack].color = defaultColour;
        currentTrack = i;
        menuItems[i].color = activeColour;
        source.Stop();
        source.clip = musicItems[i].clip;
        source.Play();
        isPlaying = true;
        playPauseButton.sprite = playIcon;
    }
}
