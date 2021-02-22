#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    [ExecuteInEditMode]
    public class DrawLineIndividual : MonoBehaviour
    {
        public List<Neighbor> ConnectedPoints = new List<Neighbor>();

        public bool refresh;

        void Update()
        {
            if (refresh)
            {
                ConnectedPoints.Clear();
                refresh = false;
            }
        }
    }
}

#endif