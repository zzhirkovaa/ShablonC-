using UnityEngine;

public abstract class WeaponConfigSO : ScriptableObject
{
    [SerializeField] private string _weaponName = "Weapon";
    [SerializeField] private WeaponKind _weaponKind;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private float _attackCooldown = 1f;
    [SerializeField] private string _animationTrigger = "Attack";

    public string WeaponName => _weaponName;
    public WeaponKind WeaponKind => _weaponKind;
    public float Damage => _damage;
    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public string AnimationTrigger => _animationTrigger;
}
