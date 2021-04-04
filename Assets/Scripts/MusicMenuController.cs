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
    public Color defaultColour;

    private int numMusic;
    private bool show = false;
    private Animator animator;
    private AudioSource source;
    private AudioSource buttonSE;
    private TextMeshProUGUI currentPlaying;
    private TextMeshProUGUI[] menuItems;
    private Image playPauseButton;
    private Transform playlist;
    private Image tabIcon;

    private void Awake() {
        numMusic = musicItems.Length;
        menuItems = new TextMeshProUGUI[numMusic];
        source = GetComponent<AudioSource>();
        playPauseButton = transform.Find("MusicBar/Buttons/PlayPauseButton/PlayPause").GetComponent<Image>();
        buttonSE = transform.Find("ButtonSE").GetComponent<AudioSource>();
        playlist = transform.Find("PlaylistMask/Playlist/Scroll View/Viewport/MusicMenuContent").transform;
        tabIcon = transform.Find("MusicBar/Tab").GetComponent<Image>();
        currentPlaying = transform.Find("MusicBar/CurrentPlaying").GetComponent<TextMeshProUGUI>();
        animator = GetComponent<Animator>();
        for (int i = 0; i < numMusic; i++) {
            Transform child = playlist.GetChild(i);
            menuItems[i] = child.GetComponent<TextMeshProUGUI>();
            Button button = child.GetComponent<Button>();
            int temp = i;
            button.onClick.AddListener(() => SwitchTrack(temp));    // use i directly does not work
            menuItems[i].text = getTitle(i);
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
            if (PauseMenu.paused) return;
            if (show) {
                animator.SetBool("show", false);
                show = false;
                tabIcon.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
            } else {
                animator.SetBool("show", true);
                show = true;
                tabIcon.enabled = false;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        if (!source.isPlaying && isPlaying) {
            NextTrack(false);
        }
    }

    int CurrentTrack {
        set {
            menuItems[currentTrack].color = defaultColour;
            menuItems[value].color = activeColour;
            currentTrack = value;
            currentPlaying.text = menuItems[value].text;
        }
    }


    public void PlayPause() {
        buttonSE.Play();
        if (isPlaying) {
            Pause();
        } else {
            Play();
        }
    }

    public void Play() {
        source.Play();
        isPlaying = true;
        playPauseButton.sprite = pauseIcon;
    }

    public void Pause() {
        source.Pause();
        isPlaying = false;
        playPauseButton.sprite = playIcon;
    }

    public void NextTrack(bool playButtonSE = true) {
        if (playButtonSE) buttonSE.Play();
        int next = (currentTrack + 1) % numMusic;
        while (!musicItems[next].collected) {
            next = (next + 1) % numMusic;
        }
        CurrentTrack = next;
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
        CurrentTrack = prev;
        source.clip = musicItems[prev].clip;
        source.Play();
        isPlaying = true;
        playPauseButton.sprite = pauseIcon;
    }

    public void SwitchTrack(int i) {
        if (!musicItems[i].collected) return;
        buttonSE.Play();
        CurrentTrack = i;
        source.clip = musicItems[i].clip;
        source.Play();
        isPlaying = true;
        playPauseButton.sprite = pauseIcon;
    }

    public void AddToCollection(int id) {
        if (musicItems[id].collected) return;
        musicItems[id].collected = true;
        menuItems[id].text = getTitle(id);
    }

    string getTitle(int id) {
        string title = musicItems[id].collected ? musicItems[id].name : "??????";
        return $"{string.Format("{0:00}", id + 1)}.  {title}";
    }
}
