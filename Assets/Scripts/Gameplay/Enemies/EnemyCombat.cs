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
        RebuildCombatLogic();
    }

    public bool CanAttack(float currentTime) => _combatLogic != null && _combatLogic.CanAttack(currentTime);

    public void ResetCooldownAfterTrigger(float currentTime)
    {
        _combatLogic?.ResetCooldownAfterTrigger(currentTime);
    }

    public void Attack()
    {
        if (_player == null || _combatLogic == null)
            return;

        if (_combatLogic.TryCreateDamage(transform.position, _player.position, out DamageInfo damage) &&
            _player.TryGetComponent<IDamageable>(out var victim))
        {
            damage.Source = gameObject;
            victim.TakeDamage(damage);
        }
    }

    public void ApplyWeaponConfig(MeleeWeaponConfigSO weaponConfig)
    {
        if (weaponConfig == null)
            return;

        damageAmount = weaponConfig.Damage;
        attackCooldown = weaponConfig.AttackCooldown;
        attackVisualDistance = weaponConfig.AttackRange;
        RebuildCombatLogic();
    }

    private void RebuildCombatLogic()
    {
        _combatLogic = new EnemyCombatLogic(damageAmount, attackCooldown, attackVisualDistance);
    }
}
