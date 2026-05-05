using UnityEngine;
using UnityEngine.EventSystems;

public class CameraZoomController : MonoBehaviour
{
    [Header("Camera")]
    public Camera targetCamera;

    [Header("Zoom Settings")]
    public float minOrthoSize = 1f;
    public float maxOrthoSize = 20f;
    public float minFOV = 15f;
    public float maxFOV = 100f;
    public float pinchSensitivity = 0.02f;

    [Header("Mouse Zoom (Editor)")]
    public float mouseZoomSpeed = 5f;

    [Header("Pan Settings")]
    public float panSpeed = 1f;
    public bool invertPan = false;
    public bool clampPan = false;
    public Vector2 panMin = new Vector2(-50f, -50f);
    public Vector2 panMax = new Vector2(50f, 50f);

    private float _lastPinchDistance;
    private Vector3 _panOrigin;
    private bool _isPanning;
    private int _panFingerId = -1;

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Update()
    {
        HandleTouch();
        HandleMouseInput();
    }

    // ── UI Block Check ──────────────────────────────────────────
    bool IsPointerOverUI(int fingerId = -1)
    {
        if (EventSystem.current == null) return false;

        if (fingerId >= 0)
            return EventSystem.current.IsPointerOverGameObject(fingerId);

        return EventSystem.current.IsPointerOverGameObject();
    }

    // ───────────────────────────────────────────────────────────
    //  TOUCH
    // ───────────────────────────────────────────────────────────
    void HandleTouch()
    {
        int touchCount = Input.touchCount;

        // ── Two fingers → pinch zoom ──
        if (touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            // Block zoom if either finger is over UI
            if (IsPointerOverUI(t0.fingerId) || IsPointerOverUI(t1.fingerId))
            {
                _isPanning = false;
                _panFingerId = -1;
                return;
            }

            _isPanning = false;
            _panFingerId = -1;

            float currentDist = Vector2.Distance(t0.position, t1.position);

            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                _lastPinchDistance = currentDist;
                return;
            }

            float delta = (_lastPinchDistance - currentDist) * pinchSensitivity;
            _lastPinchDistance = currentDist;
            ApplyZoom(delta);
            return;
        }

        // ── One finger → pan ──
        if (touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                // Block pan start if over UI
                if (IsPointerOverUI(t.fingerId))
                {
                    _isPanning = false;
                    _panFingerId = -1;
                    return;
                }

                _panFingerId = t.fingerId;
                _panOrigin = ScreenToWorld(t.position);
                _isPanning = true;
            }

