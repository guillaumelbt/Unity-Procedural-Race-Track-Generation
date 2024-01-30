using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Path
{
    [SerializeField, HideInInspector] private List<Vector2> points;

    public Path(Vector2 center)
    {
        points = new List<Vector2>()
        {
            center + Vector2.left,
            center + (Vector2.left + Vector2.up) * .5f,
            center + (Vector2.right + Vector2.down) * .5f,
            center + Vector2.right,
        };
    }
   
   
    public Vector2 this[int i] { get { return points[i]; } }
    
    public int NumPoints { get { return points.Count; } }

    public int NumSegments { get { return ((points.Count - 4) / 3) + 2; } }
    
    public void AddSegment(Vector2 anchorPos)
    {
        points.Add(points[points.Count - 1] * 2 - points[points.Count-2]);
        points.Add((points[points.Count - 1] + anchorPos)*.5f);
        points.Add(anchorPos);
    }

    public Vector2[] GetPointsInSegment(int i)
    {
        return new Vector2[] { points[LoopIndex(i * 3)], points[LoopIndex(i * 3 + 1)], points[LoopIndex(i*3+2)], points[LoopIndex(i*3+3)] };
    }
    

    public void Close()
    {
        points.Add(points[points.Count - 1] * 2 - points[points.Count-2]);
        points.Add(points[0] * 2 - points[1]);

    }

    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(points[0]);
        Vector2 previousPoint = points[0];
        float dstSinceLastEvenPoint = 0;
        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Vector2[] p = GetPointsInSegment(segmentIndex);
            float controlNetLenght =
                Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2],p[3]);
            float estimatedCurveLenght = Vector2.Distance(p[0], p[3]) + controlNetLenght /2;
            int divisions = Mathf.CeilToInt(estimatedCurveLenght * resolution * 10);
            float t = 0;
            while (t <= 1)
            {
                t += 1f/divisions;
                Vector2 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);
                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overshootDst = dstSinceLastEvenPoint - spacing;
                    Vector2 newEvenlySpacedPoint =
                        pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDst;
                    previousPoint = newEvenlySpacedPoint;
                }
                previousPoint = pointOnCurve;
            }
        }

        return evenlySpacedPoints.ToArray();
    }
    int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }
    

    public void AutoSetAllControlPoints()
    {
        for (int i = 0; i < points.Count; i += 3)
        {
            AutoSetAnchorControlPoints(i);
        }
    }
    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector2 anchorPos = points[anchorIndex];
        Vector2 dir = Vector2.zero;

        float[] neighbourDistances = new float[2];

        if (anchorIndex - 3 >= 0)
        {
            Vector2 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }
        if (anchorIndex + 3 >= 0)
        {
            Vector2 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < points.Count)
            {
                points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * .5f;
            }
        }
        points[1] = (points[0] + points[2]) * .5f;
        points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * .5f;
        
    }
}
