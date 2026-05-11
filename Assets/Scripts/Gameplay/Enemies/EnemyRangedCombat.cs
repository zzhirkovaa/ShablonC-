using UnityEngine;

public class EnemyRangedCombat : MonoBehaviour
{
    [Header("РќР°СЃС‚СЂРѕР№РєРё СЃС‚СЂРµР»СЊР±С‹")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackCooldown = 3f;
    public float projectileDamage = 12f;
    public float projectileSpeed = 15f;
    public float projectileLifetime = 5f;

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
        if (_player == null || projectilePrefab == null || firePoint == null)
            return;

        GameObject projectileObject = _attackLogic.ShootAtTarget(transform, _player, projectilePrefab, firePoint);
        if (projectileObject == null)
            return;

        Projectile projectile = EnsureProjectileRuntimeSetup(projectileObject);
        projectile.owner = gameObject;
        projectile.damageAmount = projectileDamage;
        projectile.speed = projectileSpeed;
        projectile.lifetime = projectileLifetime;
    }

    public void ApplyWeaponConfig(RangedWeaponConfigSO weaponConfig)
    {
        if (weaponConfig == null)
            return;

        projectilePrefab = weaponConfig.ProjectilePrefab;
        attackCooldown = weaponConfig.AttackCooldown;
        projectileDamage = weaponConfig.Damage;
        projectileSpeed = weaponConfig.ProjectileSpeed;
        projectileLifetime = weaponConfig.ProjectileLifetime;

        Transform resolvedFirePoint = TransformSearchUtility.FindChildRecursive(transform, weaponConfig.FirePointName);
        if (resolvedFirePoint != null)
            firePoint = resolvedFirePoint;

        _cooldownState = new CooldownState(attackCooldown);
    }

    private Projectile EnsureProjectileRuntimeSetup(GameObject projectileObject)
    {
        Projectile projectile =
            projectileObject.GetComponent<Projectile>() ??
            projectileObject.AddComponent<Projectile>();

        Collider projectileCollider = projectileObject.GetComponent<Collider>();
        if (projectileCollider == null)
            projectileCollider = projectileObject.AddComponent<SphereCollider>();

        projectileCollider.isTrigger = true;

        Rigidbody projectileBody = projectileObject.GetComponent<Rigidbody>();
        if (projectileBody == null)
            projectileBody = projectileObject.AddComponent<Rigidbody>();

        projectileBody.useGravity = false;
        projectileBody.isKinematic = true;
        projectileBody.linearVelocity = Vector3.zero;
        projectileBody.angularVelocity = Vector3.zero;

        return projectile;
    }
}