            if (_isPanning && t.fingerId == _panFingerId)
            {
                if (t.phase == TouchPhase.Moved)
                {
                    Vector3 currentWorld = ScreenToWorld(t.position);
                    Vector3 drag = _panOrigin - currentWorld;
                    if (invertPan) drag = -drag;
                    MoveCamera(drag);
                }

                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    _isPanning = false;
                    _panFingerId = -1;
                }
            }

            return;
        }

        _isPanning = false;
        _panFingerId = -1;
    }

    // ───────────────────────────────────────────────────────────
    //  MOUSE (editor / desktop)
    // ───────────────────────────────────────────────────────────
    void HandleMouseInput()
    {
        // Scroll wheel → zoom (block if over UI)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f && !IsPointerOverUI())
            ApplyZoom(-scroll * mouseZoomSpeed);

        // Pan start (block if over UI)
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            if (IsPointerOverUI()) return;

            _panOrigin = ScreenToWorld(Input.mousePosition);
            _isPanning = true;
        }

        if (_isPanning && (Input.GetMouseButton(1) || Input.GetMouseButton(2)))
        {
            Vector3 currentWorld = ScreenToWorld(Input.mousePosition);
            Vector3 drag = _panOrigin - currentWorld;
            if (invertPan) drag = -drag;
            MoveCamera(drag);
        }

        if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
            _isPanning = false;
    }

    // ───────────────────────────────────────────────────────────
    //  ZOOM
    // ───────────────────────────────────────────────────────────
    void ApplyZoom(float delta)
    {
        if (targetCamera.orthographic)
        {
            targetCamera.orthographicSize =
                Mathf.Clamp(targetCamera.orthographicSize + delta,
                            minOrthoSize, maxOrthoSize);
        }
        else
        {
            targetCamera.fieldOfView =
                Mathf.Clamp(targetCamera.fieldOfView + delta,
                            minFOV, maxFOV);
        }
    }

    // ───────────────────────────────────────────────────────────
    //  PAN HELPERS
    // ───────────────────────────────────────────────────────────
    void MoveCamera(Vector3 worldDelta)
    {
        Vector3 newPos = targetCamera.transform.position + worldDelta * panSpeed;

        if (clampPan)
        {
            newPos.x = Mathf.Clamp(newPos.x, panMin.x, panMax.x);
            newPos.y = Mathf.Clamp(newPos.y, panMin.y, panMax.y);
        }

        targetCamera.transform.position = newPos;
    }

    Vector3 ScreenToWorld(Vector3 screenPos)
    {
        if (targetCamera.orthographic)
        {
            screenPos.z = Mathf.Abs(targetCamera.transform.position.z);
            return targetCamera.ScreenToWorldPoint(screenPos);
        }
        else
        {
            float planeZ = 0f;
            Ray ray = targetCamera.ScreenPointToRay(screenPos);
            float t = (planeZ - ray.origin.z) / ray.direction.z;
            return ray.origin + ray.direction * t;
        }
    }
}





//using UnityEngine;

//public class CameraZoomController : MonoBehaviour
//{
//    [Header("Camera")]
//    public Camera targetCamera;

//    [Header("Zoom Settings")]
//    public float minOrthoSize = 1f;
//    public float maxOrthoSize = 20f;
//    public float minFOV = 15f;
//    public float maxFOV = 100f;
//    public float pinchSensitivity = 0.02f;

//    [Header("Mouse Zoom (Editor)")]
//    public float mouseZoomSpeed = 5f;

//    [Header("Pan Settings")]
//    public float panSpeed = 1f;    // tune this per scene scale
//    public bool invertPan = false;
//    public bool clampPan = false; // optional: keep camera inside world bounds
//    public Vector2 panMin = new Vector2(-50f, -50f);
//    public Vector2 panMax = new Vector2(50f, 50f);

//    // ── private state ──────────────────────────────────────────
//    private float _lastPinchDistance;
//    private Vector3 _panOrigin;          // world point where finger/mouse went down
//    private bool _isPanning;
//    private int _panFingerId = -1;   // which finger is doing the pan

//    void Awake()
//    {
//        if (targetCamera == null)
//            targetCamera = Camera.main;
//    }

//    void Update()
//    {
//        HandleTouch();
//        HandleMouseInput();
//    }

//    // ───────────────────────────────────────────────────────────
//    //  TOUCH
//    // ───────────────────────────────────────────────────────────
//    void HandleTouch()
//    {
//        int touchCount = Input.touchCount;

//        // ── Two fingers → pinch zoom (cancel any active pan) ──
//        if (touchCount == 2)
//        {
//            _isPanning = false;
//            _panFingerId = -1;

//            Touch t0 = Input.GetTouch(0);
//            Touch t1 = Input.GetTouch(1);

//            float currentDist = Vector2.Distance(t0.position, t1.position);

//            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
//            {
//                _lastPinchDistance = currentDist;
//                return;
//            }

//            float delta = (_lastPinchDistance - currentDist) * pinchSensitivity;
//            _lastPinchDistance = currentDist;
//            ApplyZoom(delta);
//            return;
//        }

//        // ── One finger → pan ──
//        if (touchCount == 1)
//        {
//            Touch t = Input.GetTouch(0);

