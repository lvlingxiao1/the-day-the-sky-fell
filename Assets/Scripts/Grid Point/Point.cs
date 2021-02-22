using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    [System.Serializable]
    public class Point : MonoBehaviour
    {
        public List<Neighbor> neighbors = new List<Neighbor>();
        public List<IkPositions> iks = new List<IkPositions>();

        public IkPositions ReturnIK(AvatarIKGoal goal)
        {
            for (int i = 0; i < iks.Count; i++)
            {
                if (iks[i].IK == goal)
                {
                    return iks[i];
                }
            }
            return null;
        }

        public Neighbor ReturnNeighbor(Point target)
        {
            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i].target == target)
                {
                    return neighbors[i];
                }
            }
            return null;
        }

        public void ResetNeighbor()
        {
            List<Neighbor> tmp = new List<Neighbor>();
            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i].target != null && neighbors[i].cType == ConnectionType.dismount)
                {
                    tmp.Add(neighbors[i]);
                }
            }

            neighbors = tmp;
        }
    }

    [System.Serializable]
    public class Neighbor
    {
        public Vector3 direction;
        public Point target;
        public ConnectionType cType;
    }

    [System.Serializable]
    public class IkPositions
    {
        public AvatarIKGoal IK;
        public Transform target;
        public Transform hint;
    }

    public enum ConnectionType
    {
        inBetween,
        leap,
        dismount,
        fall
    }
}
