using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LedgeDetector {
    Vector3 entryDetectOrigin = new Vector3(0, -0.3f, 1);
    readonly int environment = LayerMask.GetMask("environment");
    readonly Transform playerTransform;
    readonly Transform modelTransform;
    readonly Text debugText;
    public readonly float hangOffsetY = -2.224f;
    public readonly float hangOffsetZ = 0.17f;
    readonly Vector3 ledgeAboveOffset = new Vector3(0, 2.0f, 0);
    readonly float grabDistance = 0.5f;

    public LedgeDetector(Transform playerTransform, Transform modelTransform) {
        this.playerTransform = playerTransform;
        this.modelTransform = modelTransform;
        debugText = GameObject.Find("debugText").GetComponent<Text>();
        hangOffsetZ = playerTransform.GetComponent<CapsuleCollider>().radius;
    }

    public void EnterLedge() {
        bool hit = Physics.Raycast(playerTransform.position + modelTransform.rotation * entryDetectOrigin,
            modelTransform.rotation * Vector3.back, out RaycastHit hitInfo, 1f, environment);
        if (!hit) return;
        Vector3 newPos = hitInfo.point + hitInfo.normal * hangOffsetZ;
        newPos.y = playerTransform.position.y + hangOffsetY;
        Vector3 newForward = -hitInfo.normal;
        newForward.y = 0;
        newForward.Normalize();

        playerTransform.position = newPos;
        modelTransform.forward = newForward;
    }

    public void AdjustFacingToLedge() {
        bool hit = Physics.Raycast(playerTransform.position + ledgeAboveOffset,
            modelTransform.forward, out RaycastHit hitInfo, grabDistance, environment);
        if (!hit) return;
        Vector3 newPos = hitInfo.point + hitInfo.normal * hangOffsetZ;
        newPos.y = playerTransform.position.y;
        Vector3 newForward = -hitInfo.normal;
        newForward.y = 0;
        playerTransform.position = newPos;
        modelTransform.forward = newForward;
    }
}
