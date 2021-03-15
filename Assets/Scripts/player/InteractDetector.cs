using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractDetector {
    readonly Vector3 forwardOffset = new Vector3(0, 0.5f, 0);
    readonly float forwardDistance = 1f;

    readonly Vector3 downOffset = new Vector3(0, 0.3f, 1f);
    readonly float downDistance = 2f;

    readonly Vector3 ledgeAboveOffset = new Vector3(0, 2.5f, 0);
    readonly float minVerticalOffset = 0.5f;
    readonly float maxVerticalOffset = 1.5f;
    readonly Vector3 grabDetectBoxHalfSize = new Vector3(0.4f, 0.75f, 0.25f);
    readonly float grabDistance = 0.5f;

    readonly Transform playerTransform;
    readonly Transform modelTransform;

    readonly int environment;

    RaycastHit hitInfo;
    public InteractDetector(Transform playerTransform, Transform modelTransform) {
        this.playerTransform = playerTransform;
        this.modelTransform = modelTransform;
        environment = LayerMask.GetMask("environment");
    }
    public bool DetectFront(out RaycastHit hit) {
        Debug.DrawRay(playerTransform.position + forwardOffset, modelTransform.forward * forwardDistance, new Color(0, 1, 0));
        if (Physics.Raycast(playerTransform.position + forwardOffset, modelTransform.forward, out hit, forwardDistance)) {
            return true;
        }
        return false;
    }

    public bool DetectLedgeBelow() {
        Debug.DrawRay(playerTransform.position + modelTransform.rotation * downOffset, Vector3.down * downDistance, new Color(0, 1, 0));
        Debug.DrawRay(playerTransform.position + modelTransform.rotation * new Vector3(0, -0.1f, 1), -modelTransform.forward * 1.5f, new Color(0, 1, 0));
        if (!Physics.Raycast(playerTransform.position + modelTransform.rotation * downOffset, Vector3.down, downDistance)) {
            return true;
        }
        return false;
    }

    public bool DetectLedgeAbove() {
        Debug.DrawRay(playerTransform.position + ledgeAboveOffset, modelTransform.forward * grabDistance, new Color(0, 1, 0));
        Vector3 checkDownwardOrigin = playerTransform.position + ledgeAboveOffset + modelTransform.forward * grabDistance;
        Debug.DrawRay(checkDownwardOrigin, Vector3.down * maxVerticalOffset, new Color(0, 1, 0));
        Debug.DrawRay(checkDownwardOrigin, Vector3.down * minVerticalOffset, new Color(1, 0, 0));
        if (!Physics.Raycast(playerTransform.position + ledgeAboveOffset, modelTransform.forward, grabDistance)) {
            //if (Physics.Raycast(checkDownwardOrigin, Vector3.down, out hitInfo, maxVerticalOffset)) {
            //    if (hitInfo.distance >= minVerticalOffset) {
            //        return true;
            //    }
            //}
            if (Physics.BoxCast(checkDownwardOrigin, grabDetectBoxHalfSize, Vector3.down, modelTransform.rotation, 1.5f, environment)) {
                return true;
            }
        }
        return false;
    }
}
