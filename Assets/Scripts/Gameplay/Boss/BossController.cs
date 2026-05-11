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
    [SerializeField] private bool _disableAnimatorRootMotion = true;

    [Header("Damage")]
    [SerializeField] private float _attackDamage = 18f;
    [SerializeField] private float _heavyAttackDamage = 36f;
    [SerializeField] private DamageType _damageType = DamageType.Physical;
    [SerializeField] private BossDamageHitbox[] _damageHitboxes;

    [Header("Elements")]
    [SerializeField] private BossElementConfig[] _elementConfigs;
    [SerializeField] private BossElementVisuals _elementVisuals;
    [SerializeField] private BossAttackType _currentAttackType = BossAttackType.Kick;
    [SerializeField] private BossElementType _currentElementType = BossElementType.Fire;
    [SerializeField] private bool _randomizeLoadoutOnStart = true;
    [SerializeField] private bool _canChangeElementDuringFight = true;
    [SerializeField] private bool _canChangeAttackTypeDuringFight = true;
    [SerializeField] private float _changeInterval = 8f;

    [Header("Ranged Fire Attack")]
    [SerializeField] private float _fireRangedAttackRange = 18f;
    [SerializeField, Range(0f, 1f)] private float _fireRangedAttackChance = 0.35f;
    [SerializeField] private float _fireRangedDecisionCooldown = 2.5f;
    [SerializeField] private bool _allowFireRangedAttackWhenOtherElementActive = true;

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

    [Header("Death")]
    [SerializeField] private float _destroyAfterDeathDelay = 5f;

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
    [SerializeField] private string _dieTriggerParameter = "Die";

    private readonly HashSet<string> _missingAnimatorParameters = new HashSet<string>();
    private bool _attackAnimationFinished;
    private bool _damageWindowOpenedThisAttack;
    private bool _elementMomentTriggered;
    private bool _deathStarted;
    private bool _hasLastPerformedAttack;
    private BossAttackType _lastPerformedAttackType;
    private BossElementType _lastPerformedElementType;
    private float _activeAttackDamage;
    private float _loadoutChangeTimer;
    private float _nextFireRangedDecisionTime;

    public BossStateMachine StateMachine { get; private set; }
    public BossContext Context { get; private set; }

    public float DetectionRadius => _detectionRadius;
    public float LoseTargetRadius => Mathf.Max(_loseTargetRadius, _detectionRadius);
    public float AttackRange => _attackRange;
    public float AttackDamage => _attackDamage;
    public float HeavyAttackDamage => _heavyAttackDamage;
    public float AttackCooldown => _attackCooldown;
    public float HeavyAttackCooldown => _heavyAttackCooldown;
    public float AttackDuration => GetAttackDuration(BossAttackType.Kick);
    public float HeavyAttackDuration => GetAttackDuration(BossAttackType.HeavyHands);
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
    public BossAttackType CurrentAttackType => _currentAttackType;
    public BossElementType CurrentElementType => _currentElementType;
    public float FireRangedAttackRange => Mathf.Max(_attackRange, _fireRangedAttackRange);
    private float CurrentAttackSpeedMultiplier => Context != null ? Context.AttackSpeedMultiplier : 1f;

    private void Awake()
    {
        CacheComponents();
        DisableLegacyEnemyLogic();

        Context = new BossContext(this, transform, _agent, _animator, _health);
        Context.SetPeacefulMode(_isPeacefulMode);
        StateMachine = new BossStateMachine(Context, gameObject.name);

        SubscribeToHealth();
        DisableDamageHitboxes();
        StateMachine.ChangeState(BossStateType.Idle, "Initial boss state");
    }

    private void Start()
    {
        if (_target == null)
            _target = FindPlayer();

        Context.SetTarget(_target);

        if (_randomizeLoadoutOnStart)
            RandomizeLoadout();
    }

    private void OnDestroy()
    {
        UnsubscribeFromHealth();
    }

    private void Update()
    {
        Context.Tick(Time.deltaTime);
        TickLoadoutChange(Time.deltaTime);
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
        StateMachine.ChangeState(BossStateType.Aggro, "Boss was hit by player");
    }

    public void ActivateBoss()
    {
        if (_target == null)
            _target = FindPlayer();

        Context?.SetTarget(_target);
        RandomizeLoadout();

        if (StateMachine != null)
            StateMachine.ChangeState(BossStateType.Idle, "Boss activated after enemy kills");
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
            _agent.velocity = Vector3.zero;
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

        if (WouldRepeatLastAttackType(BossAttackType.Kick))
            return false;

        AvoidRepeatingLastElementIfPossible();

        _attackAnimationFinished = false;
        _damageWindowOpenedThisAttack = false;
        _elementMomentTriggered = false;
        _activeAttackDamage = _attackDamage;
        Context.TriggerAttackCooldown();
        RegisterStartedAttack(BossAttackType.Kick);
        DisableDamageHitboxes();
        ResetAttackAnimatorTriggers();
        SetAnimatorTrigger(ResolveAttackTrigger(BossAttackType.Kick, _attackTriggerParameter));
        SetAnimatorFloat(_attackSpeedMultiplierParameter, Context.AttackSpeedMultiplier);
        return true;
    }

    public bool TryStartHeavyAttack()
    {
        if (!Context.CanUseHeavyAttack)
            return false;

        if (WouldRepeatLastAttackType(BossAttackType.HeavyHands))
            return false;

        AvoidRepeatingLastElementIfPossible();

        _attackAnimationFinished = false;
        _damageWindowOpenedThisAttack = false;
        _elementMomentTriggered = false;
        _activeAttackDamage = _heavyAttackDamage;
        Context.TriggerHeavyAttackCooldown();
        RegisterStartedAttack(BossAttackType.HeavyHands);
        DisableDamageHitboxes();
        ResetAttackAnimatorTriggers();
        SetAnimatorTrigger(ResolveAttackTrigger(BossAttackType.HeavyHands, _heavyAttackTriggerParameter));
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

    public bool TrySelectReadyAttackState(out BossStateType stateType)
    {
        AvoidRepeatingLastElementIfPossible();

        if (TryGetReadyAttackState(_currentAttackType, out stateType))
            return true;

        BossAttackType fallbackAttackType = GetOppositeAttackType(_currentAttackType);
        return TryGetReadyAttackState(fallbackAttackType, out stateType);
    }

    private bool TryGetReadyAttackState(BossAttackType attackType, out BossStateType stateType)
    {
        stateType = attackType == BossAttackType.HeavyHands
            ? BossStateType.HeavyAttack
            : BossStateType.Attack;

        if (WouldRepeatLastAttackType(attackType))
            return false;

        if (attackType == BossAttackType.HeavyHands)
            return Context.CanUseHeavyAttack;

        return Context.CanUseAttack;
    }

    public bool TrySelectReadyRangedFireAttackState(bool forceIfFireIsActive, out BossStateType stateType)
    {
        stateType = BossStateType.Attack;

        if (!CanStartRangedFireAttack(forceIfFireIsActive))
            return false;

        _currentElementType = BossElementType.Fire;
        return TrySelectReadyAttackState(out stateType);
    }

    public void OpenAttackDamageWindow()
    {
        _damageWindowOpenedThisAttack = true;
        PerformCurrentElementMoment();

        BossElementConfig config = GetCurrentElementConfig();
        if (ShouldUseMeleeHitboxes(config))
            EnableDamageHitboxes();
    }

    public void EnableDamageHitboxes()
    {
        _damageWindowOpenedThisAttack = true;
        PerformCurrentElementMoment();

        BossElementConfig config = GetCurrentElementConfig();
        if (!ShouldUseMeleeHitboxes(config))
            return;

        if (_damageHitboxes == null)
            return;

        foreach (BossDamageHitbox hitbox in _damageHitboxes)
        {
            if (hitbox == null)
                continue;

            hitbox.TargetDamaged -= HandleHitboxTargetDamaged;
            hitbox.TargetDamaged += HandleHitboxTargetDamaged;
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
            if (hitbox == null)
                continue;

            hitbox.TargetDamaged -= HandleHitboxTargetDamaged;
            hitbox.SetActive(false);
        }
    }

    public void OnAttackAnimationFinished()
    {
        ClearActiveElementEffects();
        _attackAnimationFinished = true;
    }

    public void ClearActiveElementEffects()
    {
        _elementVisuals?.ClearAirEffect();
    }

    public void PrepareAnimatorForPostAttack(BossStateType nextStateType)
    {
        ClearActiveElementEffects();
        ResetAttackAnimatorTriggers();
        SetAnimatorBool(_isMovingParameter, nextStateType == BossStateType.Chase);
    }

    public void OnFireCastMoment()
    {
        PerformCurrentElementMoment(BossElementType.Fire);
    }

    public void OnEarthStompMoment()
    {
        PerformCurrentElementMoment(BossElementType.Earth);
    }

    public void OnAirCastMoment()
    {
        PerformCurrentElementMoment(BossElementType.Air);
    }

    public void OnIceHitMoment()
    {
        PerformCurrentElementMoment(BossElementType.Ice);
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

    public void BeginDeath()
    {
        if (_deathStarted)
            return;

        _deathStarted = true;
        DisableDamageHitboxes();
        ClearActiveElementEffects();
        StopMovement();
        ResetAttackAnimatorTriggers();
        SetAnimatorBool(_isMovingParameter, false);
        SetAnimatorTrigger(_dieTriggerParameter);

        Destroy(gameObject, Mathf.Max(0f, _destroyAfterDeathDelay));
    }

    public void Heal(float amount)
    {
        _health?.Heal(amount);
    }

    public void RestoreHealthToFull()
    {
        _health?.RestoreToFull();
    }

    public float GetAttackDuration(BossAttackType attackType)
    {
        BossElementConfig config = GetCurrentElementConfig();
        float overrideDuration = config != null ? config.GetDurationOverride(attackType) : 0f;
        float baseDuration = overrideDuration > 0f
            ? overrideDuration
            : attackType == BossAttackType.HeavyHands ? _heavyAttackDuration : _attackDuration;

        return baseDuration / CurrentAttackSpeedMultiplier;
    }

    private void TickLoadoutChange(float deltaTime)
    {
        if ((!_canChangeElementDuringFight && !_canChangeAttackTypeDuringFight) || _changeInterval <= 0f)
            return;

        if (Context == null || Context.IsDead || !Context.HasTarget)
            return;

        if (IsAttackStateActive())
            return;

        _loadoutChangeTimer -= deltaTime;
        if (_loadoutChangeTimer > 0f)
            return;

        if (_canChangeAttackTypeDuringFight)
            _currentAttackType = GetRandomAttackType(true, _hasLastPerformedAttack ? _lastPerformedAttackType : _currentAttackType);

        if (_canChangeElementDuringFight)
            _currentElementType = GetRandomElementType(true, _hasLastPerformedAttack ? _lastPerformedElementType : _currentElementType);

        ResetLoadoutTimer();
    }

    private void RandomizeLoadout()
    {
        _currentAttackType = GetRandomAttackType(_hasLastPerformedAttack, _lastPerformedAttackType);
        _currentElementType = GetRandomElementType(_hasLastPerformedAttack, _lastPerformedElementType);
        ResetLoadoutTimer();

        Debug.Log($"[{gameObject.name}] Boss loadout: {_currentAttackType} + {_currentElementType}");
    }

    private bool IsAttackStateActive()
    {
        if (StateMachine == null)
            return false;

        return StateMachine.IsCurrentState<BossAttackState>()
            || StateMachine.IsCurrentState<BossHeavyAttackState>();
    }

    private void ResetLoadoutTimer()
    {
        _loadoutChangeTimer = Mathf.Max(0.1f, _changeInterval);
    }

    private BossAttackType GetRandomAttackType(bool excludeAttackType, BossAttackType excludedAttackType)
    {
        if (excludeAttackType)
            return GetOppositeAttackType(excludedAttackType);

        return Random.value < 0.5f ? BossAttackType.Kick : BossAttackType.HeavyHands;
    }

    private BossAttackType GetOppositeAttackType(BossAttackType attackType)
    {
        return attackType == BossAttackType.Kick
            ? BossAttackType.HeavyHands
            : BossAttackType.Kick;
    }

    private BossElementType GetRandomElementType(bool excludeElementType, BossElementType excludedElementType)
    {
        List<BossElementType> availableTypes = GetAvailableElementTypes();

        if (excludeElementType && availableTypes.Count > 1)
            availableTypes.Remove(excludedElementType);

        if (availableTypes.Count > 0)
            return availableTypes[Random.Range(0, availableTypes.Count)];

        return excludedElementType;
    }

    private List<BossElementType> GetAvailableElementTypes()
    {
        List<BossElementType> availableTypes = new List<BossElementType>();

        if (_elementConfigs != null)
        {
            foreach (BossElementConfig config in _elementConfigs)
            {
                if (config != null && !availableTypes.Contains(config.ElementType))
                    availableTypes.Add(config.ElementType);
            }
        }

        if (availableTypes.Count > 0)
            return availableTypes;

        foreach (BossElementType elementType in System.Enum.GetValues(typeof(BossElementType)))
            availableTypes.Add(elementType);

        return availableTypes;
    }

    private BossElementConfig GetCurrentElementConfig()
    {
        if (_elementConfigs == null)
            return null;

        foreach (BossElementConfig config in _elementConfigs)
        {
            if (config != null && config.ElementType == _currentElementType)
                return config;
        }

        return null;
    }

    private bool CanStartRangedFireAttack(bool forceIfFireIsActive)
    {
        if (Context == null || !Context.HasTarget || Context.HasLostTarget || Context.IsTargetInAttackRange)
            return false;

        if (Context.DistanceToTarget > FireRangedAttackRange)
            return false;

        if (!Context.CanUseAttack && !Context.CanUseHeavyAttack)
            return false;

        if (!HasElementConfig(BossElementType.Fire))
            return false;

        bool fireIsActive = _currentElementType == BossElementType.Fire;
        if (!fireIsActive && !_allowFireRangedAttackWhenOtherElementActive)
            return false;

        if (_hasLastPerformedAttack && _lastPerformedElementType == BossElementType.Fire)
            return false;

        if (fireIsActive && forceIfFireIsActive)
            return true;

        if (Time.time < _nextFireRangedDecisionTime)
            return false;

        _nextFireRangedDecisionTime = Time.time + Mathf.Max(0.1f, _fireRangedDecisionCooldown);
        return Random.value <= _fireRangedAttackChance;
    }

    private bool HasElementConfig(BossElementType elementType)
    {
        if (_elementConfigs == null)
            return false;

        foreach (BossElementConfig config in _elementConfigs)
        {
            if (config != null && config.ElementType == elementType)
                return true;
        }

        return false;
    }

    private void AvoidRepeatingLastElementIfPossible()
    {
        if (!_hasLastPerformedAttack || _currentElementType != _lastPerformedElementType)
            return;

        _currentElementType = GetRandomElementType(true, _lastPerformedElementType);
    }

    private bool WouldRepeatLastAttackType(BossAttackType attackType)
    {
        return _hasLastPerformedAttack && _lastPerformedAttackType == attackType;
    }

    private void RegisterStartedAttack(BossAttackType attackType)
    {
        _lastPerformedAttackType = attackType;
        _lastPerformedElementType = _currentElementType;
        _hasLastPerformedAttack = true;
    }

    private string ResolveAttackTrigger(BossAttackType attackType, string fallbackTrigger)
    {
        BossElementConfig config = GetCurrentElementConfig();
        string configuredTrigger = config != null ? config.GetTriggerName(attackType) : null;
        return string.IsNullOrWhiteSpace(configuredTrigger) ? fallbackTrigger : configuredTrigger;
    }

    private void ResetAttackAnimatorTriggers()
    {
        ResetAnimatorTrigger(_attackTriggerParameter);
        ResetAnimatorTrigger(_heavyAttackTriggerParameter);

        if (_elementConfigs == null)
            return;

        foreach (BossElementConfig config in _elementConfigs)
        {
            if (config == null)
                continue;

            ResetAnimatorTrigger(config.GetTriggerName(BossAttackType.Kick));
            ResetAnimatorTrigger(config.GetTriggerName(BossAttackType.HeavyHands));
        }
    }

    private void PerformCurrentElementMoment()
    {
        PerformCurrentElementMoment(_currentElementType);
    }

    private void PerformCurrentElementMoment(BossElementType expectedElement)
    {
        if (_elementMomentTriggered || _currentElementType != expectedElement)
            return;

        _elementMomentTriggered = true;

        BossElementConfig config = GetCurrentElementConfig();
        if (config == null)
            return;

        switch (_currentElementType)
        {
            case BossElementType.Fire:
                ExecuteFire(config);
                break;
            case BossElementType.Earth:
                ExecuteEarth(config);
                break;
            case BossElementType.Air:
                ExecuteAir(config);
                break;
            case BossElementType.Ice:
                _elementVisuals?.SpawnIceEffects(config);
                break;
        }
    }

    private void ExecuteFire(BossElementConfig config)
    {
        StopMovement();

        if (config.FireProjectilePrefab == null)
            return;

        Transform firePoint = _elementVisuals != null ? _elementVisuals.FirePoint : transform;
        Vector3 targetPosition = Context.HasTarget ? Context.Target.position + Vector3.up : firePoint.position + firePoint.forward;
        Vector3 direction = (targetPosition - firePoint.position).normalized;
        if (direction == Vector3.zero)
            direction = firePoint.forward;

        GameObject projectileObject = Instantiate(config.FireProjectilePrefab, firePoint.position, Quaternion.LookRotation(direction, Vector3.up));
        FireProjectile projectile = projectileObject.GetComponent<FireProjectile>();
        if (projectile == null)
            projectile = projectileObject.AddComponent<FireProjectile>();

        projectile.Initialize(
            direction,
            config.FireProjectileSpeed,
            config.FireDirectDamage,
            config.FireBurnDamagePerSecond,
            config.FireBurnDuration,
            gameObject);
    }

    private void ExecuteEarth(BossElementConfig config)
    {
        _elementVisuals?.SpawnEarthEffect(config);

        PlayerStatusEffects targetStatus = GetTargetStatusEffects();
        if (targetStatus == null || !Context.HasTarget)
            return;

        float distance = Vector3.Distance(transform.position, Context.Target.position);
        if (distance <= config.EarthRadius)
            targetStatus.KnockUp(config.EarthKnockUpHeight);
    }

    private void ExecuteAir(BossElementConfig config)
    {
        StopMovement();

        Transform airPoint = _elementVisuals != null ? _elementVisuals.AirPoint : transform;
        Vector3 targetPosition = Context.HasTarget ? Context.Target.position + Vector3.up : airPoint.position + airPoint.forward;
        Vector3 direction = (targetPosition - airPoint.position).normalized;
        if (direction == Vector3.zero)
            direction = airPoint.forward;

        GameObject projectilePrefab = config.AirProjectilePrefab;
        if (projectilePrefab == null)
            return;

        GameObject projectileObject = Instantiate(projectilePrefab, airPoint.position, Quaternion.LookRotation(direction, Vector3.up));
        AirProjectile projectile = projectileObject.GetComponent<AirProjectile>();
        if (projectile == null)
            projectile = projectileObject.AddComponent<AirProjectile>();

        projectile.Initialize(
            direction,
            config.AirProjectileSpeed,
            config.AirSpinDuration,
            config.AirSpinDegrees,
            gameObject);
    }

    private void HandleHitboxTargetDamaged(Collider targetCollider)
    {
        BossElementConfig config = GetCurrentElementConfig();
        if (config == null || config.ElementType != BossElementType.Ice)
            return;

        PlayerStatusEffects statusEffects = FindOrCreateStatusEffects(targetCollider);
        statusEffects?.Freeze(config.IceFreezeDuration);
    }

    private bool ShouldUseMeleeHitboxes(BossElementConfig config)
    {
        if (_currentElementType == BossElementType.Air || _currentElementType == BossElementType.Fire)
            return false;

        return config == null || config.UsesMeleeHitboxes;
    }

    private PlayerStatusEffects GetTargetStatusEffects()
    {
        if (!Context.HasTarget)
            return null;

        PlayerStatusEffects statusEffects = Context.Target.GetComponentInParent<PlayerStatusEffects>();
        if (statusEffects != null)
            return statusEffects;

        PlayerController playerController = Context.Target.GetComponentInParent<PlayerController>();
        return playerController != null
            ? playerController.gameObject.AddComponent<PlayerStatusEffects>()
            : null;
    }

    private static PlayerStatusEffects FindOrCreateStatusEffects(Collider targetCollider)
    {
        if (targetCollider == null)
            return null;

        PlayerStatusEffects statusEffects = targetCollider.GetComponentInParent<PlayerStatusEffects>();
        if (statusEffects != null)
            return statusEffects;

        PlayerController playerController = targetCollider.GetComponentInParent<PlayerController>();
        return playerController != null
            ? playerController.gameObject.AddComponent<PlayerStatusEffects>()
            : null;
    }

    public BossStateType SelectMovementOrIdleState()
    {
        if (!Context.HasTarget || Context.HasLostTarget)
            return BossStateType.Idle;

        if (Context.IsTargetInAttackRange)
            return BossStateType.Aggro;

        return BossStateType.Chase;
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

        if (_elementVisuals == null)
            _elementVisuals = GetComponentInChildren<BossElementVisuals>(true);

        if (_animator != null && _disableAnimatorRootMotion)
            _animator.applyRootMotion = false;

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
        StateMachine.ChangeState(BossStateType.Death, "Boss health depleted");
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

    private void ResetAnimatorTrigger(string parameterName)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Trigger))
            _animator.ResetTrigger(parameterName);
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
