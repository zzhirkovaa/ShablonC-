using System.Collections.Generic;
using UnityEngine;

public sealed class BossDamageHitbox : MonoBehaviour
{
    [SerializeField] private Collider _triggerCollider;
    [SerializeField] private LayerMask _targetLayers = ~0;
    [SerializeField] private DamageType _damageType = DamageType.Physical;
    [SerializeField] private bool _activeOnStart;

    private readonly HashSet<IDamageable> _damagedTargets = new HashSet<IDamageable>();
    private bool _isActive;
    private float _damageAmount;

    private void Awake()
    {
        if (_triggerCollider == null)
            _triggerCollider = GetComponent<Collider>();

        if (_triggerCollider == null)
        {
            Debug.LogWarning($"[{name}] BossDamageHitbox requires a trigger Collider.");
            return;
        }

        if (!_triggerCollider.isTrigger)
            Debug.LogWarning($"[{name}] BossDamageHitbox Collider should have Is Trigger enabled.");

        SetActive(_activeOnStart);
    }

    public void Configure(float damageAmount, DamageType damageType)
    {
        _damageAmount = damageAmount;
        _damageType = damageType;
    }

    public void SetActive(bool isActive)
    {
        _isActive = isActive;

        if (isActive)
            _damagedTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider other)
    {
        if (!_isActive || other == null)
            return;

        if (((1 << other.gameObject.layer) & _targetLayers.value) == 0)
            return;

        if (other.transform.root == transform.root)
            return;

        IDamageable victim = FindDamageable(other);
        if (victim == null || _damagedTargets.Contains(victim))
            return;

        _damagedTargets.Add(victim);
        victim.TakeDamage(new DamageInfo(_damageAmount, _damageType));
    }

    private IDamageable FindDamageable(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var directDamageable))
            return directDamageable;

        foreach (MonoBehaviour behaviour in other.GetComponentsInParent<MonoBehaviour>())
        {
            if (behaviour is IDamageable damageable)
                return damageable;
        }

        return null;
    }
}
