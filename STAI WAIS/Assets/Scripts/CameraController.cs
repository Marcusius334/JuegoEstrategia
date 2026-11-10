using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movimiento (Panning)")]
    [Tooltip("Sensibilidad de arrastre del ratón")]
    [SerializeField] private float dragSpeed = 1.0f;

    [Header("Zoom")]
    [Tooltip("Velocidad de respuesta del zoom")]
    [SerializeField] private float zoomSpeed = 2.0f;
    [SerializeField] private float minZoom = 3.0f;
    [SerializeField] private float maxZoom = 15.0f;

    private Camera cam;
    private Vector3 dragOrigin;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        
        // Nos aseguramos de que la cámara sea Ortográfica para 2D
        if (!cam.orthographic)
        {
            cam.orthographic = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandlePan();
        HandleZoom();
    }

    private void HandlePan()
    {
        // 0 = Click Izquierdo, 1 = Click Derecho, 2 = Click Botón Medio (Rueda)
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position += difference * dragSpeed;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            float targetZoom = cam.orthographicSize - (scroll * zoomSpeed);
            cam.orthographicSize = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }
}
