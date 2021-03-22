using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPoster : MonoBehaviour, IItem {
    public int musicId;
    private bool pickedUp = false;

    public bool IsPickedUp() { return pickedUp; }
    public void PickUp() {
        MusicMenuController menu = FindObjectOfType<MusicMenuController>();
        FindObjectOfType<AudioManager>().Play("QR Scan");
        menu.AddToCollection(musicId);
        menu.SwitchTrack(musicId);
        FindObjectOfType<CaptionManager>().StartCaption(new CaptionSentence[] { new CaptionSentence($"New music: {menu.musicItems[musicId].name} is found! Press [Tab] to show music player") });
        pickedUp = true;
    }

    public string GetInteractMessage() {
        return "Press [E] to Scan QR Code";
    }
}
