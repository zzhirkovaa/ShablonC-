using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Параметры обнаружения")]
    public float detectionRadius = 10f;
    public float attackRange = 2.2f;

    [Header("Параметры движения")]
    public float moveSpeed = 3f;

    private Transform _playerTransform;
    private Animator _anim;
    private EnemyCombat _combat;
    private IEnemyMovementBounds _movementBounds;
    private EnemyAiBrain _brain;

    public void Construct(Transform playerTransform, IEnemyMovementBounds movementBounds)
    {
        _playerTransform = playerTransform;
        _movementBounds = movementBounds;
    }

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _combat = GetComponent<EnemyCombat>();
        _brain = new EnemyAiBrain(detectionRadius, attackRange, moveSpeed);
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        EnemyAiDecision decision = _brain.Evaluate(
            transform.position,
            _playerTransform.position,
            _movementBounds);

        switch (decision.Type)
        {
            case EnemyAiDecisionType.Attack:
                AttackState(decision.Direction);
                break;
            case EnemyAiDecisionType.Follow:
                FollowState(decision.Direction, decision.TargetPosition);
                break;
            default:
                IdleState();
                break;
        }
    }

    private void IdleState()
    {
        _anim.SetBool("IsRunning", false);
        _anim.ResetTrigger("Attack");
    }

    private void FollowState(Vector3 direction, Vector3 nextPosition)
    {
        _anim.ResetTrigger("Attack");
        _anim.SetBool("IsRunning", true);

        transform.position = nextPosition;

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    private void AttackState(Vector3 lookDirection)
    {
        _anim.SetBool("IsRunning", false);

        if (lookDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDirection);

        bool isAlreadyAttacking = _anim.GetCurrentAnimatorStateInfo(0).IsName("Attack");

        if (_combat.CanAttack && !isAlreadyAttacking)
        {
            _anim.SetTrigger("Attack");
            _combat.ResetCooldownAfterTrigger();
        }
    }
}
