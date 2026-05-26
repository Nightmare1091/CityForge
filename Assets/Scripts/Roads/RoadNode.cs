using UnityEngine;
using System.Collections.Generic;

public class RoadNode
{
    public Vector3 position;
    public List<RoadEdge> edges = new List<RoadEdge>();
    public GameObject nodeObject;

    public RoadNode(Vector3 pos)
    {
        position = pos;
    }

    public void RebuildJunction(Material material)
    {
        if (nodeObject != null) Object.Destroy(nodeObject);
        if (edges.Count == 0) return;

        float radius = GetJunctionRadius();

        nodeObject = new GameObject("RoadNode");
        nodeObject.layer = LayerMask.NameToLayer("Road");
        nodeObject.transform.position = position;
        nodeObject.AddComponent<MeshFilter>().mesh = GenerateJunctionMesh(radius);
        nodeObject.AddComponent<MeshRenderer>().material = material;
        nodeObject.tag = "Road";
    }

    float GetJunctionRadius()
    {
        float maxWidth = 0f;
        foreach (var edge in edges)
            if (edge.roadWidth > maxWidth) maxWidth = edge.roadWidth;
        return maxWidth / 2f;
    }

    Mesh GenerateJunctionMesh(float radius)
    {
        int segments = 24;
        var vertices = new Vector3[segments + 1];
        var triangles = new int[segments * 3];
        var uvs = new Vector2[segments + 1];

        vertices[0] = Vector3.up * 0.03f;
        uvs[0] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            vertices[i + 1] = new Vector3(x, 0.03f, z);
            uvs[i + 1] = new Vector2(x / radius * 0.5f + 0.5f, z / radius * 0.5f + 0.5f);

            int next = (i + 1) % segments + 1;
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = next; 
            triangles[i * 3 + 2] = i + 1;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}