//            if (t.phase == TouchPhase.Began)
//            {
//                _panFingerId = t.fingerId;
//                _panOrigin = ScreenToWorld(t.position);
//                _isPanning = true;
//            }

//            if (_isPanning && t.fingerId == _panFingerId)
//            {
//                if (t.phase == TouchPhase.Moved)
//                {
//                    Vector3 currentWorld = ScreenToWorld(t.position);
//                    Vector3 drag = _panOrigin - currentWorld; // world delta
//                    if (invertPan) drag = -drag;
//                    MoveCamera(drag);
//                }

//                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
//                {
//                    _isPanning = false;
//                    _panFingerId = -1;
//                }
//            }

//            return;
//        }

//        // ── No fingers ──
//        _isPanning = false;
//        _panFingerId = -1;
//    }

//    // ───────────────────────────────────────────────────────────
//    //  MOUSE (editor / desktop)
//    // ───────────────────────────────────────────────────────────
//    void HandleMouseInput()
//    {
//        // Scroll wheel → zoom
//        float scroll = Input.GetAxis("Mouse ScrollWheel");
//        if (Mathf.Abs(scroll) > 0.001f)
//            ApplyZoom(-scroll * mouseZoomSpeed);

//        // Middle mouse button drag OR right mouse button → pan
//        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
//        {
//            _panOrigin = ScreenToWorld(Input.mousePosition);
//            _isPanning = true;
//        }

//        if (_isPanning && (Input.GetMouseButton(1) || Input.GetMouseButton(2)))
//        {
//            Vector3 currentWorld = ScreenToWorld(Input.mousePosition);
//            Vector3 drag = _panOrigin - currentWorld;
//            if (invertPan) drag = -drag;
//            MoveCamera(drag);
//            // Don't update _panOrigin — drag is always relative to where you clicked
//        }

//        if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
//        {
//            _isPanning = false;
//        }
//    }

//    // ───────────────────────────────────────────────────────────
//    //  ZOOM
//    // ───────────────────────────────────────────────────────────
//    void ApplyZoom(float delta)
//    {
//        if (targetCamera.orthographic)
//        {
//            targetCamera.orthographicSize =
//                Mathf.Clamp(targetCamera.orthographicSize + delta,
//                            minOrthoSize, maxOrthoSize);
//        }
//        else
//        {
//            targetCamera.fieldOfView =
//                Mathf.Clamp(targetCamera.fieldOfView + delta,
//                            minFOV, maxFOV);
//        }
//    }

//    // ───────────────────────────────────────────────────────────
//    //  PAN HELPERS
//    // ───────────────────────────────────────────────────────────
//    void MoveCamera(Vector3 worldDelta)
//    {
//        Vector3 newPos = targetCamera.transform.position + worldDelta * panSpeed;

//        if (clampPan)
//        {
//            newPos.x = Mathf.Clamp(newPos.x, panMin.x, panMax.x);
//            newPos.y = Mathf.Clamp(newPos.y, panMin.y, panMax.y);
//        }

//        targetCamera.transform.position = newPos;
//    }

//    // Converts a screen point to a world point on the camera's near plane.
//    // Works for both orthographic and perspective cameras.
//    Vector3 ScreenToWorld(Vector3 screenPos)
//    {
//        if (targetCamera.orthographic)
//        {
//            // Z doesn't matter for ortho pan — keep camera's current Z
//            screenPos.z = Mathf.Abs(targetCamera.transform.position.z);
//            return targetCamera.ScreenToWorldPoint(screenPos);
//        }
//        else
//        {
//            // For perspective: raycast onto the XY plane at z=0 (or whatever
//            // depth your scene lives at — adjust planeZ to match)
//            float planeZ = 0f;
//            Ray ray = targetCamera.ScreenPointToRay(screenPos);
//            float t = (planeZ - ray.origin.z) / ray.direction.z;
//            return ray.origin + ray.direction * t;
//        }
//    }
//}