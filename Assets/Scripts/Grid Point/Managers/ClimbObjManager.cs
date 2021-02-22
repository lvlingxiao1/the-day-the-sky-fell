using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    public class ClimbObjManager : MonoBehaviour
    {
        [HideInInspector]
        public List<Point> allPoints = new List<Point>();

        // Start is called before the first frame update
        void Start()
        {
            PopulateAllPoints();
        }

        public void Init() {
            PopulateAllPoints();
        }

        void PopulateAllPoints()
        {
            Point[] allChildPoints = GetComponentsInChildren<Point>();

            foreach (Point p in allChildPoints)
            {
                if (!allPoints.Contains(p))
                {
                    allPoints.Add(p);
                }
            }
        }

        public Neighbor GetNeighborForDirection(Vector3 targetDirection, Point currentPoint)
        {
            foreach(Neighbor n in currentPoint.neighbors)
            {
                if (n.direction == targetDirection)
                {
                    return n;
                }
            }

            return null;
        }

        public Point ReturnClosest(Vector3 from)
        {
            Point ret = null;
            float minDistance = Mathf.Infinity;

            for (int i =0; i < allPoints.Count; i++)
            {
                float distance = Vector3.Distance(allPoints[i].transform.position, from);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    ret = allPoints[i];
                }
            }

            return ret;
        }
    }
}
