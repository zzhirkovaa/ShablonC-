using UnityEngine;
using Player.Interfaces;
using Player.Core;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _distanceFromTarget = 5f;
    [SerializeField] private float _minVerticalAngle = -30f;
    [SerializeField] private float _maxVerticalAngle = 60f;
    [SerializeField] private float _smoothTime = 0.2f;
    [SerializeField] private Transform _target;

    [Header("Mouse Settings")]
    [SerializeField] private bool _invertY = false;
    [SerializeField] private float _scrollSensitivity = 2f;
    [SerializeField] private float _minDistance = 2f;
    [SerializeField] private float _maxDistance = 10f;

    [Header("Collision Settings - мнбне")]
    [SerializeField] private LayerMask _collisionLayers = -1; 
    [SerializeField] private float _collisionRadius = 0.3f;

    private ICameraController _cameraLogic;
    private Vector2 _mouseDelta;
    private bool _cursorLocked = true;

    private void Awake()
    {
        _cameraLogic = new CameraLogic(
            transform,
            _mouseSensitivity,
            _distanceFromTarget,
            _minVerticalAngle,
            _maxVerticalAngle,
            _smoothTime,
            _collisionLayers  
        );

        if (_target != null)
        {
            _cameraLogic.SetTarget(_target);
        }

        LockCursor();
    }

    private void Update()
    {
        HandleInput();
        HandleCursorLock();
    }

    private void LateUpdate()
    {
        _cameraLogic.HandleMouseInput(_mouseDelta);
        _cameraLogic.UpdatePosition();
    }

    private void HandleInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (_invertY)
            mouseY = -mouseY;

        _mouseDelta = new Vector2(mouseX, mouseY);

    }

    private void HandleCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _cursorLocked = false;
            UnlockCursor();
        }
        else if (Input.GetMouseButtonDown(0) && !_cursorLocked)
        {
            _cursorLocked = true;
            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
        _cameraLogic.SetTarget(newTarget);
    }
}