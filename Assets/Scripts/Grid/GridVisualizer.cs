using UnityEngine;
using System.Collections.Generic;

public class GridVisualizer : MonoBehaviour
{
    public static GridVisualizer Instance { get; private set; }

    [Header("Settings")]
    public int visibleSections = 6;
    public float lineWidth = 0.15f;
    public Color sectionColor = new Color(0.4f, 0.8f, 1f, 0.6f);
    public Color cellColor = new Color(0.4f, 0.8f, 1f, 0.15f);

    [Header("References")]
    public Material lineMaterial;

    private List<GameObject> lineObjects = new List<GameObject>();
    private bool isVisible = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void UpdateGrid(Vector3 origin, Vector3 direction, float roadLength, float roadWidth)
    {
        if (!isVisible) return;
        ClearLines();

        direction.y = 0;
        if (direction.magnitude < 0.01f) return;
        direction.Normalize();

        Vector3 perpendicular = new Vector3(-direction.z, 0, direction.x);
        float sectionSize = GridSystem.Instance.sectionSize;
        float cellSize = GridSystem.Instance.cellSize;
        float halfWidth = roadWidth * 0.6f;

        int sections = Mathf.Max(1, Mathf.CeilToInt(roadLength / sectionSize));

        for (int i = 0; i <= sections; i++)
        {
            Vector3 center = origin + direction * sectionSize * i;
            center = GridSystem.Instance.Snap(center);

            DrawLine(
                center - perpendicular * halfWidth,
                center + perpendicular * halfWidth,
                sectionColor, lineWidth
            );

            if (i < sections)
            {
                for (int c = 1; c < Mathf.RoundToInt(sectionSize / cellSize); c++)
                {
                    Vector3 cellCenter = center + direction * cellSize * c;
                    DrawLine(
                        cellCenter - perpendicular * halfWidth * 0.5f,
                        cellCenter + perpendicular * halfWidth * 0.5f,
                        cellColor, lineWidth * 0.5f
                    );
                }
            }
        }

        DrawLine(
            origin - perpendicular * halfWidth,
            origin + direction * roadLength - perpendicular * halfWidth,
            cellColor, lineWidth * 0.5f
        );
        DrawLine(
            origin + perpendicular * halfWidth,
            origin + direction * roadLength + perpendicular * halfWidth,
            cellColor, lineWidth * 0.5f
        );
    }

    public void Show() { isVisible = true; }

    public void Hide()
    {
        isVisible = false;
        ClearLines();
    }

    void DrawLine(Vector3 start, Vector3 end, Color color, float width)
    {
        GameObject obj = new GameObject("GridLine");
        obj.transform.SetParent(transform);
        lineObjects.Add(obj);

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        lr.SetPosition(0, start + Vector3.up * 0.05f);
        lr.SetPosition(1, end + Vector3.up * 0.05f);
    }

    void ClearLines()
    {
        foreach (var obj in lineObjects)
            if (obj != null) Destroy(obj);
        lineObjects.Clear();
    }
}