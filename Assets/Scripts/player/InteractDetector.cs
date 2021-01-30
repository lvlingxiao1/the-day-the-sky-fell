using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractDetector {
    Vector3 forwardOffset;
    float forwardDistance;
    Vector3 downOffset;
    float downDistance;
    public InteractDetector(Vector3 forwardOffset, float forwardDistance, Vector3 downOffset, float downDistance) {
        this.forwardOffset = forwardOffset;
        this.forwardDistance = forwardDistance;
        this.downOffset = downOffset;
        this.downDistance = downDistance;
    }
    public string Detect(Vector3 pos, Quaternion rotation, Vector3 forward, out RaycastHit hitInfo) {
        Debug.DrawRay(pos + forwardOffset, forward * forwardDistance, new Color(0, 1, 0));
        if (Physics.Raycast(pos + forwardOffset, forward, out hitInfo, forwardDistance)) {
            if (hitInfo.collider.tag == "ladder") {
                return "Ladder";
            }
        }

        Debug.DrawRay(pos + rotation * downOffset, Vector3.down * downDistance, new Color(0, 1, 0));
        if (!Physics.Raycast(pos + rotation * downOffset, Vector3.down, out hitInfo, downDistance)) {
            return "Edge";
        }
        return "None";
    }
}
