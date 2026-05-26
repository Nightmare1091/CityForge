using UnityEngine;
using System.Collections.Generic;

public class RoadNetwork : MonoBehaviour
{
    public static RoadNetwork Instance { get; private set; }

    [Header("Settings")]
    public Material roadMaterial;
    public Material roadInvalidMaterial;
    public float mergeNodeRadius = 0.5f;

    private List<RoadNode> nodes = new List<RoadNode>();
    private List<RoadEdge> edges = new List<RoadEdge>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void HighlightOverlappingEdges(List<RoadEdge> overlapping)
    {
        ClearAllHighlights();
        foreach (var edge in overlapping)
            edge.SetHighlight(roadMaterial, roadInvalidMaterial);
    }

    public void ClearAllHighlights()
    {
        foreach (var edge in edges)
            edge.ClearHighlight(roadMaterial);
    }

    public void AddRoad(Vector3 start, Vector3? control, Vector3 end, float width, int segments)
    {
        RoadNode startNode = GetOrCreateNode(start);
        RoadNode endNode = GetOrCreateNode(end);

        Vector3 midPoint = control.HasValue
            ? 0.25f * start + 0.5f * control.Value + 0.25f * end
            : Vector3.Lerp(start, end, 0.5f);

        RoadNode midNode = GetOrCreateNode(midPoint);

        Vector3? controlA = null;
        Vector3? controlB = null;

        if (control.HasValue)
        {
            controlA = Vector3.Lerp(start, control.Value, 0.5f);
            controlB = Vector3.Lerp(control.Value, end, 0.5f);
        }

        RoadEdge edgeA = new RoadEdge(startNode, midNode, controlA, width, segments / 2);
        RoadEdge edgeB = new RoadEdge(midNode, endNode, controlB, width, segments / 2);

        startNode.edges.Add(edgeA);
        midNode.edges.Add(edgeA);
        midNode.edges.Add(edgeB);
        endNode.edges.Add(edgeB);

        edges.Add(edgeA);
        edges.Add(edgeB);

        var affected = new HashSet<RoadEdge> { edgeA, edgeB };
        foreach (var e in startNode.edges) affected.Add(e);
        foreach (var e in midNode.edges) affected.Add(e);
        foreach (var e in endNode.edges) affected.Add(e);
        foreach (var e in affected) e.BuildMesh(roadMaterial);

        startNode.RebuildJunction(roadMaterial);
        midNode.RebuildJunction(roadMaterial);
        endNode.RebuildJunction(roadMaterial);

        ProcessIntersections(edgeA);
        ProcessIntersections(edgeB);
    }

    void ProcessIntersections(RoadEdge newEdge)
    {
        var hits = IntersectionDetector.FindIntersections(newEdge, edges);
        if (hits.Count == 0) return;

        var nodesToRebuild = new HashSet<RoadNode>();

        foreach (var hit in hits)
        {
            RoadNode existingNearby = FindNearbyNode(hit.point, roadMaterial != null ? 6f : 6f);
            if (existingNearby != null)
            {
                nodesToRebuild.Add(existingNearby);
                continue;
            }

            RoadNode intersectionNode = SplitEdge(hit.edge, hit.point);
            nodesToRebuild.Add(intersectionNode);
        }

        for (int i = hits.Count - 1; i >= 0; i--)
        {
            RoadNode existingNearby = FindNearbyNode(hits[i].point, 6f);
            if (existingNearby != null) continue;

            if (edges.Contains(newEdge))
                SplitEdge(newEdge, hits[i].point);
        }

        var affectedEdges = new HashSet<RoadEdge>();
        foreach (var node in nodesToRebuild)
            foreach (var e in node.edges) affectedEdges.Add(e);

        foreach (var e in affectedEdges) e.BuildMesh(roadMaterial);
        foreach (var node in nodesToRebuild) node.RebuildJunction(roadMaterial);
    }

    RoadNode FindNearbyNode(Vector3 position, float radius)
    {
        foreach (var node in nodes)
        {
            if (Vector2.Distance(
                new Vector2(node.position.x, node.position.z),
                new Vector2(position.x, position.z)) < radius)
                return node;
        }
        return null;
    }

    RoadNode SplitEdge(RoadEdge edge, Vector3 splitPoint)
    {
        RoadNode newNode = GetOrCreateNode(splitPoint);
        if (newNode == edge.startNode || newNode == edge.endNode) return newNode;

        if (edge.startNode.edges.Count >= 4 || edge.endNode.edges.Count >= 4)
            return null;
        
        if (newNode == edge.startNode || newNode == edge.endNode) return newNode;

        RoadNode oldStart = edge.startNode;
        RoadNode oldEnd = edge.endNode;

        oldStart.edges.Remove(edge);
        oldEnd.edges.Remove(edge);
        edges.Remove(edge);
        if (edge.edgeObject != null) Object.Destroy(edge.edgeObject);

        Vector3? controlA = null;
        Vector3? controlB = null;

        if (edge.controlPoint.HasValue)
        {
            Vector3 cp = edge.controlPoint.Value;
            controlA = Vector3.Lerp(oldStart.position, cp, 0.5f);
            controlB = Vector3.Lerp(cp, oldEnd.position, 0.5f);
        }

        RoadEdge edgeA = new RoadEdge(oldStart, newNode, controlA, edge.roadWidth, edge.meshSegments / 2);
        RoadEdge edgeB = new RoadEdge(newNode, oldEnd, controlB, edge.roadWidth, edge.meshSegments / 2);

        oldStart.edges.Add(edgeA);
        newNode.edges.Add(edgeA);
        newNode.edges.Add(edgeB);
        oldEnd.edges.Add(edgeB);

        edges.Add(edgeA);
        edges.Add(edgeB);

        return newNode;
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

    public List<RoadEdge> GetAllEdges()
    {
        return edges;
    }
}