using UnityEngine;

public sealed class EnemyContext
{
    private const float PeacefulFleeHealthThreshold = 0.5f;

    public EnemyContext(
        Transform transform,
        Rigidbody rigidbody,
        Animator animator,
        Transform playerTransform,
        IEnemyMovementBounds movementBounds,
        EnemyHealth health,
        float detectionRadius,
        float attackRange,
        float moveSpeed,
        float rotationSpeed,
        float fleeDuration,
        float fleeCooldownDuration,
        float fleeHealthThreshold,
        float fleeSafeDistanceMultiplier,
        bool fleeOnMeleeHit,
        bool fleeOnLowHealth)
    {
        Transform = transform;
        Rigidbody = rigidbody;
        Animator = animator;
        PlayerTransform = playerTransform;
        MovementBounds = movementBounds;
        Health = health;
        DetectionRadius = detectionRadius;
        AttackRange = attackRange;
        MoveSpeed = moveSpeed;
        RotationSpeed = rotationSpeed;
        FleeDuration = fleeDuration;
        FleeCooldownDuration = fleeCooldownDuration;
        FleeHealthThreshold = fleeHealthThreshold;
        FleeSafeDistanceMultiplier = fleeSafeDistanceMultiplier;
        FleeOnMeleeHit = fleeOnMeleeHit;
        FleeOnLowHealth = fleeOnLowHealth;
        FleeReason = EnemyFleeReason.None;
    }

    public Transform Transform { get; }
    public Rigidbody Rigidbody { get; }
    public Animator Animator { get; }
    public Transform PlayerTransform { get; private set; }
    public IEnemyMovementBounds MovementBounds { get; private set; }
    public EnemyHealth Health { get; }

    public float DetectionRadius { get; }
    public float AttackRange { get; }
    public float MoveSpeed { get; }
    public float RotationSpeed { get; }
    public float FleeDuration { get; }
    public float FleeCooldownDuration { get; }
    public float FleeHealthThreshold { get; }
    public float FleeSafeDistanceMultiplier { get; }
    public bool FleeOnMeleeHit { get; }
    public bool FleeOnLowHealth { get; }

    public bool WasMeleeHit { get; private set; }
    public bool FleeRequested { get; private set; }
    public bool IsFleeing { get; private set; }
    public bool IsPeacefulMode { get; private set; }
    public bool PanicRequested { get; private set; }
    public EnemyFleeReason FleeReason { get; private set; }

    private float _fleeCooldownRemaining;

    public IEnemyState IdleState { get; set; }
    public IEnemyState AggressionState { get; set; }
    public IEnemyState AttackState { get; set; }
    public IEnemyState FleeState { get; set; }

    public void UpdateBindings(Transform playerTransform, IEnemyMovementBounds movementBounds)
    {
        PlayerTransform = playerTransform;
        MovementBounds = movementBounds;
    }

    public void Tick(float deltaTime)
    {
        if (_fleeCooldownRemaining > 0f)
            _fleeCooldownRemaining = Mathf.Max(0f, _fleeCooldownRemaining - deltaTime);
    }

    public bool HasPlayer => PlayerTransform != null;
    public bool CanStartFlee => !IsFleeing && _fleeCooldownRemaining <= 0f;

    public float DistanceToPlayer
    {
        get
        {
            if (!HasPlayer)
                return float.PositiveInfinity;

            return Vector3.Distance(Transform.position, PlayerTransform.position);
        }
    }

    public bool HasDetectedPlayer => HasPlayer && DistanceToPlayer <= DetectionRadius;
    public bool IsPlayerInAttackRange => HasPlayer && DistanceToPlayer <= AttackRange;
    public bool HasReachedFleeSafety => HasPlayer && DistanceToPlayer >= DetectionRadius * FleeSafeDistanceMultiplier;

    public bool IsHealthCritical
    {
        get
        {
            if (Health == null || Health.MaxHealth <= 0f || Health.IsDead)
                return false;

            return Health.CurrentHealth / Health.MaxHealth <= FleeHealthThreshold;
        }
    }

