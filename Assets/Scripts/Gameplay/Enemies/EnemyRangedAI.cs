using UnityEngine;

public class EnemyRangedAI : MonoBehaviour
{
    public float detectionRadius = 20f;
    public float attackRange = 12f;
    public float moveSpeed = 2f;

    private Transform _playerTransform;
    private Animator _anim;
    private EnemyRangedCombat _combat;
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
        _combat = GetComponent<EnemyRangedCombat>();
        _brain = new EnemyAiBrain(detectionRadius, attackRange, moveSpeed);
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        EnemyAiDecision decision = _brain.Evaluate(
            transform.position,
            _playerTransform.position,
            _movementBounds,
            Time.deltaTime);

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

    private void AttackState(Vector3 lookDirection)
    {
        _anim.SetBool("IsRunning", false);

        if (lookDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDirection);

        bool isAlreadyAttacking = _anim.GetCurrentAnimatorStateInfo(0).IsName("Attack");

        if (_combat.CanAttack && !isAlreadyAttacking)
        {
            _anim.SetTrigger("Attack");
            _combat.ResetCooldown();
        }
    }

    private void FollowState(Vector3 direction, Vector3 nextPosition)
    {
        _anim.SetBool("IsRunning", true);
        _anim.ResetTrigger("Attack");

        transform.position = nextPosition;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                Time.deltaTime * 5f);
        }
    }

    private void IdleState()
    {
        _anim.SetBool("IsRunning", false);
    }
}
