using UnityEngine;

public class EnemyRangedAI : MonoBehaviour
{
    public float detectionRadius = 20f;
    public float attackRange = 12f;
    public float moveSpeed = 2f;

    private Transform _playerTransform;
    private Animator _anim;
    private EnemyRangedCombat _combat;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _combat = GetComponent<EnemyRangedCombat>();
        GameObject p = GameObject.FindGameObjectWithTag("player");
        if (p != null) _playerTransform = p.transform;
    }

    void Update()
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

        transform.LookAt(new Vector3(_playerTransform.position.x, transform.position.y, _playerTransform.position.z));

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
        dir.y = 0;
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
    }

    private void IdleState()
    {
        _anim.SetBool("IsRunning", false);
    }
}