using UnityEngine;

public class EnemyAI : EnemyStatefulAIBase
{
    [Header("Параметры обнаружения")]
    public float detectionRadius = 10f;
    public float attackRange = 2.2f;

    [Header("Параметры движения")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;

    [Header("Параметры бегства")]
    public float fleeDuration = 2f;
    public float fleeCooldownDuration = 4f;
    [Range(0.05f, 1f)] public float fleeHealthThreshold = 0.15f;
    public float fleeSafeDistanceMultiplier = 1.25f;
    public bool fleeOnMeleeHit = false;
    public bool fleeOnLowHealth = true;

    private Transform _playerTransform;
    private EnemyCombat _combat;
    private IEnemyMovementBounds _movementBounds;

    public void Construct(Transform playerTransform, IEnemyMovementBounds movementBounds)
    {
        _playerTransform = playerTransform;
        _movementBounds = movementBounds;
        UpdateContextBindings(_playerTransform, _movementBounds);
    }

    private void Awake()
    {
        _combat = GetComponent<EnemyCombat>();
        InitializeStateMachine(
            _playerTransform,
            _movementBounds,
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
    }

    protected override IEnemyState CreateAttackState(EnemyContext context, EnemyStateMachine stateMachine)
    {
        return new EnemyMeleeAttackState(context, stateMachine, _combat);
    }
}
