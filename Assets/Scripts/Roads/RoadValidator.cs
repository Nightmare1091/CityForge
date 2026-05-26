using UnityEngine;
using System.Collections.Generic;

public static class RoadValidator
{
    public const float MIN_ROAD_LENGTH = 1f;
    public const float OVERLAP_THRESHOLD = 0.45f;

    public static bool IsValidPlacement(Vector3 start, Vector3? control, Vector3 end,
                                     float roadWidth, List<RoadEdge> existingEdges)
    {
        if (!HasMinimumLength(start, control, end)) return false;
        if (ConnectsWithinSameEdge(start, end, existingEdges)) return false;
        if (HasOverlap(start, control, end, roadWidth, existingEdges)) return false;
        return true;
    }

    static bool ConnectsWithinSameEdge(Vector3 start, Vector3 end, List<RoadEdge> existingEdges)
    {
        RoadNode startNode = null;
        RoadNode endNode = null;

        foreach (var edge in existingEdges)
        {
            if (Vector3.Distance(start, edge.startNode.position) < 0.5f) startNode = edge.startNode;
            if (Vector3.Distance(start, edge.endNode.position) < 0.5f) startNode = edge.endNode;
            if (Vector3.Distance(end, edge.startNode.position) < 0.5f) endNode = edge.startNode;
            if (Vector3.Distance(end, edge.endNode.position) < 0.5f) endNode = edge.endNode;
        }

        if (startNode == null || endNode == null) return false;
        if (startNode == endNode) return true;

        foreach (var edge in startNode.edges)
        {
            if (edge.startNode == endNode || edge.endNode == endNode)
                return true;
        }

        return false;
    }

    static bool HasMinimumLength(Vector3 start, Vector3? control, Vector3 end)
    {
        if (control.HasValue)
        {
            float length = 0f;
            Vector3 prev = start;
            for (int i = 1; i <= 8; i++)
            {
                float t = i / 8f;
                float u = 1 - t;
                Vector3 p = u * u * start + 2 * u * t * control.Value + t * t * end;
                length += Vector3.Distance(prev, p);
                prev = p;
            }
            return length >= MIN_ROAD_LENGTH;
        }
        return Vector3.Distance(start, end) >= MIN_ROAD_LENGTH;
    }

    static bool HasOverlap(Vector3 start, Vector3? control, Vector3 end,
                       float roadWidth, List<RoadEdge> existingEdges)
    {
        List<Vector3> newPoints = control.HasValue
            ? GenerateCurvePoints(start, control.Value, end, 20)
            : GenerateStraightPoints(start, end, 20);

        float minDistance = roadWidth * 0.8f;

        foreach (var edge in existingEdges)
        {
            List<Vector3> edgePoints = edge.controlPoint.HasValue
                ? GenerateCurvePoints(edge.startNode.position, edge.controlPoint.Value, edge.endNode.position, 20)
                : GenerateStraightPoints(edge.startNode.position, edge.endNode.position, 20);

            int blocked = 0;
            int total = 0;

            for (int i = 2; i < newPoints.Count - 2; i++)
            {
                Vector2 np2D = new Vector2(newPoints[i].x, newPoints[i].z);

                float closest = float.MaxValue;
                int closestJ = 0;
                for (int j = 0; j < edgePoints.Count; j++)
                {
                    float d = Vector2.Distance(np2D, new Vector2(edgePoints[j].x, edgePoints[j].z));
                    if (d < closest) { closest = d; closestJ = j; }
                }

                if (closest > minDistance * 3f) continue;
                total++;

                if (closest >= minDistance) continue;

                Vector3 newDir = GetDirectionAt(newPoints, i);
                Vector3 edgeDir = GetDirectionAt(edgePoints, closestJ);
                float angle = Vector3.Angle(newDir, edgeDir);
                if (angle > 90f) angle = 180f - angle;

                if (angle < 35f) blocked++;
            }

            if (total > 3 && blocked > total * 0.4f) return true;
        }

        return false;
    }

    static Vector3 GetDirectionAt(List<Vector3> points, int index)
    {
        if (index < points.Count - 1)
            return (points[index + 1] - points[index]).normalized;
        if (index > 0)
            return (points[index] - points[index - 1]).normalized;
        return Vector3.forward;
    }

    static List<Vector3> GenerateStraightPoints(Vector3 start, Vector3 end, int segments)
    {
        var points = new List<Vector3>();
        for (int i = 0; i <= segments; i++)
            points.Add(Vector3.Lerp(start, end, i / (float)segments));
        return points;
    }

    static List<Vector3> GenerateCurvePoints(Vector3 p0, Vector3 p1, Vector3 p2, int segments)
    {
        var points = new List<Vector3>();
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float u = 1 - t;
            points.Add(u * u * p0 + 2 * u * t * p1 + t * t * p2);
        }
        return points;
    }
}