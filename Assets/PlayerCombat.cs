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
        if (attackPoint == null) return;
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<IDamageable>(out var victim))
            {
                victim.TakeDamage(attackDamage);
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