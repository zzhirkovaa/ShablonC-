using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public float damageAmount = 10f;
    public float attackCooldown = 5f;
    public float attackVisualDistance = 3.5f;

    private Transform _player;
    private EnemyCombatLogic _combatLogic;

    public void Construct(Transform playerTransform)
    {
        _player = playerTransform;
    }

    private void Awake()
    {
        _combatLogic = new EnemyCombatLogic(damageAmount, attackCooldown, attackVisualDistance);
    }

    public bool CanAttack => _combatLogic != null && _combatLogic.CanAttack;

    public void ResetCooldownAfterTrigger()
    {
        _combatLogic?.ResetCooldownAfterTrigger();
    }

    public void Attack()
    {
        if (_player == null || _combatLogic == null)
            return;

        if (_combatLogic.TryCreateDamage(transform.position, _player, out DamageInfo damage) &&
            _player.TryGetComponent<IDamageable>(out var victim))
        {
            victim.TakeDamage(damage);
        }
    }
}
