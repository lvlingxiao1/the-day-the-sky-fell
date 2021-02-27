// only run in editor view for point visualization
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Climbing
{
    [CustomEditor(typeof(DrawWireCube))]
    public class DrawWireCubeEditor : Editor
    {
        void OnSceneGUI()
        {
            DrawWireCube t = target as DrawWireCube;

            if (t.IkPositionList.Count == 0)
            {
                t.IkPositionList = t.transform.GetComponent<Point>().iks;
            }

            for (int i = 0; i < t.IkPositionList.Count; i++)
            {
                IkPositions currentIK = t.IkPositionList[i];
                if (currentIK.target != null)
                {
                    // Default color, to be overwritten once determined IK part
                    Color targetColor = Color.red;

                    switch (currentIK.IK)
                    {
                        case AvatarIKGoal.LeftHand:
                            targetColor = Color.cyan;
                            break;
                        case AvatarIKGoal.RightHand:
                            targetColor = Color.yellow;
                            break;
                        case AvatarIKGoal.LeftFoot:
                            targetColor = Color.magenta;
                            break;
                        case AvatarIKGoal.RightFoot:
                            targetColor = Color.green;
                            break;
                        default:
                            Debug.Log("Invalid IK component!");
                            break;
                    }

                    Handles.color = targetColor;

                    Handles.CubeHandleCap(0, currentIK.target.position, currentIK.target.rotation, 0.05f, EventType.Repaint);
                
                    if (currentIK.hint != null)
                    {
                        Handles.CubeHandleCap(0, currentIK.hint.position, currentIK.hint.rotation, 0.05f, EventType.Repaint);
                    }
                }
                else
                {
                    // Assign IKs to editor
                    t.IkPositionList = t.transform.GetComponent<Point>().iks;
                }
            }
        }
    }

    [CustomEditor(typeof(DrawLineIndividual))]
    public class DrawLineVisual : Editor
    {
        void OnSceneGUI()
        {
            DrawLineIndividual t = target as DrawLineIndividual;

            if (t == null) { return; }

            if (t.ConnectedPoints.Count == 0)
            {
                t.ConnectedPoints.AddRange(t.transform.GetComponent<Point>().neighbors);
            }

            for (int i = 0; i < t.ConnectedPoints.Count; i++)
            {
                Neighbor currentNeighbor = t.ConnectedPoints[i];

                if (currentNeighbor.target == null) { continue; }

                Vector3 start = t.transform.position;
                Vector3 goal = currentNeighbor.target.transform.position;

                switch (currentNeighbor.cType)
                {
                    case ConnectionType.leap:
                        Handles.color = Color.red;
                        break;
                    case ConnectionType.inBetween:
                        Handles.color = Color.green;
                        break;
                }

                Handles.DrawLine(start, goal);
                t.refresh = false;
            }
        }
    }

    [CustomEditor(typeof(DrawLine))]
    public class EditorVisual : Editor
    {
        void OnSceneGUI()
        {
            DrawLine t = target as DrawLine;

            if (t == null) { return; }

            if (t.ConnectedPoints.Count == 0)
            {
                t.ConnectedPoints.AddRange(t.transform.GetComponent<GridManager>().GetAllConnections());
            }

            for (int i = 0; i < t.ConnectedPoints.Count; i++)
            {
                Vector3 point1 = t.ConnectedPoints[i].point1.transform.position;
                Vector3 point2 = t.ConnectedPoints[i].point2.transform.position;

                switch (t.ConnectedPoints[i].cType)
                {
                    case ConnectionType.leap:
                        Handles.color = Color.red;
                        break;
                    case ConnectionType.inBetween:
                        Handles.color = Color.green;
                        break;
                }

                Handles.DrawLine(point1, point2);
                t.refresh = false;
            }
        }
    }
}

#endif