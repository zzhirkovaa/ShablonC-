using UnityEngine;

[CreateAssetMenu(
    fileName = "BossElementConfig",
    menuName = "Game/Boss/Element Config")]
public sealed class BossElementConfig : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private BossElementType _elementType;
    [SerializeField] private bool _usesMeleeHitboxes = true;

    [Header("Animation")]
    [SerializeField] private string _kickTriggerName;
    [SerializeField] private string _heavyHandsTriggerName;
    [SerializeField] private float _kickDurationOverride;
    [SerializeField] private float _heavyHandsDurationOverride;

    [Header("Fire")]
    [SerializeField] private GameObject _fireProjectilePrefab;
    [SerializeField] private float _fireProjectileSpeed = 12f;
    [SerializeField] private float _fireDirectDamage = 10f;
    [SerializeField] private float _fireBurnDamagePerSecond = 5f;
    [SerializeField] private float _fireBurnDuration = 3f;

    [Header("Earth")]
    [SerializeField] private GameObject _earthEffectPrefab;
    [SerializeField] private float _earthRadius = 5f;
    [SerializeField] private float _earthKnockUpHeight = 4f;
    [SerializeField, Tooltip("Legacy option. Earth attack now knocks the player up without stunning them.")]
    private float _earthControlDisableDuration = 0f;

    [Header("Air")]
    [SerializeField] private GameObject _airEffectPrefab;
    [SerializeField] private GameObject _airProjectilePrefab;
    [SerializeField] private float _airProjectileSpeed = 12f;
    [SerializeField] private float _airSpinDuration = 1.25f;
    [SerializeField] private float _airSpinDegrees = 720f;
    [SerializeField, Tooltip("Legacy option. Air attack now uses a projectile instead of pull.")]
    private float _airDistance = 8f;
    [SerializeField, Tooltip("Legacy option. Air attack now uses a projectile instead of pull.")]
    private float _airForwardDotThreshold = 0.35f;
    [SerializeField, Tooltip("Legacy option. Air attack now uses a projectile instead of pull.")]
    private float _airPullDuration = 1.25f;
    [SerializeField, Tooltip("Legacy option. Air attack now uses a projectile instead of pull.")]
    private float _airPullSpeed = 8f;
    [SerializeField, Tooltip("Legacy option. Air attack now uses a projectile instead of pull.")]
    private float _airStopDistance = 2.25f;

    [Header("Ice")]
    [SerializeField] private GameObject _iceLeftHandEffectPrefab;
    [SerializeField] private GameObject _iceRightHandEffectPrefab;
    [SerializeField] private float _iceFreezeDuration = 5f;

    [Header("Visual Lifetime")]
    [SerializeField] private float _effectLifetime = 2f;

    public BossElementType ElementType => _elementType;
    public bool UsesMeleeHitboxes => _usesMeleeHitboxes;
    public GameObject FireProjectilePrefab => _fireProjectilePrefab;
    public float FireProjectileSpeed => _fireProjectileSpeed;
    public float FireDirectDamage => _fireDirectDamage;
    public float FireBurnDamagePerSecond => _fireBurnDamagePerSecond;
    public float FireBurnDuration => _fireBurnDuration;
    public GameObject EarthEffectPrefab => _earthEffectPrefab;
    public float EarthRadius => _earthRadius;
    public float EarthKnockUpHeight => _earthKnockUpHeight;
    public float EarthControlDisableDuration => _earthControlDisableDuration;
    public GameObject AirEffectPrefab => _airEffectPrefab;
    public GameObject AirProjectilePrefab => _airProjectilePrefab != null ? _airProjectilePrefab : _airEffectPrefab;
    public float AirProjectileSpeed => _airProjectileSpeed;
    public float AirSpinDuration => _airSpinDuration;
    public float AirSpinDegrees => _airSpinDegrees;
    public float AirDistance => _airDistance;
    public float AirForwardDotThreshold => _airForwardDotThreshold;
    public float AirPullDuration => _airPullDuration;
    public float AirPullSpeed => _airPullSpeed;
    public float AirStopDistance => _airStopDistance;
    public GameObject IceLeftHandEffectPrefab => _iceLeftHandEffectPrefab;
    public GameObject IceRightHandEffectPrefab => _iceRightHandEffectPrefab;
    public float IceFreezeDuration => _iceFreezeDuration;
    public float EffectLifetime => _effectLifetime;

    public string GetTriggerName(BossAttackType attackType)
    {
        return attackType == BossAttackType.HeavyHands
            ? _heavyHandsTriggerName
            : _kickTriggerName;
    }

    public float GetDurationOverride(BossAttackType attackType)
    {
        return attackType == BossAttackType.HeavyHands
            ? _heavyHandsDurationOverride
            : _kickDurationOverride;
    }
}
