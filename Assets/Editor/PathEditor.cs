using System;
using System.Collections;
using System.Collections.Generic;
using Codice.Client.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    private PathCreator creator;
    private Path path;


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate New Track"))
        {
            creator.CreatePath();
            path = creator.path;
            SceneView.RepaintAll();
        }
        
        if (GUILayout.Button("Auto Set"))
        {
            path.AutoSetAllControlPoints();
            SceneView.RepaintAll();
        }
         
        if (GUILayout.Button("Create Mesh"))
        {
            creator.UpdateRoad();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Create Point"))
        {
            foreach (var p in path.CalculateEvenlySpacedPoints(6))
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = p;
                go.transform.localScale = Vector3.one*1;
            }
            SceneView.RepaintAll();
        }
    }

    void OnSceneGUI()
    {
        if(creator.path != null) Draw();
    }
    
    void Draw()
    {
        if (path is null) return;
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector2[] points = path.GetPointsInSegment(i);
            Handles.color = Color.black;
            Handles.DrawLine(points[0],points[1]);
            Handles.DrawLine(points[3],points[2]);
            Handles.DrawBezier(points[0],points[3],points[1],points[2],Color.green, null,2);
        }
    }

    private void OnEnable()
    {
        creator = (PathCreator) target;
    }
}
