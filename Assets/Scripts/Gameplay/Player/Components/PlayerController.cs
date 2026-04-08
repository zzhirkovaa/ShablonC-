using UnityEngine;
using Player.Interfaces;
using Player.Core;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
// Unity-facing adapter for the player. In project architecture this MonoBehaviour plays the View role.
public class PlayerController : MonoBehaviour, IPlayerView
{
    [Header("Movement Settings")]
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _runSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Animation Settings")]
    [SerializeField] private float _animationSmoothTime = 0.2f;
    [SerializeField] private float _startAnimThreshold = 0.1f;

    private CharacterController _controller;
    private Animator _animator;
    private Renderer[] _renderers;

    private PlayerGameplayController _gameplayController;
    private bool _isConstructed;

    public CharacterController CharacterController => _controller;
    public Animator Animator => _animator;
    public Renderer[] Renderers => _renderers;
    public Quaternion Rotation => transform.rotation;

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
        _gameplayController = new PlayerGameplayController(
            new PlayerModel(),
            this,
            inputService,
            cameraYawProvider,
            movement,
            appearance,
            _rotationSpeed,
            _startAnimThreshold);

        _isConstructed = true;
    }

    private void Update()
    {
        if (!_isConstructed)
            return;

        _gameplayController.Tick();
    }

    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    public void TriggerPunch()
    {
        _animator.SetTrigger("Punch");
    }
}
