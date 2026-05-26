using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance { get; private set; }

    [Header("Settings")]
    public float cellSize = 1f;
    public float sectionSize = 16f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public Vector3 Snap(Vector3 worldPosition)
    {
        float x = Mathf.Round(worldPosition.x / cellSize) * cellSize;
        float z = Mathf.Round(worldPosition.z / cellSize) * cellSize;
        return new Vector3(x, worldPosition.y, z);
    }

    public bool IsSubsectionBoundary(Vector3 position)
    {
        float x = Mathf.Abs(position.x % sectionSize);
        float z = Mathf.Abs(position.z % sectionSize);
        return x < cellSize * 0.5f || z < cellSize * 0.5f;
    }

    public Vector3 SnapToSubsection(Vector3 worldPosition)
    {
        float x = Mathf.Round(worldPosition.x / sectionSize) * sectionSize;
        float z = Mathf.Round(worldPosition.z / sectionSize) * sectionSize;
        return new Vector3(x, worldPosition.y, z);
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / cellSize);
        int z = Mathf.RoundToInt(worldPosition.z / cellSize);
        return new Vector2Int(x, z);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, 0f, gridPos.y * cellSize);
    }
}