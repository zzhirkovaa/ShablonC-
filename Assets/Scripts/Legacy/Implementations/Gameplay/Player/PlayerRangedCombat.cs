using UnityEngine;

public class PlayerRangedCombat : MonoBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _attackCooldown = 3f;
    [SerializeField] private AbilityCooldownUI _cooldownUI;

    private float _cooldownTimer = 0f;
    private Animator _animator;

    public float CurrentCooldown => _cooldownTimer;
    public float MaxCooldown => _attackCooldown;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;

            if (_cooldownTimer < 0f)
                _cooldownTimer = 0f;
        }

        if (_cooldownUI != null)
        {
            _cooldownUI.UpdateFill(_cooldownTimer, _attackCooldown);
        }

        if (Input.GetMouseButtonDown(1) && _cooldownTimer <= 0f)
        {
            _animator.SetTrigger("Shoot");
            _cooldownTimer = _attackCooldown;
        }
    }

    public void RestoreCooldown(float value)
    {
        _cooldownTimer = Mathf.Clamp(value, 0f, _attackCooldown);

        if (_cooldownUI != null)
        {
            _cooldownUI.UpdateFill(_cooldownTimer, _attackCooldown);
        }
    }

    public void LaunchProjectile()
    {
        if (_projectilePrefab != null && _firePoint != null)
        {
            Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);
        }
    }
}