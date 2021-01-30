using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraCollision : MonoBehaviour {
    Vector3 cameraRestPosition;
    Vector3 cameraRestDirection;
    float restDistance;
    //Text debugText;
    Transform cameraTransform;
    int environmentLayerMask;
    SkinnedMeshRenderer player_renderer1;
    SkinnedMeshRenderer player_renderer2;

    void Awake() {
        cameraTransform = GameObject.Find("Main Camera").transform;
        cameraRestPosition = cameraTransform.localPosition;
        restDistance = cameraRestPosition.magnitude;
        cameraRestDirection = cameraRestPosition.normalized;
        //debugText = GameObject.Find("debugText").GetComponent<Text>();
        environmentLayerMask = LayerMask.GetMask("environment");
        player_renderer1 = GameObject.Find("Alpha_Surface").GetComponent<SkinnedMeshRenderer>();
        player_renderer2 = GameObject.Find("Alpha_Joints").GetComponent<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    void Update() {
        bool hit = Physics.Raycast(transform.position, transform.rotation * cameraRestPosition, 
            out RaycastHit hitInfo, restDistance, environmentLayerMask);
        if (hit) {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, 
                (hitInfo.distance - 0.3f) * cameraRestDirection, 0.3f);
            if (hitInfo.distance < 1f) {
                player_renderer1.enabled = false;
                player_renderer2.enabled = false;
                return;
            }
        } else {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, cameraRestPosition, 0.3f);
        }
        player_renderer1.enabled = true;
        player_renderer2.enabled = true;
        //debugText.text = $"{transform.rotation * cameraRestPosition} {transform.position} {restDistance} {hit}";
    }
}
