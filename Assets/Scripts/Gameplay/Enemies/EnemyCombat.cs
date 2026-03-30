using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public float damageAmount = 10f;
    public float attackCooldown = 5f;
    public float attackVisualDistance = 3.5f;

    private float _nextAttackTime;
    private Transform _player;

    public void Construct(Transform playerTransform)
    {
        _player = playerTransform;
    }

    public bool CanAttack => Time.time >= _nextAttackTime;

    public void ResetCooldownAfterTrigger()
    {
        _nextAttackTime = Time.time + attackCooldown;
    }

    public void Attack()
    {
        if (_player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist <= attackVisualDistance &&
            _player.TryGetComponent<IDamageable>(out var victim))
        {
            DamageInfo info = new DamageInfo(damageAmount, DamageType.Physical);
            victim.TakeDamage(info);
        }
    }
}