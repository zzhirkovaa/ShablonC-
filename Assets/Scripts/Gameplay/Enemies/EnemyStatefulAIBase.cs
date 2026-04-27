using UnityEngine;

public abstract class EnemyStatefulAIBase : MonoBehaviour
{
    protected EnemyStateMachine StateMachine { get; private set; }
    protected EnemyContext Context { get; private set; }

    private Animator _animator;
    private Rigidbody _rigidbody;
    private EnemyHealth _health;

    protected void InitializeStateMachine(
        Transform playerTransform,
        IEnemyMovementBounds movementBounds,
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
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _health = GetComponent<EnemyHealth>();

        Context = new EnemyContext(
            transform,
            _rigidbody,
            _animator,
            playerTransform,
            movementBounds,
            _health,
            detectionRadius,
            attackRange,
            moveSpeed,
            rotationSpeed,
            fleeDuration,
            fleeCooldownDuration,
            fleeHealthThreshold,
            fleeSafeDistanceMultiplier,
            fleeOnMeleeHit,
            fleeOnLowHealth);

        if (_health != null)
            _health.SetFleeContext(Context);

        StateMachine = new EnemyStateMachine(Context, gameObject.name);

        Context.IdleState = new EnemyIdleState(Context, StateMachine);
        Context.AggressionState = new EnemyAggressionState(Context, StateMachine);
        Context.FleeState = new EnemyFleeState(Context, StateMachine);
        Context.AttackState = CreateAttackState(Context, StateMachine);

        StateMachine.ChangeState(Context.IdleState, "Initial state");
    }

    protected void UpdateContextBindings(Transform playerTransform, IEnemyMovementBounds movementBounds)
    {
        Context?.UpdateBindings(playerTransform, movementBounds);
    }

    public void SetPeacefulMode(bool isPeacefulMode)
    {
        Context?.SetPeacefulMode(isPeacefulMode);
    }

    protected virtual void Update()
    {
        Context?.Tick(Time.deltaTime);
        StateMachine?.Tick();
    }

    protected virtual void FixedUpdate()
    {
        StateMachine?.FixedTick();
    }

    protected abstract IEnemyState CreateAttackState(EnemyContext context, EnemyStateMachine stateMachine);
}
