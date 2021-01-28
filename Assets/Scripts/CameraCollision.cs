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

    void Start() {
        cameraTransform = GameObject.Find("Main Camera").transform;
        cameraRestPosition = cameraTransform.localPosition;
        restDistance = cameraRestPosition.magnitude;
        cameraRestDirection = cameraRestPosition.normalized;
        //debugText = GameObject.Find("debugText").GetComponent<Text>();
        environmentLayerMask = LayerMask.GetMask("environment");
    }

    // Update is called once per frame
    void Update() {
        bool hit = Physics.Raycast(transform.position, transform.rotation * cameraRestPosition, 
            out RaycastHit hitInfo, restDistance, environmentLayerMask);
        if (hit) {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, 
                (hitInfo.distance - 0.3f) * cameraRestDirection, 0.3f);
        } else {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, cameraRestPosition, 0.3f);
        }
        //debugText.text = $"{transform.rotation * cameraRestPosition} {transform.position} {restDistance} {hit}";
    }
}
