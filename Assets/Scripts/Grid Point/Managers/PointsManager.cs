#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    [ExecuteInEditMode]
    public class PointsManager : MonoBehaviour
    {
        [Header("Helper properties")]
        public bool DismountPoint;
        public bool FallPoint;
        public bool HangingPoint;
        public bool SinglePoint;

        [Header("Point Spawn Utilities")]
        public bool spawnInBetweenPoints;

        [Header("Helper Utilities")]
        public bool deleteAll;
        public bool updatePointList;
        public bool createIndicators;
        public float pointGap = 0.5f;

        public GameObject pointPrefab;

        public Point leftMost;
        public Point rightMost;

        //[HideInInspector]
        public List<Point> pointsInOrder;

        void HandlePrefab()
        {
            pointPrefab = Resources.Load("Point") as GameObject;

            if (pointPrefab == null)
            {
                Debug.Log("Cannot find point prefab!");
            }
        }

        void UpdatePoints()
        {
            Point[] points = GetComponentsInChildren<Point>();

            if (SinglePoint)
            {
                pointsInOrder = new List<Point>();
                pointsInOrder.AddRange(points);
            }

            if (points.Length < 1)
            {
                Debug.Log("Error: failed to find edge point indicator!");
                return;
            }

            DeleteInBetween(points, leftMost, rightMost);

            points = null;
            points = GetComponentsInChildren<Point>();

            CreateInBetween(leftMost, rightMost);
        }

        void CreateSingleIndicator()
        {
            GameObject leftPoint = Instantiate(pointPrefab) as GameObject;
            leftPoint.transform.parent = transform;
            leftPoint.transform.localPosition = Vector3.zero;
            leftPoint.transform.localEulerAngles = Vector3.zero;
        }

        void CreatePairIndicators()
        {
            GameObject leftPoint = Instantiate(pointPrefab) as GameObject;
            leftPoint.name = "Left Most Point";
            GameObject rightPoint = Instantiate(pointPrefab) as GameObject;
            rightPoint.name = "Right Most Point";

            leftPoint.transform.parent = transform;
            leftPoint.transform.localPosition = -(Vector3.right / 2);
            leftPoint.transform.localEulerAngles = Vector3.zero;

            rightPoint.transform.parent = transform;
            rightPoint.transform.localPosition = (Vector3.right / 2);
            rightPoint.transform.localEulerAngles = Vector3.zero;

            leftMost = leftPoint.GetComponentInChildren<Point>();
            rightMost = rightPoint.GetComponentInChildren<Point>();
        }

        void DeleteAll()
        {
            Point[] points = GetComponentsInChildren<Point>();
            for (int i = 0; i < points.Length; i++)
            {
                DestroyImmediate(points[i].transform.parent.gameObject);
            }
        }

        void DeleteInBetween(Point[] points, Point leftEnd, Point rightEnd)
        {
            for (int i = 0; i < points.Length; i++)
            {
                // not two ends
                if ((points[i] != leftEnd) && (points[i] != rightEnd))
                {
                    DestroyImmediate(points[i].gameObject.transform.parent.gameObject);
                }
            }
        }

        void CreateInBetween(Point leftEnd, Point rightEnd)
        {
            float trailDistance = Vector3.Distance(GetPosition(leftEnd), GetPosition(rightEnd));
            int newPointCount = Mathf.FloorToInt(trailDistance / pointGap);
            Vector3 spawnDirection = (GetPosition(rightEnd) - GetPosition(leftEnd)).normalized;
            Vector3[] newPointPositions = new Vector3[newPointCount];

            float interval = 0;
            pointsInOrder = new List<Point>();
            pointsInOrder.Add(leftEnd);

            for (int i = 0; i < newPointCount; i++)
            {
                interval += pointGap;
                newPointPositions[i] = GetPosition(leftEnd) + (spawnDirection * interval);

                if (Vector3.Distance(newPointPositions[i], GetPosition(rightEnd)) > pointGap){
                    GameObject point = Instantiate(pointPrefab, newPointPositions[i], Quaternion.identity) as GameObject;
                    point.transform.parent = transform;
                    point.transform.rotation = leftEnd.transform.rotation;
                    pointsInOrder.Add(point.GetComponentInChildren<Point>());
                }
                else
                {
                    // Adjust right end to a more inward position for equal spacing
                    rightEnd.transform.parent.transform.localPosition = transform.InverseTransformPoint(newPointPositions[i]);
                    break;
                }
            }

            pointsInOrder.Add(rightEnd);
        }

        Vector3 GetPosition(Point p)
        {
            return p.transform.parent.position;
        }


        private void Update()
        {
            if (spawnInBetweenPoints)
            {
                HandlePrefab();
                UpdatePoints();
                spawnInBetweenPoints = false;
            }

            if (createIndicators)
            {
                HandlePrefab();

                if (SinglePoint) { CreateSingleIndicator(); }
                else { CreatePairIndicators(); }
                createIndicators = false;
            }

            if (deleteAll)
            {
                DeleteAll();
                deleteAll = false;
            }

            if (updatePointList)
            {
                Point[] points = GetComponentsInChildren<Point>();
                pointsInOrder = new List<Point>();
                pointsInOrder.AddRange(points);
                updatePointList = false;
            }
        }
    }
}

#endif