using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedCollider : MonoBehaviour {
    private bool isColliding;
    private BoxCollider _collider;
    private void Awake() {
        _collider = GetComponent<BoxCollider>();
    }
    private void OnTriggerEnter(Collider other) {
        isColliding = true;
    }
    private void OnTriggerExit(Collider other) {
        isColliding = false;
    }

    public void TurnOnCollider() {
        if (isColliding) {
            StartCoroutine(TryTurnOnCollider());
        } else {
            _collider.isTrigger = false;
        }
    }
    public void TurnOffCollider() {
        StopAllCoroutines();
        _collider.isTrigger = true;
    }
    IEnumerator TryTurnOnCollider() {
        while (isColliding) {
            yield return null;
        }
        _collider.isTrigger = false;
    }
}
