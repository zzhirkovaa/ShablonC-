using UnityEngine;

public class CameraController : MonoBehaviour, ICameraYawProvider, ICameraPlanarBasisProvider
{
    [Header("Target Settings")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0.7f, 1.8f, -3.0f);

    [Header("Mouse Settings")]
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _minVerticalAngle = -30f;
    [SerializeField] private float _maxVerticalAngle = 60f;

    private float _yaw;
    private float _pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 angles = transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        _yaw += Input.GetAxis("Mouse X") * _mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, _minVerticalAngle, _maxVerticalAngle);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 position = _target.position + (rotation * _offset);

        transform.rotation = rotation;
        transform.position = position;
    }

    public float GetYaw() => _yaw;

    public Vector3 ForwardOnPlane
    {
        get
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            return forward.normalized;
        }
    }

    public Vector3 RightOnPlane
    {
        get
        {
            Vector3 right = transform.right;
            right.y = 0f;
            return right.normalized;
        }
    }
}
