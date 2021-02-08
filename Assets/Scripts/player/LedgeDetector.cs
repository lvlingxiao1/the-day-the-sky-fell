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
    readonly Vector3 adjustFacingPoint1 = new Vector3(0, 2f, -0.1f);
    readonly Vector3 adjustFacingPoint2 = new Vector3(-0.4f, 2.2f, -0.1f);
    readonly Vector3 adjustFacingPoint3 = new Vector3(0.4f, 2.2f, -0.1f);
    readonly float adjustDetectDistance = 0.5f;
    private RaycastHit hitInfo;

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

    public bool AdjustFacingToLedge() {
        Debug.DrawRay(playerTransform.position + adjustFacingPoint1, modelTransform.forward * adjustDetectDistance, new Color(0, 1, 0));
        Debug.DrawRay(playerTransform.position + modelTransform.rotation * adjustFacingPoint2, modelTransform.forward * adjustDetectDistance, new Color(0, 1, 0));
        Debug.DrawRay(playerTransform.position + modelTransform.rotation * adjustFacingPoint3, modelTransform.forward * adjustDetectDistance, new Color(0, 1, 0));
        Vector3 horizontalOffset = Vector3.zero;
        if (!Physics.Raycast(playerTransform.position + adjustFacingPoint1, modelTransform.forward, out hitInfo, adjustDetectDistance, environment)) {
            if (!Physics.Raycast(playerTransform.position + modelTransform.rotation * adjustFacingPoint2, modelTransform.forward, out hitInfo, adjustDetectDistance, environment)) {
                if (!Physics.Raycast(playerTransform.position + modelTransform.rotation * adjustFacingPoint3, modelTransform.forward, out hitInfo, adjustDetectDistance, environment)) return false;
                else horizontalOffset = modelTransform.rotation * new Vector3(-0.4f, 0, 0.1f);
            } else horizontalOffset = modelTransform.rotation * new Vector3(0.4f, 0, 0.1f);
        }
        Vector3 newPos = hitInfo.point + hitInfo.normal * (hangOffsetZ + 0.1f) + horizontalOffset;
        newPos.y = playerTransform.position.y;
        Vector3 newForward = -hitInfo.normal;
        newForward.y = 0;
        playerTransform.position = newPos;
        modelTransform.forward = newForward;
        return true;
    }
}
