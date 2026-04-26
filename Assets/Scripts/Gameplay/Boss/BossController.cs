using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class BossController : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float _detectionRadius = 16f;
    [SerializeField] private float _loseTargetRadius = 24f;
    [SerializeField] private float _attackRange = 4f;
    [SerializeField] private bool _isPeacefulMode;

    [Header("Movement")]
    [SerializeField] private float _chaseSpeed = 3.5f;
    [SerializeField] private float _finisherChaseSpeed = 5.25f;
    [SerializeField] private float _playerLowHealthThreshold = 0.5f;

    [Header("Damage")]
    [SerializeField] private float _attackDamage = 18f;
    [SerializeField] private float _heavyAttackDamage = 36f;
    [SerializeField] private DamageType _damageType = DamageType.Physical;
    [SerializeField] private BossDamageHitbox[] _damageHitboxes;

    [Header("Cooldowns")]
    [SerializeField] private float _attackCooldown = 2.4f;
    [SerializeField] private float _heavyAttackCooldown = 5.5f;

    [Header("Attack Timings")]
    [SerializeField] private float _attackDuration = 1.15f;
    [SerializeField] private float _heavyAttackDuration = 1.4f;
    [SerializeField, Range(0f, 1f)] private float _attackDamageWindowStart = 0.25f;
    [SerializeField, Range(0f, 1f)] private float _attackDamageWindowEnd = 0.65f;
    [SerializeField, Range(0f, 1f)] private float _heavyAttackDamageWindowStart = 0.2f;
    [SerializeField, Range(0f, 1f)] private float _heavyAttackDamageWindowEnd = 0.75f;

    [Header("Phase 2")]
    [SerializeField] private float _phaseTwoHealthThreshold = 0.5f;
    [SerializeField] private float _enrageDuration = 1.1f;
    [SerializeField] private float _enragedAttackSpeedMultiplier = 1.5f;

    [Header("Healing")]
    [SerializeField] private bool _canHealOnce = true;
    [SerializeField] private float _healThreshold = 0.5f;
    [SerializeField] private float _healDuration = 2f;
    [SerializeField] private bool _healToFull = true;
    [SerializeField] private float _healAmount = 50f;

    [Header("References")]
    [SerializeField] private Transform _target;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;
    [SerializeField] private EnemyHealth _health;

    [Header("Animator Parameters")]
    [SerializeField] private string _isMovingParameter = "IsMoving";
    [SerializeField] private string _attackTriggerParameter = "Attack";
    [SerializeField] private string _heavyAttackTriggerParameter = "HeavyAttack";
    [SerializeField] private string _enrageTriggerParameter = "Enrage";
    [SerializeField] private string _isEnragedParameter = "IsEnraged";
    [SerializeField] private string _attackSpeedMultiplierParameter = "AttackSpeedMultiplier";
    [SerializeField] private string _healTriggerParameter = "Heal";

    private readonly HashSet<string> _missingAnimatorParameters = new HashSet<string>();
    private bool _attackAnimationFinished;
    private bool _damageWindowOpenedThisAttack;
    private float _activeAttackDamage;

    public BossStateMachine StateMachine { get; private set; }
    public BossContext Context { get; private set; }

    public BossIdleState IdleState { get; private set; }
    public BossAggroState AggroState { get; private set; }
    public BossChaseState ChaseState { get; private set; }
    public BossAttackState AttackState { get; private set; }
    public BossHeavyAttackState HeavyAttackState { get; private set; }
    public BossEnrageState EnrageState { get; private set; }
    public BossHealState HealState { get; private set; }
    public BossDeathState DeathState { get; private set; }

    public float DetectionRadius => _detectionRadius;
    public float LoseTargetRadius => Mathf.Max(_loseTargetRadius, _detectionRadius);
    public float AttackRange => _attackRange;
    public float AttackDamage => _attackDamage;
    public float HeavyAttackDamage => _heavyAttackDamage;
    public float AttackCooldown => _attackCooldown;
    public float HeavyAttackCooldown => _heavyAttackCooldown;
    public float AttackDuration => _attackDuration / CurrentAttackSpeedMultiplier;
    public float HeavyAttackDuration => _heavyAttackDuration / CurrentAttackSpeedMultiplier;
    public float AttackDamageWindowStart => _attackDamageWindowStart;
    public float AttackDamageWindowEnd => Mathf.Max(_attackDamageWindowStart, _attackDamageWindowEnd);
    public float HeavyAttackDamageWindowStart => _heavyAttackDamageWindowStart;
    public float HeavyAttackDamageWindowEnd => Mathf.Max(_heavyAttackDamageWindowStart, _heavyAttackDamageWindowEnd);
    public float ChaseSpeed => _chaseSpeed;
    public float FinisherChaseSpeed => _finisherChaseSpeed;
    public float PlayerLowHealthThreshold => _playerLowHealthThreshold;
    public float EnrageHealthThreshold => _phaseTwoHealthThreshold;
    public float EnrageDuration => _enrageDuration;
    public float EnragedAttackSpeedMultiplier => _enragedAttackSpeedMultiplier;
    public bool CanHealOnce => _canHealOnce;
    public float HealThreshold => _healThreshold;
    public float HealDuration => _healDuration;
    public bool HealToFull => _healToFull;
    public float HealAmount => _healAmount;
    public bool IsPeacefulMode => Context != null ? Context.IsPeacefulMode : _isPeacefulMode;
    public bool IsEnraged => Context != null && Context.IsEnraged;
    public bool HasDamageWindowOpenedThisAttack => _damageWindowOpenedThisAttack;
    private float CurrentAttackSpeedMultiplier => Context != null ? Context.AttackSpeedMultiplier : 1f;

    private void Awake()
    {
        CacheComponents();
        DisableLegacyEnemyLogic();

        Context = new BossContext(this, transform, _agent, _animator, _health);
        Context.SetPeacefulMode(_isPeacefulMode);
        StateMachine = new BossStateMachine(Context, gameObject.name);

        IdleState = new BossIdleState(Context, StateMachine);
        AggroState = new BossAggroState(Context, StateMachine);
        ChaseState = new BossChaseState(Context, StateMachine);
        AttackState = new BossAttackState(Context, StateMachine);
        HeavyAttackState = new BossHeavyAttackState(Context, StateMachine);
        EnrageState = new BossEnrageState(Context, StateMachine);
        HealState = new BossHealState(Context, StateMachine);
        DeathState = new BossDeathState(Context, StateMachine);

        SubscribeToHealth();
        DisableDamageHitboxes();
        StateMachine.ChangeState(IdleState, "Initial boss state");
    }

    private void Start()
    {
        if (_target == null)
            _target = FindPlayer();

        Context.SetTarget(_target);
    }

    private void OnDestroy()
    {
        UnsubscribeFromHealth();
    }

    private void Update()
    {
        Context.Tick(Time.deltaTime);
        StateMachine.Tick();
    }

    private void FixedUpdate()
    {
        StateMachine.FixedTick();
    }

    public void Construct(Transform playerTransform, IEnemyMovementBounds movementBounds)
    {
        _target = playerTransform;
        Context?.SetTarget(playerTransform);
    }

    public void SetPeacefulMode(bool isPeacefulMode)
    {
        _isPeacefulMode = isPeacefulMode;
        Context?.SetPeacefulMode(isPeacefulMode);
    }

    public void NotifyHitByPlayer()
    {
        if (Context == null || Context.IsDead)
            return;

        Context.MarkProvokedByPlayer();
        StateMachine.ChangeState(AggroState, "Boss was hit by player");
    }

    public void MoveToTarget()
    {
        MoveToTarget(_chaseSpeed);
    }

    public void MoveToTarget(float speed)
    {
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh || !Context.HasTarget)
            return;

        _agent.speed = speed;
        _agent.isStopped = false;
        _agent.SetDestination(Context.Target.position);
        SetAnimatorBool(_isMovingParameter, true);
    }

    public void StopMovement()
    {
        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        SetAnimatorBool(_isMovingParameter, false);
    }

    public void FaceTargetImmediately()
    {
        FaceDirection(Context.DirectionToTarget, 1f);
    }

    public void FaceDirection(Vector3 direction, float deltaTime)
    {
        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Mathf.Max(1f, deltaTime * 12f));
    }

    public bool TryStartAttack()
    {
        if (!Context.CanUseAttack)
            return false;

        _attackAnimationFinished = false;
        _damageWindowOpenedThisAttack = false;
        _activeAttackDamage = _attackDamage;
        Context.TriggerAttackCooldown();
        DisableDamageHitboxes();
        SetAnimatorTrigger(_attackTriggerParameter);
        SetAnimatorFloat(_attackSpeedMultiplierParameter, Context.AttackSpeedMultiplier);
        return true;
    }

    public bool TryStartHeavyAttack()
    {
        if (!Context.CanUseHeavyAttack)
            return false;

        _attackAnimationFinished = false;
        _damageWindowOpenedThisAttack = false;
        _activeAttackDamage = _heavyAttackDamage;
        Context.TriggerHeavyAttackCooldown();
        DisableDamageHitboxes();
        SetAnimatorTrigger(_heavyAttackTriggerParameter);
        SetAnimatorFloat(_attackSpeedMultiplierParameter, Context.AttackSpeedMultiplier);
        return true;
    }

    public bool ConsumeAttackAnimationFinished()
    {
        if (!_attackAnimationFinished)
            return false;

        _attackAnimationFinished = false;
        return true;
    }

    public void EnableDamageHitboxes()
    {
        if (_damageHitboxes == null)
            return;

        _damageWindowOpenedThisAttack = true;
        foreach (BossDamageHitbox hitbox in _damageHitboxes)
        {
            if (hitbox == null)
                continue;

            hitbox.Configure(_activeAttackDamage, _damageType);
            hitbox.SetActive(true);
        }
    }

    public void DisableDamageHitboxes()
    {
        if (_damageHitboxes == null)
            return;

        foreach (BossDamageHitbox hitbox in _damageHitboxes)
        {
            if (hitbox != null)
                hitbox.SetActive(false);
        }
    }

    public void OnAttackAnimationFinished()
    {
        _attackAnimationFinished = true;
    }

    public void EnterPhaseTwo()
    {
        if (Context == null || Context.IsDead)
            return;

        Context.EnterEnrage(_enragedAttackSpeedMultiplier);
        SetAnimatorBool(_isEnragedParameter, true);
        SetAnimatorFloat(_attackSpeedMultiplierParameter, Context.AttackSpeedMultiplier);

        Debug.Log($"[{gameObject.name}] Boss entered phase 2. Attack speed multiplier: {_enragedAttackSpeedMultiplier}");
    }

    public void BeginEnrageAnimation()
    {
        StopMovement();
        SetAnimatorBool(_isMovingParameter, false);
        SetAnimatorBool(_isEnragedParameter, true);
        SetAnimatorFloat(_attackSpeedMultiplierParameter, Context.AttackSpeedMultiplier);
        SetAnimatorTrigger(_enrageTriggerParameter);
    }

    public void BeginHealAnimation()
    {
        StopMovement();
        SetAnimatorBool(_isMovingParameter, false);
        SetAnimatorTrigger(_healTriggerParameter);
    }

    public void Heal(float amount)
    {
        _health?.Heal(amount);
    }

    public void RestoreHealthToFull()
    {
        _health?.RestoreToFull();
    }

    public IBossState SelectMovementOrIdleState()
    {
        if (!Context.HasTarget || Context.HasLostTarget)
            return IdleState;

        if (Context.IsTargetInAttackRange)
            return AggroState;

        return ChaseState;
    }

    private void CacheComponents()
    {
        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();

        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_health == null)
            _health = GetComponent<EnemyHealth>();

        if (_damageHitboxes == null || _damageHitboxes.Length == 0)
            _damageHitboxes = GetComponentsInChildren<BossDamageHitbox>(true);

        if (_agent == null)
            Debug.LogWarning($"[{name}] BossController requires a NavMeshAgent for Aggro movement.");

        if (_animator == null)
            Debug.LogWarning($"[{name}] BossController has no Animator. Animation parameters will be skipped.");

        if (_health == null)
            Debug.LogWarning($"[{name}] BossController has no EnemyHealth. Phase 2 and death detection will be limited.");
    }

    private Transform FindPlayer()
    {
        GameObject player = FindWithTagIfExists("player");
        if (player == null)
            player = FindWithTagIfExists("Player");

        if (player != null)
            return player.transform;

        PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
        return playerController != null ? playerController.transform : null;
    }

    private GameObject FindWithTagIfExists(string tagName)
    {
        try
        {
            return GameObject.FindGameObjectWithTag(tagName);
        }
        catch (UnityException)
        {
            return null;
        }
    }

    private void SubscribeToHealth()
    {
        if (_health == null)
            return;

        _health.OnHealthChanged += HandleHealthChanged;
        _health.OnDamaged += HandleDamaged;
        _health.OnDied += HandleDied;
    }

    private void UnsubscribeFromHealth()
    {
        if (_health == null)
            return;

        _health.OnHealthChanged -= HandleHealthChanged;
        _health.OnDamaged -= HandleDamaged;
        _health.OnDied -= HandleDied;
    }

    private void HandleDamaged(DamageInfo damage)
    {
        if (IsDamageFromPlayer(damage))
            NotifyHitByPlayer();
    }

    private void HandleHealthChanged(float healthRatio)
    {
        if (healthRatio <= _phaseTwoHealthThreshold)
            Context?.RequestEnrage();
    }

    private void HandleDied()
    {
        DisableDamageHitboxes();
        StopMovement();
        StateMachine.ChangeState(DeathState, "Boss health depleted");
    }

    private bool IsDamageFromPlayer(DamageInfo damage)
    {
        if (damage.Source == null)
            return false;

        if (HasTag(damage.Source, "player") || HasTag(damage.Source, "Player"))
            return true;

        return damage.Source.GetComponentInParent<PlayerController>() != null;
    }

    private bool HasTag(GameObject source, string tagName)
    {
        try
        {
            return source.CompareTag(tagName);
        }
        catch (UnityException)
        {
            return false;
        }
    }

    private void DisableLegacyEnemyLogic()
    {
        if (TryGetComponent<EnemyAI>(out var enemyAI))
            enemyAI.enabled = false;

        if (TryGetComponent<EnemyRangedAI>(out var rangedAI))
            rangedAI.enabled = false;

        if (TryGetComponent<EnemyCombat>(out var enemyCombat))
            enemyCombat.enabled = false;

        if (TryGetComponent<EnemyRangedCombat>(out var rangedCombat))
            rangedCombat.enabled = false;

        if (TryGetComponent<BossCombat>(out var legacyBossCombat))
            legacyBossCombat.enabled = false;
    }

    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Bool))
            _animator.SetBool(parameterName, value);
    }

    private void SetAnimatorFloat(string parameterName, float value)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Float))
            _animator.SetFloat(parameterName, value);
    }

    private void SetAnimatorTrigger(string parameterName)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Trigger))
            _animator.SetTrigger(parameterName);
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (_animator == null || string.IsNullOrWhiteSpace(parameterName))
            return false;

        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == parameterType)
                return true;
        }

        string key = $"{parameterType}:{parameterName}";
        if (_missingAnimatorParameters.Add(key))
            Debug.LogWarning($"[{name}] Animator parameter '{parameterName}' ({parameterType}) was not found.");

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, LoseTargetRadius);
    }
}
