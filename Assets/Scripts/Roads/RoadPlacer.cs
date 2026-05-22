using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

[RequireComponent(typeof(SplineContainer))]
public class RoadPlacer : MonoBehaviour
{
    public enum RoadMode { None, Straight, Curved }
    public enum DrawStage { Idle, DefiningEnd, DefiningControl }

    [Header("Settings")]
    public float roadWidth = 8f;
    public int meshSegments = 20;
    //public Material roadMaterial;
    public Material previewMaterial;
    public LayerMask groundLayer;

    [HideInInspector] public RoadMode currentMode = RoadMode.None;

    private DrawStage stage = DrawStage.Idle;
    private Vector3 startPoint;
    private Vector3 controlPoint;

    private GameObject previewObj;
    private Camera cam;
    private SplineContainer splineContainer;

    void Start()
    {
        cam = Camera.main;
        splineContainer = GetComponent<SplineContainer>();
        CreatePreview();
    }

    void Update()
    {
        if (currentMode == RoadMode.None) return;

        if (Input.GetKeyDown(KeyCode.Escape)) { Reset(); return; }

        Vector3 rawPos = GetGroundPosition();
        Vector3 mousePos = SnapManager.Instance.GetSnappedPosition(rawPos, out bool snapped);

        if (stage == DrawStage.Idle)
        {
            if (Input.GetMouseButtonDown(0))
            {
                startPoint = mousePos;
                stage = DrawStage.DefiningEnd;
            }
        }
        else if (stage == DrawStage.DefiningEnd)
        {
            UpdateStraightPreview(startPoint, mousePos);

            if (Input.GetMouseButtonDown(0))
            {
                if (currentMode == RoadMode.Straight)
                {
                    ConfirmRoad(startPoint, null, mousePos);
                    startPoint = mousePos; 
                    stage = DrawStage.DefiningEnd;
                }
                else 
                {
                    controlPoint = mousePos;
                    stage = DrawStage.DefiningControl;
                }
            }
            else if (Input.GetMouseButtonDown(1)) Reset();
        }
        else if (stage == DrawStage.DefiningControl)
        {
            UpdateCurvedPreview(startPoint, mousePos, controlPoint);

            if (Input.GetMouseButtonDown(0))
            {
                ConfirmRoad(startPoint, mousePos, controlPoint);
                startPoint = controlPoint; 
                stage = DrawStage.DefiningEnd;
            }
            else if (Input.GetMouseButtonDown(1)) Reset();
        }
    }

    public void SetMode(RoadMode mode)
    {
        currentMode = mode;
        Reset();
        Debug.Log($"Road mode: {mode}");
    }

    void CreatePreview()
    {
        previewObj = new GameObject("RoadPreview");
        previewObj.AddComponent<MeshFilter>();
        previewObj.AddComponent<MeshRenderer>().material = previewMaterial;
        previewObj.SetActive(false);
    }

    void UpdateStraightPreview(Vector3 start, Vector3 end)
    {
        previewObj.GetComponent<MeshFilter>().mesh = GenerateMesh(GenerateStraightPoints(start, end));
        previewObj.SetActive(true);
    }

    void UpdateCurvedPreview(Vector3 start, Vector3 control, Vector3 end)
    {
        previewObj.GetComponent<MeshFilter>().mesh = GenerateMesh(GenerateCurvePoints(start, control, end));
        previewObj.SetActive(true);
    }

    void ConfirmRoad(Vector3 start, Vector3? control, Vector3 end)
    {
        RoadNetwork.Instance.AddRoad(start, control, end, roadWidth, meshSegments);
        RegisterSpline(start, control, end);
    }

    void RegisterSpline(Vector3 start, Vector3? control, Vector3 end)
    {
        Spline spline = new Spline();

        if (control.HasValue)
        {
            var knotA = new BezierKnot(start);
            var knotB = new BezierKnot(end);
            float3 tangent = (float3)(control.Value - start);
            knotA.TangentOut = tangent;
            knotB.TangentIn = -tangent;
            spline.Add(knotA);
            spline.Add(knotB);
        }
        else
        {
            spline.Add(new BezierKnot(start));
            spline.Add(new BezierKnot(end));
        }

        splineContainer.AddSpline(spline);
    }

    List<Vector3> GenerateStraightPoints(Vector3 start, Vector3 end)
    {
        var points = new List<Vector3>();
        for (int i = 0; i <= meshSegments; i++)
        {
            float t = i / (float)meshSegments;
            points.Add(Vector3.Lerp(start, end, t));
        }
        return points;
    }

    List<Vector3> GenerateCurvePoints(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        var points = new List<Vector3>();
        for (int i = 0; i <= meshSegments; i++)
        {
            float t = i / (float)meshSegments;
            float u = 1 - t;
            points.Add(u * u * p0 + 2 * u * t * p1 + t * t * p2);
        }
        return points;
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

    void Reset()
    {
        stage = DrawStage.Idle;
        previewObj.SetActive(false);
        SnapManager.Instance.HideIndicator();
    }

    Vector3 GetGroundPosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
            return hit.point;

        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (ground.Raycast(ray, out float dist))
            return ray.GetPoint(dist);

        return Vector3.zero;
    }
}