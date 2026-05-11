using UnityEngine;

public sealed class BossCombat : MonoBehaviour
{
    private enum PendingAttackKind
    {
        None,
        Normal,
        Strong
    }

    [Header("Damage")]
    [SerializeField] private float _damageAmount = 18f;
    [SerializeField] private float _strongAttackDamageMultiplier = 2.1f;
    [SerializeField] private float _attackVisualDistance = 3.2f;
    [SerializeField] private float _strongAttackVisualDistance = 4.5f;

    [Header("Cooldowns")]
    [SerializeField] private float _attackCooldown = 2.4f;
    [SerializeField] private float _strongAttackCooldown = 5.5f;

    private Transform _player;
    private float _attackCooldownRemaining;
    private float _strongAttackCooldownRemaining;
    private float _attackSpeedMultiplier = 1f;
    private PendingAttackKind _pendingAttack;

    public bool CanUseNormalAttack => _attackCooldownRemaining <= 0f;
    public bool CanUseStrongAttack => _strongAttackCooldownRemaining <= 0f;
    public float AttackCooldown => _attackCooldown;
    public float StrongAttackCooldown => _strongAttackCooldown;

    public void Construct(Transform playerTransform)
    {
        _player = playerTransform;
    }

    public void Tick(float deltaTime)
    {
        _attackCooldownRemaining = Mathf.Max(0f, _attackCooldownRemaining - deltaTime);
        _strongAttackCooldownRemaining = Mathf.Max(0f, _strongAttackCooldownRemaining - deltaTime);
    }

    public void SetAttackSpeedMultiplier(float multiplier)
    {
        _attackSpeedMultiplier = Mathf.Max(0.1f, multiplier);
    }

    public bool TryStartNormalAttack()
    {
        if (!CanUseNormalAttack)
            return false;

        _attackCooldownRemaining = _attackCooldown / _attackSpeedMultiplier;
        _pendingAttack = PendingAttackKind.Normal;
        return true;
    }

    public bool TryStartStrongAttack()
    {
        if (!CanUseStrongAttack)
            return false;

        _strongAttackCooldownRemaining = _strongAttackCooldown / _attackSpeedMultiplier;
        _pendingAttack = PendingAttackKind.Strong;
        return true;
    }

    public void CancelPendingAttack()
    {
        _pendingAttack = PendingAttackKind.None;
    }

    // Called by animation events. The active state decides which attack is pending.
    public void Attack()
    {
        if (_pendingAttack == PendingAttackKind.None || _player == null)
            return;

        float damageAmount = _damageAmount;
        float maxDistance = _attackVisualDistance;

        if (_pendingAttack == PendingAttackKind.Strong)
        {
            damageAmount *= _strongAttackDamageMultiplier;
            maxDistance = _strongAttackVisualDistance;
        }

        if (Vector3.Distance(transform.position, _player.position) <= maxDistance &&
            _player.TryGetComponent<IDamageable>(out var victim))
        {
            victim.TakeDamage(new DamageInfo(damageAmount, DamageType.Physical, gameObject));
        }

        _pendingAttack = PendingAttackKind.None;
    }
}
