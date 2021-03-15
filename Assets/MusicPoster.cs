using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPoster : MonoBehaviour, IItem {
    public int musicId;
    private bool pickedUp = false;

    public bool IsPickedUp() { return pickedUp; }
    public void PickUp() {
        MusicMenuController menu = FindObjectOfType<MusicMenuController>();
        menu.AddToCollection(musicId);
        menu.SwitchTrack(musicId);
        FindObjectOfType<DialogueManager>().StartCaption(new string[] { $"New music: {menu.musicItems[musicId].name} is found! Press [Tab] to show music player" }, null, 3);
        pickedUp = true;
    }

    public string GetInteractMessage() {
        return "Press [F] to Scan QR Code";
    }
}
