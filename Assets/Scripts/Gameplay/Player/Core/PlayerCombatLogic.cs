using UnityEngine;

public sealed class PlayerCombatLogic
{
    private readonly float _attackDamage;
    private readonly float _attackRange;
    private readonly LayerMask _enemyLayers;

    public PlayerCombatLogic(float attackDamage, float attackRange, LayerMask enemyLayers)
    {
        _attackDamage = attackDamage;
        _attackRange = attackRange;
        _enemyLayers = enemyLayers;
    }

    public void PerformAttack(Transform attackPoint, DamageType damageType)
    {
        if (attackPoint == null)
            return;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, _attackRange, _enemyLayers);
        DamageInfo info = new DamageInfo(_attackDamage, damageType);

        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<IDamageable>(out var victim))
                victim.TakeDamage(info);
        }
    }
}
