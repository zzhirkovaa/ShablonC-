using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Settings")]
    public float attackDamage = 20f;
    public float attackRange = 2f;
    public Transform attackPoint;
    public LayerMask enemyLayers;

    public void Hit()
    {
        PerformAttack(DamageType.Physical);
    }

    public void MagicHit()
    {
        PerformAttack(DamageType.Magical);
    }

    private void PerformAttack(DamageType type)
    {
        if (attackPoint == null) return;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        DamageInfo info = new DamageInfo(attackDamage, type);

        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<IDamageable>(out var victim))
            {
                victim.TakeDamage(info);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}