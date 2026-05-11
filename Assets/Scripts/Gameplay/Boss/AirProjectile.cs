using UnityEngine;

public sealed class AirProjectile : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayers = ~0;
    [SerializeField] private float _lifetime = 6f;

    private Vector3 _direction = Vector3.forward;
    private float _speed;
    private float _spinDuration;
    private float _spinDegrees;
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
            Debug.LogWarning($"[{name}] AirProjectile collider should have Is Trigger enabled.");
    }

    public void Initialize(
        Vector3 direction,
        float speed,
        float spinDuration,
        float spinDegrees,
        GameObject source)
    {
        _direction = direction == Vector3.zero ? transform.forward : direction.normalized;
        _speed = speed;
        _spinDuration = spinDuration;
        _spinDegrees = spinDegrees;
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

        PlayerStatusEffects statusEffects = FindOrCreateStatusEffects(other);
        if (statusEffects == null)
            return;

        statusEffects.SpinAroundY(_spinDuration, _spinDegrees);
        Destroy(gameObject);
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
