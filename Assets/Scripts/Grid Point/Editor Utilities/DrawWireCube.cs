#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    [ExecuteInEditMode]
    public class DrawWireCube : MonoBehaviour
    {
        public List<IkPositions> IkPositionList = new List<IkPositions>();

        public bool refresh;

        void Update()
        {
            if (refresh)
            {
                IkPositionList.Clear();
                refresh = false;
            }
        }
    }
}

#endif