    public bool ShouldFleeInPeacefulMode
    {
        get
        {
            if (Health == null || Health.MaxHealth <= 0f || Health.IsDead)
                return false;

            return Health.CurrentHealth / Health.MaxHealth <= PeacefulFleeHealthThreshold;
        }
    }

    public bool ShouldEnterFlee
    {
        get
        {
            if (!CanStartFlee)
                return false;

            if (FleeRequested || PanicRequested)
                return true;

            if (IsPeacefulMode)
                return ShouldFleeInPeacefulMode;

            return FleeOnLowHealth && IsHealthCritical;
        }
    }

    public void RequestFlee(EnemyFleeReason reason, bool wasMeleeHit = false)
    {
        if (!CanStartFlee)
            return;

        FleeRequested = true;
        WasMeleeHit = wasMeleeHit;
        FleeReason = reason;
    }

    public void SetPeacefulMode(bool isPeacefulMode)
    {
        IsPeacefulMode = isPeacefulMode;
    }

    public void RequestPanic()
    {
        if (!CanStartFlee)
            return;

        PanicRequested = true;
        FleeReason = EnemyFleeReason.Panic;
    }

    public void EnterFlee()
    {
        IsFleeing = true;
    }

    public void ClearFleeRequest()
    {
        FleeRequested = false;
        WasMeleeHit = false;
        PanicRequested = false;
        IsFleeing = false;
        FleeReason = EnemyFleeReason.None;
        _fleeCooldownRemaining = FleeCooldownDuration;
    }

    public string GetFleeReasonLabel()
    {
        return FleeReason switch
        {
            EnemyFleeReason.MeleeHit => "Received melee hit",
            EnemyFleeReason.LowHealth => "Critical health threshold reached",
            EnemyFleeReason.Panic => "Panic flag requested flee",
            _ => "Flee requested"
        };
    }

    public Vector3 GetDirectionToPlayer()
    {
        if (!HasPlayer)
            return Vector3.zero;

        Vector3 direction = PlayerTransform.position - Transform.position;
        direction.y = 0f;
        return direction.normalized;
    }

    public Vector3 GetDirectionAwayFromPlayer()
    {
        Vector3 direction = -GetDirectionToPlayer();
        direction.y = 0f;
        return direction.normalized;
    }

    public void Move(Vector3 direction, float deltaTime)
    {
        if (direction == Vector3.zero)
        {
            StopMotion();
            return;
        }

        Vector3 targetPosition = Transform.position + direction.normalized * MoveSpeed * deltaTime;
        if (MovementBounds != null)
            targetPosition = MovementBounds.ClampPosition(targetPosition);

        if (Rigidbody != null && !Rigidbody.isKinematic)
            Rigidbody.MovePosition(targetPosition);
        else
            Transform.position = targetPosition;
    }

    public void StopMotion()
    {
        if (Rigidbody != null)
            Rigidbody.linearVelocity = Vector3.zero;
    }

    public void FaceDirection(Vector3 direction, float deltaTime)
    {
        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Transform.rotation = Quaternion.Slerp(Transform.rotation, targetRotation, deltaTime * RotationSpeed);
    }

    public void FacePlayerImmediately()
    {
        Vector3 direction = GetDirectionToPlayer();
        if (direction != Vector3.zero)
            Transform.rotation = Quaternion.LookRotation(direction);
    }

    public bool IsAnimatorStatePlaying(string stateName)
    {
        return Animator != null && Animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    public void SetAnimatorBool(string parameterName, bool value)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Bool))
            Animator.SetBool(parameterName, value);
    }

    public void SetAnimatorTrigger(string parameterName)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Trigger))
            Animator.SetTrigger(parameterName);
    }

    public void ResetAnimatorTrigger(string parameterName)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Trigger))
            Animator.ResetTrigger(parameterName);
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (Animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter in Animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == parameterType)
                return true;
        }

        return false;
    }
}
