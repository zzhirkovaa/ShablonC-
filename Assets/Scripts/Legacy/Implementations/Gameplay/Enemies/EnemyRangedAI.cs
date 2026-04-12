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

    public void Construct(Transform playerTransform, IEnemyMovementBounds movementBounds)
    {
        _playerTransform = playerTransform;
        _movementBounds = movementBounds;
    }

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _combat = GetComponent<EnemyRangedCombat>();
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);

        if (dist <= attackRange)
        {
            AttackState();
        }
        else if (dist <= detectionRadius)
        {
            FollowState();
        }
        else
        {
            IdleState();
        }
    }

    private void AttackState()
    {
        _anim.SetBool("IsRunning", false);

        transform.LookAt(new Vector3(
            _playerTransform.position.x,
            transform.position.y,
            _playerTransform.position.z));

        bool isAlreadyAttacking = _anim.GetCurrentAnimatorStateInfo(0).IsName("Attack");

        if (_combat.CanAttack && !isAlreadyAttacking)
        {
            _anim.SetTrigger("Attack");
            _combat.ResetCooldown();
        }
    }

    private void FollowState()
    {
        _anim.SetBool("IsRunning", true);
        _anim.ResetTrigger("Attack");

        Vector3 dir = (_playerTransform.position - transform.position).normalized;
        dir.y = 0f;

        Vector3 nextPosition = transform.position + dir * moveSpeed * Time.deltaTime;

        if (_movementBounds != null)
            nextPosition = _movementBounds.ClampPosition(nextPosition);

        transform.position = nextPosition;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 5f);
        }
    }

    private void IdleState()
    {
        _anim.SetBool("IsRunning", false);
    }
}