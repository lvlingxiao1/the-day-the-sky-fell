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
    public MusicItem[] musicItems;
    public int currentTrack = 0;
    public bool isPlaying = true;
    public Sprite playIcon;
    public Sprite pauseIcon;
    public Color activeColour;
    Color defaultColour = new Color(0, 0, 0);

    private int numMusic;
    private bool show = false;
    private Animator animator;
    private AudioSource source;
    private AudioSource buttonSE;
    private TextMeshProUGUI[] menuItems;
    private Image playPauseButton;

    private void Awake() {
        numMusic = musicItems.Length;
        menuItems = new TextMeshProUGUI[numMusic];
        source = GetComponent<AudioSource>();
        playPauseButton = transform.Find("ControlButtons/PlayPauseButton/PlayPause").GetComponent<Image>();
        buttonSE = transform.Find("ButtonSE").GetComponent<AudioSource>();
        Transform menu = transform.Find("Scroll View/Viewport/MusicMenuContent").transform;
        animator = GetComponent<Animator>();
        for (int i = 0; i < numMusic; i++) {
            Transform child = menu.GetChild(i);
            menuItems[i] = child.GetComponent<TextMeshProUGUI>();
            Button button = child.GetComponent<Button>();
            int temp = i;
            button.onClick.AddListener(() => SwitchTrack(temp));    // use i directly does not work
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
        if (!source.isPlaying) {
            NextTrack(false);
        }
    }

    public void PlayPause() {
        buttonSE.Play();
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

    public void NextTrack(bool playButtonSE = true) {
        if (playButtonSE) buttonSE.Play();
        int next = (currentTrack + 1) % numMusic;
        while (!musicItems[next].collected) {
            next = (next + 1) % numMusic;
        }
        menuItems[currentTrack].color = defaultColour;
        menuItems[next].color = activeColour;
        currentTrack = next;
        source.clip = musicItems[next].clip;
        source.Play();
        isPlaying = true;
        playPauseButton.sprite = pauseIcon;
    }

    public void PrevTrack() {
        buttonSE.Play();
        int prev = currentTrack;
        do {
            prev--;
            if (prev < 0) prev += numMusic;
        } while (!musicItems[prev].collected);
        menuItems[currentTrack].color = defaultColour;
        menuItems[prev].color = activeColour;
        currentTrack = prev;
        source.clip = musicItems[prev].clip;
        source.Play();
        isPlaying = true;
        playPauseButton.sprite = pauseIcon;
    }

    public void SwitchTrack(int i) {
        if (!musicItems[i].collected) return;
        buttonSE.Play();
        menuItems[currentTrack].color = defaultColour;
        currentTrack = i;
        menuItems[i].color = activeColour;
        source.Stop();
        source.clip = musicItems[i].clip;
        source.Play();
        isPlaying = true;
        playPauseButton.sprite = pauseIcon;
    }

    public void AddToCollection(int id) {
        if (musicItems[id].collected) return;
        musicItems[id].collected = true;
        menuItems[id].text = $"{id + 1}.  {musicItems[id].name}";
    }
}
