using UnityEngine;

public class PlayerRangedCombat : MonoBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _attackCooldown = 3f;
    [SerializeField] private AbilityCooldownUI _cooldownUI;

    private Animator _animator;
    private CooldownState _cooldownState;

    public float CurrentCooldown => _cooldownState.CurrentCooldown;
    public float MaxCooldown => _cooldownState.MaxCooldown;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _cooldownState = new CooldownState(_attackCooldown);
    }

    private void Update()
    {
        _cooldownState.Tick(Time.deltaTime);

        if (_cooldownUI != null)
            _cooldownUI.UpdateFill(_cooldownState.CurrentCooldown, _cooldownState.MaxCooldown);

        if (Input.GetMouseButtonDown(1) && _cooldownState.IsReady)
        {
            _animator.SetTrigger("Shoot");
            _cooldownState.Trigger();
        }
    }

    public void RestoreCooldown(float value)
    {
        _cooldownState.Restore(value);

        if (_cooldownUI != null)
            _cooldownUI.UpdateFill(_cooldownState.CurrentCooldown, _cooldownState.MaxCooldown);
    }

    public void LaunchProjectile()
    {
        if (_projectilePrefab != null && _firePoint != null)
        {
            GameObject projectileObject = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);
            if (projectileObject.TryGetComponent<Projectile>(out var projectile))
                projectile.owner = gameObject;
        }
    }
}
