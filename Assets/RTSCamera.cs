using UnityEngine;

public class RTSCamera : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed = 20f;
    public float borderSpeed = 15f;
    public float borderMargin = 20f; // pixels

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minHeight = 5f;
    public float maxHeight = 80f;

    [Header("Rotation")]
    public float rotationSpeed = 100f;

    [Header("Camera Tilt")]
    public float cameraTiltAngle = 45f;

    private Camera cam;
    private Vector3 targetPosition;
    private float currentHeight = 30f;
    void Start()
    {
        cam = GetComponentInChildren<Camera>();

        cam.transform.localPosition = new Vector3(0, currentHeight, -currentHeight * 0.8f);
        cam.transform.localRotation = Quaternion.Euler(cameraTiltAngle, 0, 0);

        targetPosition = transform.position;
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleRotation();

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
    }

    void HandleMovement()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            direction += transform.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            direction += -transform.forward;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            direction += -transform.right;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            direction += transform.right;

        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x < borderMargin) direction += -transform.right;
        if (mousePos.x > Screen.width - borderMargin) direction += transform.right;
        if (mousePos.y < borderMargin) direction += -transform.forward;
        if (mousePos.y > Screen.height - borderMargin) direction += transform.forward;

        direction.y = 0;

        targetPosition += direction.normalized * movementSpeed * Time.deltaTime;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        currentHeight -= scroll * zoomSpeed * currentHeight;
        currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);

        cam.transform.localPosition = new Vector3(0, currentHeight, -currentHeight * 0.8f);
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(2))
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, rotX, Space.World);
        }

        if (Input.GetKey(KeyCode.Q))
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
        if (Input.GetKey(KeyCode.E))
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}