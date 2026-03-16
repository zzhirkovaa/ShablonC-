using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public float damageAmount = 10f;
    public float attackCooldown = 5f;
    public float attackVisualDistance = 3.5f;

    private float _nextAttackTime;
    private Transform _player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("player");
        if (p != null) _player = p.transform;
    }

    public bool CanAttack => Time.time >= _nextAttackTime;

    public void ResetCooldownAfterTrigger() => _nextAttackTime = Time.time + attackCooldown;

    public void Attack()
    {
        if (_player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist <= attackVisualDistance)
        {
            if (_player.TryGetComponent<IDamageable>(out var victim))
            {
                DamageInfo info = new DamageInfo(damageAmount, DamageType.Physical);
                victim.TakeDamage(info);

                Debug.Log("Враг нанес физический урон!");
            }
        }
        else
        {
            Debug.Log("Враг промахнулся");
        }
    }
}