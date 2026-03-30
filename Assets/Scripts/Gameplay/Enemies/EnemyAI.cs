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

    public void Construct(Transform playerTransform, IEnemyMovementBounds movementBounds)
    {
        _playerTransform = playerTransform;
        _movementBounds = movementBounds;
    }

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _combat = GetComponent<EnemyCombat>();
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);

        if (dist <= attackRange)
            AttackState();
        else if (dist <= detectionRadius)
            FollowState();
        else
            IdleState();
    }

    private void IdleState()
    {
        _anim.SetBool("IsRunning", false);
        _anim.ResetTrigger("Attack");
    }

    private void FollowState()
    {
        _anim.ResetTrigger("Attack");
        _anim.SetBool("IsRunning", true);

        Vector3 dir = (_playerTransform.position - transform.position).normalized;
        dir.y = 0f;

        Vector3 nextPosition = transform.position + dir * moveSpeed * Time.deltaTime;

        if (_movementBounds != null)
            nextPosition = _movementBounds.ClampPosition(nextPosition);

        transform.position = nextPosition;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    private void AttackState()
    {
        _anim.SetBool("IsRunning", false);

        bool isAlreadyAttacking = _anim.GetCurrentAnimatorStateInfo(0).IsName("Attack");

        if (_combat.CanAttack && !isAlreadyAttacking)
        {
            _anim.SetTrigger("Attack");
            _combat.ResetCooldownAfterTrigger();
        }
    }
}