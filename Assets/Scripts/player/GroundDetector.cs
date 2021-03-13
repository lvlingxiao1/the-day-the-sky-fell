using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetector {
    readonly Vector3 offset = new Vector3(0, 0.3f, 0);
    readonly float distance = 0.4f;
    readonly Transform playerTransform;
    readonly int environment = LayerMask.GetMask("environment");
    public GroundDetector(Transform playerTransform) {
        this.playerTransform = playerTransform;
    }
    public bool IsOnGround() {
        Debug.DrawRay(playerTransform.position + offset, Vector3.down * distance, new Color(1, 0, 0));
        return Physics.Raycast(playerTransform.position + offset, Vector3.down, distance, environment);
    }
}
