using UnityEngine;

public sealed class FireProjectile : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayers = ~0;
    [SerializeField] private float _lifetime = 6f;

    private Vector3 _direction = Vector3.forward;
    private float _speed;
    private float _directDamage;
    private float _burnDamagePerSecond;
    private float _burnDuration;
    private GameObject _source;
    private Rigidbody _rigidbody;
    private bool _initialized;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        Collider projectileCollider = GetComponent<Collider>();
        if (projectileCollider == null)
        {
            projectileCollider = gameObject.AddComponent<SphereCollider>();
            projectileCollider.isTrigger = true;
        }

        if (!projectileCollider.isTrigger)
            Debug.LogWarning($"[{name}] FireProjectile collider should have Is Trigger enabled.");
    }

    public void Initialize(
        Vector3 direction,
        float speed,
        float directDamage,
        float burnDamagePerSecond,
        float burnDuration,
        GameObject source)
    {
        _direction = direction == Vector3.zero ? transform.forward : direction.normalized;
        _speed = speed;
        _directDamage = directDamage;
        _burnDamagePerSecond = burnDamagePerSecond;
        _burnDuration = burnDuration;
        _source = source;
        _initialized = true;

        if (_rigidbody != null && !_rigidbody.isKinematic)
            _rigidbody.linearVelocity = _direction * _speed;

        Destroy(gameObject, Mathf.Max(0.1f, _lifetime));
    }

    private void Update()
    {
        if (!_initialized || (_rigidbody != null && !_rigidbody.isKinematic))
            return;

        transform.position += _direction * (_speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        if (_source != null && other.transform.root == _source.transform.root)
            return;

        if (((1 << other.gameObject.layer) & _targetLayers.value) == 0)
            return;

        IDamageable damageable = FindDamageable(other);
        if (damageable == null)
            return;

        damageable.TakeDamage(new DamageInfo(_directDamage, DamageType.Magical, _source));

        PlayerStatusEffects statusEffects = FindOrCreateStatusEffects(other);
        statusEffects?.ApplyBurnDamage(_burnDamagePerSecond, _burnDuration, _source);

        Destroy(gameObject);
    }

    private static IDamageable FindDamageable(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
            return damageable;

        foreach (MonoBehaviour behaviour in other.GetComponentsInParent<MonoBehaviour>())
        {
            if (behaviour is IDamageable parentDamageable)
                return parentDamageable;
        }

        return null;
    }

    private static PlayerStatusEffects FindOrCreateStatusEffects(Collider other)
    {
        PlayerStatusEffects statusEffects = other.GetComponentInParent<PlayerStatusEffects>();
        if (statusEffects != null)
            return statusEffects;

        PlayerController playerController = other.GetComponentInParent<PlayerController>();
        return playerController != null
            ? playerController.gameObject.AddComponent<PlayerStatusEffects>()
            : null;
    }
}
