using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathCreator : MonoBehaviour
{ 
    public Path path;

    private Vector2[] data;

    void Start()
    {
        CreatePath();
    }
    public void CreatePath()
    {
        data = GenerateTrack();
        path = new Path(data[0]);
        for (int i = 1; i < data.Length; i++)
        {
            path.AddSegment(data[i]);
        }
        path.Close();
        path.AutoSetAllControlPoints();
        //UpdateRoad();
    }
    
    #region TrackPointGen
    
    private List<Vector2> points;
    private List<Vector2> dataSet;
    private Vector2[] rSet;
    private Vector2 disp;
    private int pointCount;
    private int i, j;
    float difficulty = 20f;
    float maxDisp = 20f;
    
    public Vector2[] GenerateTrack()
    {
        pointCount = Random.Range(10, 20);
        points = new List<Vector2>();
        for (i = 0; i < pointCount; i++)
        {
           
            float x = Random.Range(0.0f, 250f) - 125f;
            float y = Random.Range(0.0f, 250f) - 125f;
            points.Add(new Vector2(x, y));
        }

        dataSet = points;
        int pushIterations = 3;  
        for(int i = 0; i < pushIterations; ++i)  
        {  
            PushApart(dataSet);  
        }  
        rSet = new Vector2[dataSet.Count * 2];
        
        for (i = 0; i < dataSet.Count; i++)
        {
            float dispLen = (float) Math.Pow(Random.Range(0f, 1f), difficulty) * maxDisp;
            disp = new Vector2(0, 1);
            disp = Rotate(disp,Random.Range(0f,1f)*360);
            disp *= dispLen;
            rSet[i * 2] = dataSet[i];
            rSet[i * 2 + 1 ] = dataSet[i];
            rSet[i * 2 + 1] += dataSet[(i + 1) % dataSet.Count] * 0.5f + disp;
        }

        dataSet.Clear();
        for (i = 0; i < rSet.Length; i++)
            dataSet.Add(rSet[i]);
        
        for(int i = 0; i < pushIterations; ++i)  
        {  
            PushApart(dataSet);  
        }  
        for(int i = 0; i < 10; ++i)  
        {  
            FixAngles(dataSet);  
            PushApart(dataSet);  
        }

        return dataSet.ToArray();
    }

    private float dst, dst2, hx, hy, hl, dif;
    void PushApart(List<Vector2> dataSet)
    { 
        dst = 15;
        dst2 = dst * dst;
        for (i = 0; i < dataSet.Count; i++)
        for (i = i + 1; j < dataSet.Count; j++)
            if (Vector2.Distance(dataSet[i], dataSet[j]) < dst2)
            {
                hx = dataSet[j].x - dataSet[i].x; 
                hy = dataSet[j].y - dataSet[i].y;  
                hl = (float)Math.Sqrt(hx*hx + hy*hy);  
                hx /= hl;   
                hy /= hl;  
                dif = dst - hl;  
                hx *= dif;  
                hy *= dif;  
                dataSet[j] += new Vector2(hx,hy);
                dataSet[i] -= new Vector2(hx,hy);
            }
    }

    private float tx, ty;
    Vector2 Rotate(Vector2 v, float degrees) 
    {
        sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
         
        tx = v.x;
        ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
    
    int previous, next;
    float px, py, nx, ny, pl, nl, a, nA, diff, cos, sin, newX, newY;
    
    void FixAngles(List<Vector2> dataSet)
    {
        for (i = 0; i < dataSet.Count; i++)
        {
            previous = i - 1 < 0 ? dataSet.Count - 1 : i - 1;
            next = (i + 1) % dataSet.Count;
            px = dataSet[i].x - dataSet[previous].x;
            py = dataSet[i].y - dataSet[previous].y;
            nx = -(dataSet[i].x - dataSet[next].x);
            ny = -(dataSet[i].y - dataSet[next].y);
            pl = (float) Math.Sqrt(px * px + py * py);
            nl = (float) Math.Sqrt(nx * nx + ny * ny);
            px /= pl;
            py /= pl;
            nx /= nl;
            ny /= nl;

            a = (float) Math.Atan2(px * ny - py * nx, px * nx + py * ny);

            if (Mathf.Abs(a * Mathf.Rad2Deg) <= 100) continue;

            nA = 100 * Mathf.Sign(a) * Mathf.Deg2Rad;
            diff = nA - a;
            cos = Mathf.Cos(diff);
            sin = Mathf.Sin(diff);
            newX = nx * cos - ny * sin;
            newY = nx * sin + ny * cos;
            newX *= nl;
            newY *= nl;
            dataSet[next] = new Vector2(dataSet[i].x + newX, dataSet[i].y + newY);
        }            
    }
    
    #endregion
    
    #region MeshGen

    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private float spacing = 1;
    [SerializeField] private float roadWidth = 1;
    private Vector2[] meshPoints;
    private Vector3[] vertex;
    public void UpdateRoad()
    {
        meshPoints = path.CalculateEvenlySpacedPoints(spacing);
        meshFilter.mesh = CreateRoadMesh(meshPoints);
    }

    Mesh CreateRoadMesh(Vector2[] points)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        int[] tris = new int[(2 * (points.Length - 1) +2)  * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 forward = Vector2.zero;
            if (i < points.Length - 1)
            {
                forward += points[(i + 1)%points.Length] - points[i];
            }

            if (i > 0)
            {
                forward += points[i] - points[(i - 1 + points.Length)%points.Length];
            }
            forward.Normalize();
            Vector2 left = new Vector2(-forward.y, forward.x);

            verts[vertIndex] = points[i] + left * roadWidth * .5f;
            verts[vertIndex + 1] = points[i] - left * roadWidth * .5f;
            

            if (i < points.Length - 1)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex+1] = (vertIndex+2) % verts.Length;
                tris[triIndex+2] = vertIndex+1;
                
                tris[triIndex+3] = vertIndex+1;
                tris[triIndex+4] = (vertIndex+2) % verts.Length;
                tris[triIndex+5] = (vertIndex+3) % verts.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        return mesh;
    }
    
    #endregion
}
