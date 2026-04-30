using UnityEngine;

public class CameraOrbitController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;              // object to orbit around
    public Vector3 targetOffset = Vector3.zero; // fine-tune the pivot point

    [Header("Orbit Settings")]
    public float orbitSpeed = 0.3f;  // drag sensitivity
    public float minVerticalAngle = 1.0f; // look up limit
    public float maxVerticalAngle = 80f; // look down limit
    public bool invertX = false;
    public bool invertY = false;

    [Header("Zoom Settings")]
    public float minDistance = 1f;
    public float maxDistance = 20f;
    public float pinchSensitivity = 0.02f;
    public float mouseZoomSpeed = 5f;

    [Header("Auto Rotate")]
    public bool autoRotate = false;
    public float autoRotateSpeed = 20f;   // degrees per second

    [Header("Smoothing")]
    public bool useSmoothing = true;
    public float smoothSpeed = 8f;

    public bool useVerticalLimits = true;

    // ── private state ──────────────────────────────────────────────────────
    private float _currentX;           // horizontal angle
    private float _currentY;           // vertical angle
    private float _currentDistance;    // distance from target

    private float _targetX;
    private float _targetY;
    private float _targetDistance;

    private float _lastPinchDistance;
    private bool _isDragging;
    private int _dragFingerId = -1;
    private Vector2 _lastDragPos;

    private Camera _cam;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;

        // Initialise angles from current camera position relative to target
        if (target != null)
        {
            Vector3 offset = transform.position - GetPivot();
            _currentDistance = offset.magnitude;
            _currentX = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
            _currentY = Mathf.Asin(offset.y / _currentDistance) * Mathf.Rad2Deg;
        }
        else
        {
            _currentDistance = 5f;
            _currentX = 0f;
            _currentY = 20f;
        }

        _targetX = _currentX;
        _targetY = _currentY;
        _targetDistance = _currentDistance;

        _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
        _currentDistance = _targetDistance;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleInput();
        AutoRotate();
        ApplyTransform();
    }

    // ──────────────────────────────────────────────────────────────────────
    //  INPUT
    // ──────────────────────────────────────────────────────────────────────
    void HandleInput()
    {
        int touchCount = Input.touchCount;

        // ── Two fingers → pinch zoom ───────────────────────────────────────
        if (touchCount == 2)
        {
            _isDragging = false;
            _dragFingerId = -1;

            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float dist = Vector2.Distance(t0.position, t1.position);

            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                _lastPinchDistance = dist;
                return;
            }

            float delta = (_lastPinchDistance - dist) * pinchSensitivity;
            _lastPinchDistance = dist;
            _targetDistance = Mathf.Clamp(_targetDistance + delta, minDistance, maxDistance);
            return;
        }

        // ── One finger → orbit ─────────────────────────────────────────────
        if (touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                _dragFingerId = t.fingerId;
                _lastDragPos = t.position;
                _isDragging = true;
            }

            if (_isDragging && t.fingerId == _dragFingerId && t.phase == TouchPhase.Moved)
            {
                Vector2 delta = t.position - _lastDragPos;
                _lastDragPos = t.position;
                ApplyOrbitDelta(delta);
            }

            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                _isDragging = false;
                _dragFingerId = -1;
            }

            return;
        }

        // ── Mouse (editor / desktop) ───────────────────────────────────────

        // Left mouse drag → orbit
        if (Input.GetMouseButtonDown(0))
        {
            _lastDragPos = Input.mousePosition;
            _isDragging = true;
        }

        if (_isDragging && Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - _lastDragPos;
            _lastDragPos = Input.mousePosition;
            ApplyOrbitDelta(delta);
        }

        if (Input.GetMouseButtonUp(0))
            _isDragging = false;

        // Scroll wheel → zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            _targetDistance = Mathf.Clamp(
                _targetDistance - scroll * mouseZoomSpeed,
                minDistance, maxDistance);
        }
    }

    void ApplyOrbitDelta(Vector2 delta)
    {
        float dirX = invertX ? -1f : 1f;
        float dirY = invertY ? 1f : -1f;

        _targetX += delta.x * orbitSpeed * dirX;
        

        if (useVerticalLimits)
        {
            _targetY += delta.y * orbitSpeed * dirY;
            _targetY = Mathf.Clamp(_targetY, minVerticalAngle, maxVerticalAngle);
        }
        //_targetY = Mathf.Clamp(_targetY, minVerticalAngle, maxVerticalAngle);

        // Stop auto rotate when player manually drags
        autoRotate = false;
    }

    // ──────────────────────────────────────────────────────────────────────
    //  AUTO ROTATE
    // ──────────────────────────────────────────────────────────────────────
    void AutoRotate()
    {
        if (!autoRotate) return;
        _targetX += autoRotateSpeed * Time.deltaTime;
    }

    // ──────────────────────────────────────────────────────────────────────
    //  APPLY POSITION + ROTATION
    // ──────────────────────────────────────────────────────────────────────
    void ApplyTransform()
    {
        if (useSmoothing)
        {
            _currentX = Mathf.LerpAngle(_currentX, _targetX, Time.deltaTime * smoothSpeed);
            _currentY = Mathf.Lerp(_currentY, _targetY, Time.deltaTime * smoothSpeed);
            _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, Time.deltaTime * smoothSpeed);
        }
        else
        {
            _currentX = _targetX;
            _currentY = _targetY;
            _currentDistance = _targetDistance;
        }

        // Build rotation from angles
        Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0f);

        // Position camera behind the pivot at the current distance
        Vector3 pivot = GetPivot();
        Vector3 camPos = pivot + rotation * new Vector3(0f, 0f, -_currentDistance);

        transform.position = camPos;
        transform.rotation = rotation;
    }

    Vector3 GetPivot()
    {
        return target.position + targetOffset;
    }

    // ──────────────────────────────────────────────────────────────────────
    //  PUBLIC HELPERS
    // ──────────────────────────────────────────────────────────────────────

    // Call this to set a new orbit target at runtime
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        // Recalculate angles from current camera position
        Vector3 offset = transform.position - GetPivot();
        _targetDistance = Mathf.Clamp(offset.magnitude, minDistance, maxDistance);
        _targetX = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        _targetY = Mathf.Asin(
                               Mathf.Clamp(offset.y / _targetDistance, -1f, 1f)
                           ) * Mathf.Rad2Deg;
    }

    // Call this to snap to a specific angle instantly (no smoothing)
    public void SnapToAngle(float horizontal, float vertical)
    {
        _targetX = _currentX = horizontal;
        _targetY = _currentY = Mathf.Clamp(vertical, minVerticalAngle, maxVerticalAngle);
    }

    // Call this to resume auto rotate
    public void StartAutoRotate()
    {
        autoRotate = true;
    }

    // Reset camera to default position behind the target
    public void ResetView()
    {
        SnapToAngle(0f, 20f);
        _targetDistance = _currentDistance = (minDistance + maxDistance) / 2f;
    }
}