using UnityEngine;

public sealed class PlayerCombatLogic
{
    private readonly float _attackDamage;
    private readonly float _attackRange;
    private readonly LayerMask _enemyLayers;
    private readonly GameObject _damageSource;

    public PlayerCombatLogic(float attackDamage, float attackRange, LayerMask enemyLayers, GameObject damageSource)
    {
        _attackDamage = attackDamage;
        _attackRange = attackRange;
        _enemyLayers = enemyLayers;
        _damageSource = damageSource;
    }

    public void PerformAttack(Transform attackPoint, DamageType damageType)
    {
        if (attackPoint == null)
            return;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, _attackRange, _enemyLayers);
        DamageInfo info = new DamageInfo(_attackDamage, damageType, _damageSource);

        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<IDamageable>(out var victim))
            {
                victim.TakeDamage(info);
                continue;
            }

            foreach (MonoBehaviour behaviour in enemy.GetComponentsInParent<MonoBehaviour>())
            {
                if (behaviour is IDamageable parentVictim)
                {
                    parentVictim.TakeDamage(info);
                    break;
                }
            }
        }
    }
}
