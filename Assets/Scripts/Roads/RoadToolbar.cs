using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoadToolbar : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnStraightRoad;
    public Button btnCurvedRoad;

    [Header("Colors")]
    public Color activeColor = new Color(0.2f, 0.6f, 1f);
    public Color inactiveColor = new Color(0.15f, 0.15f, 0.15f);

    private RoadPlacer roadPlacer;

    void Start()
    {
        roadPlacer = FindFirstObjectByType<RoadPlacer>();

        btnStraightRoad.onClick.AddListener(() => SelectMode(RoadPlacer.RoadMode.Straight));
        btnCurvedRoad.onClick.AddListener(() => SelectMode(RoadPlacer.RoadMode.Curved));

        UpdateVisual(RoadPlacer.RoadMode.None);
    }

    public void SelectMode(RoadPlacer.RoadMode mode)
    {
        if (roadPlacer.currentMode == mode)
        {
            roadPlacer.SetMode(RoadPlacer.RoadMode.None);
            UpdateVisual(RoadPlacer.RoadMode.None);
        }
        else
        {
            roadPlacer.SetMode(mode);
            UpdateVisual(mode);
        }
    }

    void UpdateVisual(RoadPlacer.RoadMode activeMode)
    {
        SetButtonColor(btnStraightRoad, activeMode == RoadPlacer.RoadMode.Straight);
        SetButtonColor(btnCurvedRoad, activeMode == RoadPlacer.RoadMode.Curved);
    }

    void SetButtonColor(Button btn, bool active)
    {
        btn.GetComponent<Image>().color = active ? activeColor : inactiveColor;
    }
}