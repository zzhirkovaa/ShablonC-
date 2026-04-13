using UnityEngine;

public sealed class EnemyCombatLogic
{
    private readonly float _damageAmount;
    private readonly float _attackCooldown;
    private readonly float _attackVisualDistance;

    private float _nextAttackTime;

    public EnemyCombatLogic(float damageAmount, float attackCooldown, float attackVisualDistance)
    {
        _damageAmount = damageAmount;
        _attackCooldown = attackCooldown;
        _attackVisualDistance = attackVisualDistance;
    }

    public bool CanAttack => Time.time >= _nextAttackTime;

    public void ResetCooldownAfterTrigger()
    {
        _nextAttackTime = Time.time + _attackCooldown;
    }

    public bool TryCreateDamage(Vector3 attackerPosition, Transform playerTransform, out DamageInfo damage)
    {
        damage = default;

        if (playerTransform == null)
            return false;

        float distance = Vector3.Distance(attackerPosition, playerTransform.position);
        if (distance > _attackVisualDistance)
            return false;

        damage = new DamageInfo(_damageAmount, DamageType.Physical);
        return true;
    }
}
