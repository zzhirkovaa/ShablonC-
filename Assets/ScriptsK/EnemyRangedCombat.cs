using UnityEngine;

public class EnemyRangedCombat : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackCooldown = 3f;

    private float _nextAttackTime;
    private Transform _player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("player");
        if (p != null) _player = p.transform;
    }

    public bool CanAttack => Time.time >= _nextAttackTime;

    public void ResetCooldown() => _nextAttackTime = Time.time + attackCooldown;

    public void Shoot()
    {
        if (_player == null || projectilePrefab == null || firePoint == null) return;
        Vector3 lookAtTarget = new Vector3(_player.position.x, transform.position.y, _player.position.z);
        transform.LookAt(lookAtTarget);
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Vector3 aimPoint = _player.position + Vector3.up * 0.8f;
        Vector3 direction = (aimPoint - firePoint.position).normalized;
        projectile.transform.forward = direction;
    }
}