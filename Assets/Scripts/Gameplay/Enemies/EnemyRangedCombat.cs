using UnityEngine;

public class EnemyRangedCombat : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackCooldown = 3f;

    private Transform _player;
    private CooldownState _cooldownState;
    private RangedProjectileAttackLogic _attackLogic;

    private void Awake()
    {
        _cooldownState = new CooldownState(attackCooldown);
        _attackLogic = new RangedProjectileAttackLogic();
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("player");
        if (p != null) _player = p.transform;
    }

    private void Update()
    {
        _cooldownState.Tick(Time.deltaTime);
    }

    public bool CanAttack => _cooldownState.IsReady;

    public void ResetCooldown() => _cooldownState.Trigger();

    public void Shoot()
    {
        _attackLogic.ShootAtTarget(transform, _player, projectilePrefab, firePoint);
    }
}
