using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LedgeDetector {
    Vector3 entryDetectOrigin = new Vector3(0, -0.3f, 1);
    int environment = LayerMask.GetMask("environment");
    Transform playerTransform;
    Transform modelTransform;
    Text debugText;

    float hangOffsetY = -2.224f;
    float hangOffsetZ = 0.17f;

    Vector3 ledgeAboveOffset = new Vector3(0, 2.0f, 0);
    float grabDistance = 0.5f;

    public LedgeDetector(Transform playerTransform, Transform modelTransform) {
        this.playerTransform = playerTransform;
        this.modelTransform = modelTransform;
        debugText = GameObject.Find("debugText").GetComponent<Text>();
        hangOffsetZ = playerTransform.GetComponent<CapsuleCollider>().radius;
    }

    public void EnterLedge() {
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(playerTransform.position + modelTransform.rotation * entryDetectOrigin,
            modelTransform.rotation * Vector3.back, out hitInfo, 1f, environment);
        if (!hit) return;
        Vector3 newPos = hitInfo.point + hitInfo.normal * hangOffsetZ;
        newPos.y = playerTransform.position.y + hangOffsetY;
        Vector3 newForward = -hitInfo.normal;
        newForward.y = 0;
        newForward.Normalize();

        playerTransform.position = newPos;
        modelTransform.forward = newForward;
    }

    //public bool EnterLedgeAbove() {
    //    RaycastHit hitInfo;
    //    bool hit = Physics.Raycast(playerTransform.position + ledgeAboveOffset,
    //        modelTransform.forward, out hitInfo, grabDistance, environment);
    //    if (!hit) return false;
    //    Vector3 newPos = hitInfo.point + hitInfo.normal * hangOffsetZ;
    //    newPos.y = playerTransform.position.y;
    //    Vector3 newForward = -hitInfo.normal;
    //    newForward.y = 0;
    //    newForward.Normalize();

    //    playerTransform.position = newPos;
    //    modelTransform.forward = newForward;
    //    return true;
    //}

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


    public void Detect() {
        //Debug.DrawRay(playerTransform.position + modelTransform.rotation * entryDetectOrigin,
        //    modelTransform.rotation * Vector3.back);


        //RaycastHit hitInfo;
        //bool hit = Physics.Raycast(playerTransform.position + new Vector3(0, 2.0f, 0),
        //    modelTransform.forward, out hitInfo, 1f, environment);
        //debugText.text = $"{hitInfo.distance}";
        //Debug.DrawRay(playerTransform.position + new Vector3(0, 2.0f, 0),
        //    modelTransform.forward);
    }

    public void ClimbUp(Rigidbody rb) {
        rb.position += modelTransform.forward * hangOffsetZ * 2 + new Vector3(0, -hangOffsetY, 0);
    }
}
