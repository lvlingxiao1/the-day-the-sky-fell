using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour {
    float baseY;
    float topY;
    public float offsetDistance = 0.9f;
    public float exitOffsetDistance = 0.5f;
    public Vector3 entryPos;
    public Vector3 exitPos;

    bool entering;
    bool exiting;

    float lerpTime;
    float progress;
    Transform playerTransform;
    Vector3 startPos;
    Vector3 endPos;

    void Start() {
        Bounds bounds = GetComponent<BoxCollider>().bounds;
        baseY = bounds.min.y;
        topY = bounds.max.y;
        entryPos = transform.position + transform.forward * offsetDistance;
        exitPos = transform.position - transform.forward * exitOffsetDistance;
        exitPos.y = topY;
    }
    public float BaseY { get { return baseY; } }
    public float TopY { get { return topY; } }

    public void GetOntoLadder(Transform playerTransform, Transform modelTransform, float speed) {
        this.playerTransform = playerTransform;
        modelTransform.forward = -transform.forward;
        startPos = playerTransform.position;
        endPos = entryPos;
        endPos.y = startPos.y + 0.5f;
        lerpTime = (endPos - startPos).magnitude * 2 / speed;
        progress = 0;
        entering = true;
    }

    public void ExitLadderFromTop(Transform playerTransform) {
        this.playerTransform = playerTransform;
        startPos = playerTransform.position;
        endPos = exitPos;
        lerpTime = 0.5f;
        progress = 0;
        exiting = true;
    }

    private void Update() {
        if (entering) {
            progress += Time.deltaTime;
            playerTransform.position = Vector3.Lerp(startPos, endPos, progress / lerpTime);
            if (progress >= lerpTime) {
                entering = false;
                playerTransform.GetComponentInParent<PlayerController>().SetStateOnLadder();
            }
        } else if (exiting) {
            progress += Time.deltaTime;
            playerTransform.position = Vector3.Lerp(startPos, endPos, progress / lerpTime);
            if (progress >= lerpTime) {
                exiting = false;
                playerTransform.GetComponentInParent<PlayerController>().SetStateNormal();
            }
        }
    }
}
