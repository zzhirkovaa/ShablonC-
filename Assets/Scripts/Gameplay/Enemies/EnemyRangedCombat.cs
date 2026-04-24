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

    public void Construct(Transform playerTransform)
    {
        _player = playerTransform;
    }

    private void Start()
    {
        if (_player != null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("player");
        if (playerObject != null)
            _player = playerObject.transform;
    }

    private void Update()
    {
        _cooldownState.Tick(Time.deltaTime);
    }

    public bool CanAttack => _cooldownState.IsReady;

    public void ResetCooldown() => _cooldownState.Trigger();

    public void Shoot()
    {
        if (_player == null)
            return;

        _attackLogic.ShootAtTarget(transform, _player, projectilePrefab, firePoint);
    }
}
