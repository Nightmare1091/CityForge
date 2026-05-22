using UnityEngine;
using System.Collections.Generic;

public class RoadNetwork : MonoBehaviour
{
    public static RoadNetwork Instance { get; private set; }

    [Header("Settings")]
    public Material roadMaterial;
    public float mergeNodeRadius = 0.5f;

    private List<RoadNode> nodes = new List<RoadNode>();
    private List<RoadEdge> edges = new List<RoadEdge>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddRoad(Vector3 start, Vector3? control, Vector3 end, float width, int segments)
    {
        RoadNode startNode = GetOrCreateNode(start);
        RoadNode endNode = GetOrCreateNode(end);

        RoadEdge edge = new RoadEdge(startNode, endNode, control, width, segments);

        startNode.edges.Add(edge);
        endNode.edges.Add(edge);
        edges.Add(edge);

        var affectedEdges = new HashSet<RoadEdge>();
        foreach (var e in startNode.edges) affectedEdges.Add(e);
        foreach (var e in endNode.edges) affectedEdges.Add(e);
        foreach (var e in affectedEdges) e.BuildMesh(roadMaterial);

        startNode.RebuildJunction(roadMaterial);
        endNode.RebuildJunction(roadMaterial);
    }

    RoadNode GetOrCreateNode(Vector3 position)
    {
        foreach (var node in nodes)
        {
            if (Vector3.Distance(
                new Vector3(node.position.x, 0, node.position.z),
                new Vector3(position.x, 0, position.z)) < mergeNodeRadius)
                return node;
        }

        var newNode = new RoadNode(position);
        nodes.Add(newNode);
        return newNode;
    }

    public List<Vector3> GetAllNodePositions()
    {
        var positions = new List<Vector3>();
        foreach (var node in nodes)
            positions.Add(node.position);
        return positions;
    }
}