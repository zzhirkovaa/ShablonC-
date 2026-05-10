using UnityEngine;

[CreateAssetMenu(
    fileName = "RangedWeaponConfig",
    menuName = "Game/Weapons/Ranged Weapon Config")]
public sealed class RangedWeaponConfigSO : WeaponConfigSO
{
    [Header("Projectile")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileSpeed = 15f;
    [SerializeField] private float _projectileLifetime = 5f;
    [SerializeField] private string _firePointName = "spawnPRJ";

    public GameObject ProjectilePrefab => _projectilePrefab;
    public float ProjectileSpeed => _projectileSpeed;
    public float ProjectileLifetime => _projectileLifetime;
    public string FirePointName => _firePointName;
}
