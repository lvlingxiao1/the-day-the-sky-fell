#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    [ExecuteInEditMode]
    public class GridManager : MonoBehaviour
    {
        public float minDistance = 2.5f;
        public float directThreshold = 1;
        public bool updateConnections;
        public bool resetConnections;

        List<Point> allPoints = new List<Point>();
        Vector3[] validDirections = new Vector3[8];

        void AddNeighbor(Point from, Point goal, Vector3 moveDirection)
        {
            Neighbor n = new Neighbor();
            n.direction = moveDirection;
            n.target = goal;
            n.cType = (Vector3.Distance(from.transform.position, goal.transform.position) < directThreshold) ? ConnectionType.inBetween : ConnectionType.leap;

            from.neighbors.Add(n);
            UnityEditor.EditorUtility.SetDirty(from); // notify update
        }

        void CreateDirections()
        {
            // starting from (1, 0, 0) and rotate CW on XY plane
            validDirections[0] = new Vector3( 1,  0,  0); // right
            validDirections[1] = new Vector3( 1, -1,  0); // [Diagonal] down right
            validDirections[2] = new Vector3( 0, -1,  0); // down
            validDirections[3] = new Vector3(-1, -1,  0); // [Diagonal] down left
            validDirections[4] = new Vector3(-1,  0,  0); // left
            validDirections[5] = new Vector3(-1,  1,  0); // [Diagonal] up left
            validDirections[6] = new Vector3( 0,  1,  0); // up
            validDirections[7] = new Vector3( 1,  1,  0); // [Diagonal] up right
        }

        void GetPoints()
        {
            allPoints.Clear();
            Point[] childPoints = GetComponentsInChildren<Point>();
            allPoints.AddRange(childPoints);
        }

        void CreateConnections()
        {
            for (int p = 0; p < allPoints.Count; p++)
            {
                Point currentPoint = allPoints[p];

                for (int d = 0; d < validDirections.Length; d++)
                {
                    List<Point> candidates = GetCandidatesOnDirection(validDirections[d], currentPoint);

                    Point closest = GetClosestPoint(candidates, currentPoint);

                    if (closest != null)
                    {
                        if (Vector3.Distance(currentPoint.transform.position, closest.transform.position) < minDistance)
                        {
                            // Make valid 2-step diagonal transition
                            if ((validDirections[d].x != 0) && (validDirections[d].y != 0))
                            {
                                // Skip on diagonal jump
                                if (Vector3.Distance(currentPoint.transform.position, closest.transform.position) > directThreshold)
                                {
                                    continue;
                                }
                            }

                            AddNeighbor(currentPoint, closest, validDirections[d]);
                        }
                    }
                }
            }
        }

        List<Point> GetCandidatesOnDirection(Vector3 targetDirection, Point from)
        {
            List<Point> ret = new List<Point>();

            for (int p = 0; p < allPoints.Count; p++)
            {
                Point targetPoint = allPoints[p];

                Vector3 direction = targetPoint.transform.position - from.transform.position;
                Vector3 relativeDirection = from.transform.InverseTransformDirection(direction);

                if (IsDirectionValid(targetDirection, relativeDirection))
                {
                    ret.Add(targetPoint);
                }
            }

            return ret;
        }

        bool IsDirectionValid(Vector3 targetDirection, Vector3 actualDirection)
        {
            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.y) * Mathf.Rad2Deg;
            float actualAngle = Mathf.Atan2(actualDirection.x, actualDirection.y) * Mathf.Rad2Deg;

            // Check if it is within 45 degree cone
            if ((actualAngle < targetAngle + 22.5f) && (actualAngle > targetAngle - 22.5f))
            {
                return true;
            }

            return false;
        }

        Point GetClosestPoint(List<Point> points, Point from)
        {
            Point ret = null;

            float minDistance = Mathf.Infinity;

            for (int i = 0; i < points.Count; i++)
            {
                float actualDistance = Vector3.Distance(points[i].transform.position, from.transform.position);

                if ((actualDistance < minDistance) && (points[i] != from))
                {
                    minDistance = actualDistance;
                    ret = points[i];
                }
            }

            return ret;
        }

        void FindDismountPoints()
        {
            GameObject dismountPrefab = Resources.Load("Dismount") as GameObject;
            if (dismountPrefab == null)
            {
                Debug.Log("Error: cannot find dismount prefab!");
                return;
            }

            // TODO
            PointsManager[] pointHandlers = GetComponentsInChildren<PointsManager>();

            List<Point> candidates = new List<Point>();
            for (int i = 0; i < pointHandlers.Length; i++)
            {
                if (pointHandlers[i].DismountPoint)
                {
                    candidates.AddRange(pointHandlers[i].pointsInOrder);
                    pointHandlers[i].DismountPoint = false;
                }
            }

            if (candidates.Count > 0)
            {
                GameObject parentObj = new GameObject();
                parentObj.name = "Dismount Points";
                parentObj.transform.parent = transform;
                parentObj.transform.localPosition = Vector3.zero;
                parentObj.transform.position = candidates[0].transform.position;

                foreach (Point p in candidates)
                {
                    Transform worldPosition = p.transform.parent;
                    GameObject dismountPointObj = Instantiate(dismountPrefab, worldPosition.position, worldPosition.rotation) as GameObject;

                    Vector3 exitPosition = worldPosition.position + worldPosition.forward / 1.6f + Vector3.up * 1.7f;
                    dismountPointObj.transform.position = exitPosition;

                    Point dismountPoint = dismountPointObj.GetComponentInChildren<Point>();

                    Neighbor n = new Neighbor();
                    n.direction = Vector3.up;
                    n.target = dismountPoint;
                    n.cType = ConnectionType.dismount;
                    p.neighbors.Add(n);

                    Neighbor n2 = new Neighbor();
                    n2.direction = Vector3.down;
                    n2.target = dismountPoint;
                    n2.cType = ConnectionType.dismount;
                    dismountPoint.neighbors.Add(n2);

                    dismountPointObj.transform.parent = parentObj.transform;
                }
            }
        }

        void RefreshAll()
        {
            DrawLine lineDrawer = transform.GetComponent<DrawLine>();

            if(lineDrawer != null)
            {
                lineDrawer.refresh = true;
            }

            for (int i = 0; i < allPoints.Count; i++)
            {
                DrawLineIndividual line = allPoints[i].transform.GetComponent<DrawLineIndividual>();
                if (line != null)
                {
                    line.refresh = true;
                }
            }
        }

        public List<Connection> GetAllConnections()
        {
            List<Connection> ret = new List<Connection>();

            for (int p = 0; p < allPoints.Count; p++)
            {
                for (int n = 0; n < allPoints[p].neighbors.Count; n++)
                {
                    Connection con = new Connection();
                    con.point1 = allPoints[p];
                    con.point2 = allPoints[p].neighbors[n].target;
                    con.cType = allPoints[p].neighbors[n].cType;

                    if (!HasConnection(ret, con))
                    {
                        ret.Add(con);
                    }
                }
            }

            return ret;
        }

        bool HasConnection(List<Connection> conList, Connection con)
        {
            for(int i = 0; i < conList.Count; i++)
            {
                // can consider the following optimization:
                // 1. unordered_pair set (need to find C# equivalent)
                // 2. hashmap point connection lookup
                if (
                    ((conList[i].point1 == con.point1) && (conList[i].point2 == con.point2)) ||
                    ((conList[i].point2 == con.point1) && (conList[i].point1 == con.point2))
                    ) {
                    return true;
                }
            }

            return false;
        }

        void Update()
        {
            if (updateConnections)
            {
                GetPoints();
                CreateDirections();
                CreateConnections();
                FindDismountPoints(); //TODO
                RefreshAll();

                updateConnections = false;
                Debug.Log("Connection update completed!");
            }

            if (resetConnections)
            {
                GetPoints();
                for (int p = 0; p < allPoints.Count; p++)
                {
                    // Preserve previous dismount points
                    allPoints[p].ResetNeighbor();
                }
                RefreshAll();
                resetConnections = false;
                Debug.Log("Connection reset completed!");
            }
        }
    }

    public class Connection
    {
        public Point point1;
        public Point point2;
        public ConnectionType cType;
    }
}

#endif