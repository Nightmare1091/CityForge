using UnityEngine;
using System.Collections.Generic;

public class RoadEdge
{
    public RoadNode startNode;
    public RoadNode endNode;
    public Vector3? controlPoint;
    public float roadWidth;
    public int meshSegments;
    public GameObject edgeObject;

    public RoadEdge(RoadNode start, RoadNode end, Vector3? control, float width, int segments)
    {
        startNode = start;
        endNode = end;
        controlPoint = control;
        roadWidth = width;
        meshSegments = segments;
    }

    public void BuildMesh(Material material)
    {
        if (edgeObject != null) Object.Destroy(edgeObject);

        float overlapFactor = 0.1f;
        float startRadius = startNode.edges.Count > 1 ? (roadWidth / 2f) * overlapFactor : 0f;
        float endRadius = endNode.edges.Count > 1 ? (roadWidth / 2f) * overlapFactor : 0f;

        List<Vector3> points = controlPoint.HasValue
            ? GenerateCurvePoints(startNode.position, controlPoint.Value, endNode.position, startRadius, endRadius)
            : GenerateStraightPoints(startNode.position, endNode.position, startRadius, endRadius);

        if (points.Count < 2) return;

        edgeObject = new GameObject("RoadEdge");
        edgeObject.AddComponent<MeshFilter>().mesh = GenerateMesh(points);
        edgeObject.AddComponent<MeshRenderer>().material = material;
        edgeObject.AddComponent<MeshCollider>().sharedMesh =
            edgeObject.GetComponent<MeshFilter>().sharedMesh;
        edgeObject.tag = "Road";
    }

    List<Vector3> GenerateStraightPoints(Vector3 start, Vector3 end, float trimStart, float trimEnd)
    {
        Vector3 dir = (end - start).normalized;
        Vector3 tStart = start + dir * trimStart;
        Vector3 tEnd = end - dir * trimEnd;

        var points = new List<Vector3>();
        for (int i = 0; i <= meshSegments; i++)
        {
            float t = i / (float)meshSegments;
            points.Add(Vector3.Lerp(tStart, tEnd, t));
        }
        return points;
    }

    List<Vector3> GenerateCurvePoints(Vector3 p0, Vector3 p1, Vector3 p2, float trimStart, float trimEnd)
    {
        var full = new List<Vector3>();
        for (int i = 0; i <= meshSegments * 4; i++)
        {
            float t = i / (float)(meshSegments * 4);
            float u = 1 - t;
            full.Add(u * u * p0 + 2 * u * t * p1 + t * t * p2);
        }

        float totalLength = 0f;
        for (int i = 1; i < full.Count; i++)
            totalLength += Vector3.Distance(full[i], full[i - 1]);

        float startDist = trimStart;
        float endDist = totalLength - trimEnd;

        var trimmed = new List<Vector3>();
        float accumulated = 0f;

        for (int i = 1; i < full.Count; i++)
        {
            accumulated += Vector3.Distance(full[i], full[i - 1]);
            if (accumulated >= startDist && accumulated <= endDist)
                trimmed.Add(full[i]);
        }

        if (trimmed.Count < 2)
            trimmed = full; 

        return trimmed;
    }

    Mesh GenerateMesh(List<Vector3> points)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();
        float length = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 dir = i < points.Count - 1
                ? (points[i + 1] - points[i]).normalized
                : (points[i] - points[i - 1]).normalized;

            Vector3 right = new Vector3(dir.z, 0, -dir.x);
            Vector3 elevation = Vector3.up * 0.02f;

            vertices.Add(points[i] - right * (roadWidth / 2f) + elevation);
            vertices.Add(points[i] + right * (roadWidth / 2f) + elevation);

            if (i > 0) length += Vector3.Distance(points[i], points[i - 1]);

            uvs.Add(new Vector2(0, length / roadWidth));
            uvs.Add(new Vector2(1, length / roadWidth));

            if (i > 0)
            {
                int b = (i - 1) * 2;
                triangles.Add(b); triangles.Add(b + 2); triangles.Add(b + 1);
                triangles.Add(b + 1); triangles.Add(b + 2); triangles.Add(b + 3);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
}