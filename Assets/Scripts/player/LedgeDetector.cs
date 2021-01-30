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

    public LedgeDetector(Transform playerTransform, Transform modelTransform) {
        this.playerTransform = playerTransform;
        this.modelTransform = modelTransform;
        debugText = GameObject.Find("debugText").GetComponent<Text>();
    }

    public void EnterLedge() {
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(playerTransform.position + modelTransform.rotation * entryDetectOrigin,
            modelTransform.rotation * Vector3.back, out hitInfo, 1f, environment);
        if (!hit) return;
        Vector3 newPos = hitInfo.point + hitInfo.normal * 0.21f;
        newPos.y = playerTransform.position.y - 2.224f;
        Vector3 newForward = -hitInfo.normal;
        newForward.y = 0;
        newForward.Normalize();

        playerTransform.position = newPos;
        modelTransform.forward = newForward;
        Debug.Log(newPos);
        Debug.Log(hitInfo.normal);
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
}
