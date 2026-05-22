using UnityEngine;
using System.Collections.Generic;

public static class IntersectionDetector
{
    public static List<IntersectionHit> FindIntersections(RoadEdge newEdge, List<RoadEdge> existingEdges)
    {
        var hits = new List<IntersectionHit>();

        List<Vector3> newPoints = GetEdgePoints(newEdge);

        foreach (var existing in existingEdges)
        {
            if (existing == newEdge) continue;
            if (SharesNode(newEdge, existing)) continue;

            List<Vector3> existingPoints = GetEdgePoints(existing);

            for (int i = 0; i < newPoints.Count - 1; i++)
            {
                for (int j = 0; j < existingPoints.Count - 1; j++)
                {
                    Vector3 a1 = newPoints[i];
                    Vector3 a2 = newPoints[i + 1];
                    Vector3 b1 = existingPoints[j];
                    Vector3 b2 = existingPoints[j + 1];

                    if (SegmentsIntersect(a1, a2, b1, b2, out Vector3 hitPoint))
                    {
                        hits.Add(new IntersectionHit(hitPoint, existing));
                    }
                }
            }
        }

        hits.Sort((a, b) =>
            Vector3.Distance(newEdge.startNode.position, a.point)
            .CompareTo(Vector3.Distance(newEdge.startNode.position, b.point))
        );

        return hits;
    }

    static bool SharesNode(RoadEdge a, RoadEdge b)
    {
        return a.startNode == b.startNode || a.startNode == b.endNode ||
               a.endNode == b.startNode || a.endNode == b.endNode;
    }

    static List<Vector3> GetEdgePoints(RoadEdge edge)
    {
        if (edge.controlPoint.HasValue)
            return GenerateCurvePoints(
                edge.startNode.position,
                edge.controlPoint.Value,
                edge.endNode.position,
                edge.meshSegments);
        else
            return GenerateStraightPoints(
                edge.startNode.position,
                edge.endNode.position,
                edge.meshSegments);
    }

    static List<Vector3> GenerateStraightPoints(Vector3 start, Vector3 end, int segments)
    {
        var points = new List<Vector3>();
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            points.Add(Vector3.Lerp(start, end, t));
        }
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

    static bool SegmentsIntersect(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        float d1x = a2.x - a1.x, d1z = a2.z - a1.z;
        float d2x = b2.x - b1.x, d2z = b2.z - b1.z;

        float cross = d1x * d2z - d1z * d2x;

        if (Mathf.Abs(cross) < 0.0001f) return false;

        float dx = b1.x - a1.x;
        float dz = b1.z - a1.z;

        float t = (dx * d2z - dz * d2x) / cross;
        float u = (dx * d1z - dz * d1x) / cross;

        if (t < 0.01f || t > 0.99f || u < 0.01f || u > 0.99f) return false;

        hitPoint = new Vector3(
            a1.x + t * d1x,
            0.02f,
            a1.z + t * d1z
        );

        return true;
    }
}

public class IntersectionHit
{
    public Vector3 point;
    public RoadEdge edge;

    public IntersectionHit(Vector3 p, RoadEdge e)
    {
        point = p;
        edge = e;
    }
}