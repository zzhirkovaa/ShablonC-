using UnityEngine;
using Player.Interfaces;
using Player.Core;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _runSpeed = 10f; 
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Animation Settings")]
    [SerializeField] private float _animationSmoothTime = 0.2f;
    [SerializeField] private float _startAnimThreshold = 0.1f;

    private IPlayerMovement _movement;
    private IPlayerAppearance _appearance;
    private CharacterController _controller;
    private Animator _animator;
    private Renderer[] _renderers;

    private Vector2 _inputVector;
    private bool _isRunning;  

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _renderers = GetComponentsInChildren<Renderer>();

        _movement = new PlayerMovement(transform, _controller, _walkSpeed, _runSpeed, _rotationSpeed);
        _appearance = new PlayerAppearance(_animator, _renderers, _animationSmoothTime);
    }

    private void Update()
    {
        HandleInput();
        float inputMagnitude = _inputVector.magnitude;
        bool isMoving = inputMagnitude > _startAnimThreshold;

        CameraController cam = Camera.main.GetComponent<CameraController>();
        if (cam != null)
        {
            if (isMoving)
            {
                float targetYaw = cam.GetYaw();
                Quaternion targetRotation = Quaternion.Euler(0, targetYaw, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }

        _movement.HandleMovement(_inputVector, isMoving, _isRunning);

        _movement.ApplyGravity();

        _animator.SetFloat("Blend", inputMagnitude, _animationSmoothTime, Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
        {
            _animator.SetTrigger("Punch");
        }
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        _inputVector = new Vector2(horizontal, vertical);

        _isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
}