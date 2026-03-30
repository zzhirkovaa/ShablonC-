using UnityEngine;
using Player.Interfaces;

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

    private IPlayerInputService _inputService;
    private ICameraYawProvider _cameraYawProvider;
    private IPlayerMovement _movement;
    private IPlayerAppearance _appearance;

    private CharacterController _controller;
    private Animator _animator;
    private Renderer[] _renderers;

    private bool _isConstructed;

    public CharacterController CharacterController => _controller;
    public Animator Animator => _animator;
    public Renderer[] Renderers => _renderers;

    public float WalkSpeed => _walkSpeed;
    public float RunSpeed => _runSpeed;
    public float RotationSpeed => _rotationSpeed;
    public float AnimationSmoothTime => _animationSmoothTime;
    public float StartAnimThreshold => _startAnimThreshold;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _renderers = GetComponentsInChildren<Renderer>();
    }

    public void Construct(
        IPlayerInputService inputService,
        ICameraYawProvider cameraYawProvider,
        IPlayerMovement movement,
        IPlayerAppearance appearance)
    {
        _inputService = inputService;
        _cameraYawProvider = cameraYawProvider;
        _movement = movement;
        _appearance = appearance;
        _isConstructed = true;
    }

    private void Update()
    {
        if (!_isConstructed)
            return;

        Vector2 inputVector = _inputService.GetMoveInput();
        bool isRunning = _inputService.IsRunPressed();

        float inputMagnitude = inputVector.magnitude;
        bool isMoving = inputMagnitude > _startAnimThreshold;

        if (isMoving && _cameraYawProvider != null)
        {
            float targetYaw = _cameraYawProvider.GetYaw();
            Quaternion targetRotation = Quaternion.Euler(0f, targetYaw, 0f);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);
        }

        _movement.HandleMovement(inputVector, isMoving, isRunning);
        _movement.ApplyGravity();
        _appearance.UpdateAnimations(inputMagnitude);

        if (_inputService.IsPunchPressedThisFrame())
        {
            _animator.SetTrigger("Punch");
        }
    }
}