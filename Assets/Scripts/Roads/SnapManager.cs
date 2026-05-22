using UnityEngine;
using System.Collections.Generic;

public class SnapManager : MonoBehaviour
{
    public static SnapManager Instance { get; private set; }

    [Header("Settings")]
    public float snapRadius = 3f;

    [Header("Visual")]
    public GameObject snapIndicatorPrefab;

    private GameObject indicatorInstance;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        indicatorInstance = Instantiate(snapIndicatorPrefab);
        indicatorInstance.SetActive(false);
    }

    public Vector3 GetSnappedPosition(Vector3 rawPosition, out bool snapped)
    {
        snapped = false;

        if (RoadNetwork.Instance == null) return rawPosition;

        List<Vector3> nodes = RoadNetwork.Instance.GetAllNodePositions();

        float closestDist = snapRadius;
        Vector3 closestNode = rawPosition;

        foreach (var node in nodes)
        {
            float dist = Vector2.Distance(
                new Vector2(rawPosition.x, rawPosition.z),
                new Vector2(node.x, node.z)
            );

            if (dist < closestDist)
            {
                closestDist = dist;
                closestNode = node;
                snapped = true;
            }
        }

        if (snapped)
        {
            indicatorInstance.transform.position = closestNode + Vector3.up * 0.1f;
            indicatorInstance.SetActive(true);
        }
        else
        {
            indicatorInstance.SetActive(false);
        }

        return snapped ? closestNode : rawPosition;
    }

    public void HideIndicator()
    {
        indicatorInstance.SetActive(false);
    }

    void OnDrawGizmos()
    {
        if (RoadNetwork.Instance == null) return;
        Gizmos.color = Color.yellow;
        foreach (var node in RoadNetwork.Instance.GetAllNodePositions())
            Gizmos.DrawWireSphere(node, 0.3f);
    }
}