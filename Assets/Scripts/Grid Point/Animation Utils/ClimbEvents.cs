using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    // Top level component to interface with climb controller in case there are more climb events in the future
    public class ClimbEvents : MonoBehaviour
    {
        ClimbController cb;
        void Start()
        {
            cb = transform.root.GetComponentInChildren<ClimbController>();
        }

        public void EnableRootMovement(float t)
        {
            StartCoroutine(Enabler(t));
        }

        IEnumerator Enabler(float t)
        {
            yield return new WaitForSeconds(t);
            cb.isRootMovementEnbled = true;
        }
    }

}