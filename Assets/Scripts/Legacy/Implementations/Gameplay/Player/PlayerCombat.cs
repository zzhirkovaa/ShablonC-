using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Settings")]
    public float attackDamage = 20f;
    public float attackRange = 2f;
    public Transform attackPoint;
    public LayerMask enemyLayers;

    private PlayerCombatLogic _combatLogic;

    private void Awake()
    {
        _combatLogic = new PlayerCombatLogic(attackDamage, attackRange, enemyLayers);
    }

    public void Hit()
    {
        _combatLogic.PerformAttack(attackPoint, DamageType.Physical);
    }

    public void MagicHit()
    {
        _combatLogic.PerformAttack(attackPoint, DamageType.Magical);
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
