using UnityEngine;

public class EnemyRangedAI : EnemyStatefulAIBase
{
    [Header("Параметры обнаружения")]
    public float detectionRadius = 20f;
    public float attackRange = 12f;

    [Header("Параметры движения")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;

    [Header("Параметры бегства")]
    public float fleeDuration = 3f;
    public float fleeCooldownDuration = 3.5f;
    [Range(0.05f, 1f)] public float fleeHealthThreshold = 0.3f;
    public float fleeSafeDistanceMultiplier = 1.75f;
    public bool fleeOnMeleeHit = true;
    public bool fleeOnLowHealth = true;

    private Transform _playerTransform;
    private EnemyRangedCombat _combat;
    private IEnemyMovementBounds _movementBounds;

    public void Construct(Transform playerTransform, IEnemyMovementBounds movementBounds)
    {
        _playerTransform = playerTransform;
        _movementBounds = movementBounds;
        UpdateContextBindings(_playerTransform, _movementBounds);
    }

    private void Awake()
    {
        _combat = GetComponent<EnemyRangedCombat>();
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
        return new EnemyRangedAttackState(context, stateMachine, _combat);
    }
}
