using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetector {
    Vector3 offset;
    float distance;
    public GroundDetector(Vector3 offset, float distance) {
        this.offset = offset;
        this.distance = distance;
    }
    public bool IsOnGround(Vector3 pos) {
        Debug.DrawRay(pos + offset, Vector3.down * distance, new Color(1, 0, 0));
        return Physics.Raycast(pos + offset, Vector3.down, distance);
    }
}
