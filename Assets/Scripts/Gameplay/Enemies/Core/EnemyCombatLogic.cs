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

    public bool CanAttack(float currentTime) => currentTime >= _nextAttackTime;

    public void ResetCooldownAfterTrigger(float currentTime)
    {
        _nextAttackTime = currentTime + _attackCooldown;
    }

    public bool TryCreateDamage(Vector3 attackerPosition, Vector3 targetPosition, out DamageInfo damage)
    {
        damage = default;

        float distance = Vector3.Distance(attackerPosition, targetPosition);
        if (distance > _attackVisualDistance)
            return false;

        damage = new DamageInfo(_damageAmount, DamageType.Physical);
        return true;
    }
}
