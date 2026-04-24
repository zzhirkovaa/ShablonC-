using System;
using UnityEngine;

// BossController is the context in the State pattern: it stores shared data and delegates behavior to states.
public sealed class BossController : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float _detectionRadius = 16f;
    [SerializeField] private float _attackRange = 3.2f;
    [SerializeField] private float _strongAttackRange = 4.4f;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 3.5f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float _patrolRadius = 5f;
    [SerializeField] private float _patrolPointTolerance = 0.6f;

    [Header("State Timings")]
    [SerializeField] private float _idleDuration = 1.5f;
    [SerializeField] private float _aggressionDecisionDelay = 0.2f;
    [SerializeField] private float _attackStateDuration = 1.15f;
    [SerializeField] private float _strongAttackStateDuration = 1.4f;
    [SerializeField] private float _rageStateDuration = 1f;

    [Header("Phase 2")]
    [SerializeField] private float _phaseTwoHealthThreshold = 0.5f;
    [SerializeField] private float _phaseTwoAttackSpeedMultiplier = 1.4f;

    private Animator _animator;
    private Rigidbody _rigidbody;
    private EnemyHealth _health;
    private BossCombat _combat;
    private Transform _playerTransform;
    private IEnemyMovementBounds _movementBounds;
    private Vector3 _spawnPosition;
    private bool _rageTransitionPending;
    private bool _hasEnteredPhaseTwo;
    private bool _deathHandled;

    public BossStateMachine StateMachine { get; private set; }
    public float MaxHealth => _health != null ? _health.MaxHealth : 0f;
    public float CurrentHealth => _health != null ? _health.CurrentHealth : 0f;
    public bool IsDead => _health != null && _health.IsDead;
    public bool ShouldEnterRage => _rageTransitionPending && !_deathHandled;
    public float AttackSpeedMultiplier => _hasEnteredPhaseTwo ? _phaseTwoAttackSpeedMultiplier : 1f;

    public float DetectionRadius => _detectionRadius;
    public float AttackRange => _attackRange;
    public float StrongAttackRange => _strongAttackRange;
    public float MoveSpeed => _moveSpeed;
    public float RotationSpeed => _rotationSpeed;
    public float PatrolRadius => _patrolRadius;
    public float PatrolPointTolerance => _patrolPointTolerance;
    public float IdleDuration => _idleDuration;
    public float AggressionDecisionDelay => _aggressionDecisionDelay;
    public float AttackStateDuration => _attackStateDuration / AttackSpeedMultiplier;
    public float StrongAttackStateDuration => _strongAttackStateDuration / AttackSpeedMultiplier;
    public float RageStateDuration => _rageStateDuration;

    public BossIdleState IdleState { get; private set; }
    public BossPatrolState PatrolState { get; private set; }
    public BossAggressionState AggressionState { get; private set; }
    public BossChaseState ChaseState { get; private set; }
    public BossAttackState AttackState { get; private set; }
    public BossStrongAttackState StrongAttackState { get; private set; }
    public BossRageState RageState { get; private set; }
    public BossDeathState DeathState { get; private set; }

    public bool HasPlayer => _playerTransform != null;
    public bool HasDetectedPlayer => HasPlayer && DistanceToPlayer <= _detectionRadius;
    public bool IsPlayerInAttackRange => HasPlayer && DistanceToPlayer <= _attackRange;
    public bool IsPlayerInStrongAttackRange => HasPlayer && DistanceToPlayer <= _strongAttackRange;
    public bool CanUseNormalAttack => _combat != null && _combat.CanUseNormalAttack;
    public bool CanUseStrongAttack => _combat != null && _combat.CanUseStrongAttack;

    public float DistanceToPlayer
    {
        get
        {
            if (!HasPlayer)
                return float.PositiveInfinity;

            return Vector3.Distance(transform.position, _playerTransform.position);
        }
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _health = GetComponent<EnemyHealth>();
        _combat = GetComponent<BossCombat>();
        _spawnPosition = transform.position;

        if (_combat == null)
            _combat = gameObject.AddComponent<BossCombat>();

        DisableLegacyEnemyLogic();
        SubscribeToHealth();
        InitializeStateMachine();
    }

    private void OnDestroy()
    {
        UnsubscribeFromHealth();
    }

    public void Construct(Transform playerTransform, IEnemyMovementBounds movementBounds)
    {
        _playerTransform = playerTransform;
        _movementBounds = movementBounds;
        _combat?.Construct(playerTransform);
    }

    private void Update()
    {
        _combat?.Tick(Time.deltaTime);

        if (_deathHandled || StateMachine?.CurrentState == null)
            return;

        if (ShouldEnterRage && !ReferenceEquals(StateMachine.CurrentState, RageState))
        {
            StateMachine.ChangeState(RageState, "Boss HP dropped below 50%");
        }

        StateMachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        if (_deathHandled)
            return;

        StateMachine?.CurrentState?.PhysicsUpdate();
    }

    public void StopMotion()
    {
        if (_rigidbody != null)
            _rigidbody.linearVelocity = Vector3.zero;
    }

    public void Move(Vector3 direction, float deltaTime)
    {
        if (direction == Vector3.zero)
        {
            StopMotion();
            return;
        }

        Vector3 targetPosition = transform.position + direction.normalized * _moveSpeed * deltaTime;
        targetPosition = ClampPosition(targetPosition);

        if (_rigidbody != null && !_rigidbody.isKinematic)
            _rigidbody.MovePosition(targetPosition);
        else
            transform.position = targetPosition;
    }

    public void MoveTowards(Vector3 targetPosition, float deltaTime)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;
        Move(direction.normalized, deltaTime);
    }

    public Vector3 CreatePatrolPoint()
    {
        Vector2 circle = UnityEngine.Random.insideUnitCircle * _patrolRadius;
        Vector3 point = _spawnPosition + new Vector3(circle.x, 0f, circle.y);
        return ClampPosition(point);
    }

    public Vector3 GetDirectionToPlayer()
    {
        if (!HasPlayer)
            return Vector3.zero;

        Vector3 direction = _playerTransform.position - transform.position;
        direction.y = 0f;
        return direction.normalized;
    }

    public void FaceDirection(Vector3 direction, float deltaTime)
    {
        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * _rotationSpeed);
    }

    public void FacePlayerImmediately()
    {
        Vector3 direction = GetDirectionToPlayer();
        if (direction == Vector3.zero)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    public bool HasReachedPoint(Vector3 point)
    {
        return Vector3.Distance(transform.position, point) <= _patrolPointTolerance;
    }

    public void SetMovementAnimation(bool isRunning)
    {
        if (HasAnimatorParameter("IsRunning", AnimatorControllerParameterType.Bool))
            _animator.SetBool("IsRunning", isRunning);
    }

    public void SetAnimatorSpeed(float speed)
    {
        if (_animator != null)
            _animator.speed = speed;
    }

    public void BeginAttackAnimation(bool isStrongAttack)
    {
        ResetCombatTriggers();

        if (isStrongAttack && HasAnimatorParameter("StrongAttack", AnimatorControllerParameterType.Trigger))
        {
            _animator.SetTrigger("StrongAttack");
        }
        else if (HasAnimatorParameter("Attack", AnimatorControllerParameterType.Trigger))
        {
            _animator.SetTrigger("Attack");
        }

        SetAnimatorSpeed(AttackSpeedMultiplier);
    }

    public void BeginRageAnimation()
    {
        ResetCombatTriggers();
        SetAnimatorSpeed(1f);

        if (HasAnimatorParameter("Rage", AnimatorControllerParameterType.Trigger))
            _animator.SetTrigger("Rage");

        if (HasAnimatorParameter("Rage", AnimatorControllerParameterType.Bool))
            _animator.SetBool("Rage", true);
    }

    public void EndRageAnimation()
    {
        if (HasAnimatorParameter("Rage", AnimatorControllerParameterType.Bool))
            _animator.SetBool("Rage", false);
    }

    public void BeginDeathAnimation()
    {
        ResetCombatTriggers();
        SetAnimatorSpeed(1f);
        SetMovementAnimation(false);

        if (HasAnimatorParameter("Die", AnimatorControllerParameterType.Trigger))
            _animator.SetTrigger("Die");
    }

    public bool TryStartNormalAttack()
    {
        return _combat != null && _combat.TryStartNormalAttack();
    }

    public bool TryStartStrongAttack()
    {
        return _combat != null && _combat.TryStartStrongAttack();
    }

    public void CancelPendingAttack()
    {
        _combat?.CancelPendingAttack();
    }

    public void CompleteRageTransition()
    {
        _rageTransitionPending = false;
    }

    public IBossState SelectPostAttackState()
    {
        if (!HasDetectedPlayer)
            return IdleState;

        if (IsPlayerInAttackRange || IsPlayerInStrongAttackRange)
            return AggressionState;

        return ChaseState;
    }

    public IBossState SelectPostRageState()
    {
        if (!HasDetectedPlayer)
            return IdleState;

        if (IsPlayerInAttackRange || IsPlayerInStrongAttackRange)
            return AggressionState;

        return ChaseState;
    }

    public void NotifyDeath()
    {
        if (_deathHandled)
            return;

        _deathHandled = true;
        StateMachine?.ChangeState(DeathState, "Boss health depleted");
    }

    private void InitializeStateMachine()
    {
        StateMachine = new BossStateMachine(gameObject.name);

        IdleState = new BossIdleState(this, StateMachine);
        PatrolState = new BossPatrolState(this, StateMachine);
        AggressionState = new BossAggressionState(this, StateMachine);
        ChaseState = new BossChaseState(this, StateMachine);
        AttackState = new BossAttackState(this, StateMachine);
        StrongAttackState = new BossStrongAttackState(this, StateMachine);
        RageState = new BossRageState(this, StateMachine);
        DeathState = new BossDeathState(this, StateMachine);

        StateMachine.ChangeState(IdleState, "Initial boss state");
    }

    private void DisableLegacyEnemyLogic()
    {
        if (TryGetComponent<EnemyAI>(out var enemyAI))
            enemyAI.enabled = false;

        if (TryGetComponent<EnemyCombat>(out var enemyCombat))
            enemyCombat.enabled = false;
    }

    private void SubscribeToHealth()
    {
        if (_health == null)
            return;

        _health.OnHealthChanged += HandleHealthChanged;
        _health.OnDied += NotifyDeath;
    }

    private void UnsubscribeFromHealth()
    {
        if (_health == null)
            return;

        _health.OnHealthChanged -= HandleHealthChanged;
        _health.OnDied -= NotifyDeath;
    }

    private void HandleHealthChanged(float healthRatio)
    {
        if (_hasEnteredPhaseTwo || _health == null || _health.IsDead)
            return;

        if (healthRatio > _phaseTwoHealthThreshold)
            return;

        _hasEnteredPhaseTwo = true;
        _rageTransitionPending = true;
        _combat?.SetAttackSpeedMultiplier(_phaseTwoAttackSpeedMultiplier);
        Debug.Log($"[{gameObject.name}] Boss entered phase 2. Attack speed multiplier: {_phaseTwoAttackSpeedMultiplier}");
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        return _movementBounds != null ? _movementBounds.ClampPosition(position) : position;
    }

    private void ResetCombatTriggers()
    {
        if (_animator == null)
            return;

        if (HasAnimatorParameter("Attack", AnimatorControllerParameterType.Trigger))
            _animator.ResetTrigger("Attack");

        if (HasAnimatorParameter("StrongAttack", AnimatorControllerParameterType.Trigger))
            _animator.ResetTrigger("StrongAttack");
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (_animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == parameterType)
                return true;
        }

        return false;
    }
}
