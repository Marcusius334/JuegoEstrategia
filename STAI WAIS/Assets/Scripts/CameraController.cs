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

    [Header("Límites del Mapa (Bounds)")]
    [Tooltip("Activa o desactiva la restricción de movimiento")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private float minX = -20.0f;
    [SerializeField] private float maxX = 20.0f;
    [SerializeField] private float minY = -15.0f;
    [SerializeField] private float maxY = 15.0f;

    [Header("Centrado de Cámara")]
    [Tooltip("Velocidad de interpolación para el centrado suave")]
    [SerializeField] private float focusSpeed = 10.0f;
    [SerializeField] private KeyCode centerKey = KeyCode.Space;

    private Camera cam;
    private Vector3 dragOrigin;
    private Vector3 defaultPosition = new Vector3(0f, 0f, -10f);
    private bool isFocusing = false;

    //Variable para compartir la posición de la celda y poder centrar la camara:
    public static Vector3 targetFocusPosition;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        
        // Nos aseguramos de que la cámara sea Ortográfica para 2D
        if (!cam.orthographic)
        {
            cam.orthographic = true;
        }

        targetFocusPosition = defaultPosition;
    }

    // Update is called once per frame
    void Update()
    {
        HandlePan();
        HandleZoom();
        HandleCenter();
        ApplyBounds();
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

    private void HandleCenter()
    {
        // Al pulsar la tecla configurada (por defecto Espacio), inicia el centrado
        if (Input.GetKeyDown(centerKey))
        {
            FocusOnPosition(targetFocusPosition);
        }

        // Movimiento suave hacia el objetivo
        if (isFocusing)
        {
            transform.position = Vector3.Lerp(transform.position, targetFocusPosition, Time.deltaTime * focusSpeed);
            if (Vector3.Distance(transform.position, targetFocusPosition) < 0.05f)
            {
                transform.position = targetFocusPosition;
                isFocusing = false;
                Debug.Log("Se ha centrado");
            }
        }
    }

    private void ApplyBounds()
    {
        if (!useBounds) return;

        // Mantener el valor Z original de la cámara (habitualmente -10 en 2D)
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    //Centra a un punto, se puede usar en otros scripts para centrar al seleccionar un objeto
    public void FocusOnPosition(Vector3 targetWorldPos)
    {
        targetFocusPosition = new Vector3(targetWorldPos.x, targetWorldPos.y, transform.position.z);
        isFocusing = true;
    }
}
