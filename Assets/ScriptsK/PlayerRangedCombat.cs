using UnityEngine;

public class PlayerRangedCombat : MonoBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _attackCooldown = 3f;
    [SerializeField] private AbilityCooldownUI _cooldownUI;

    private float _cooldownTimer = 0f;
    private Animator _animator;

    private void Awake() => _animator = GetComponent<Animator>();

    private void Update()
    {
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }

        if (_cooldownUI != null)
        {
            _cooldownUI.UpdateFill(_cooldownTimer, _attackCooldown);
        }

        if (Input.GetMouseButtonDown(1) && _cooldownTimer <= 0)
        {
            _animator.SetTrigger("Shoot");
            _cooldownTimer = _attackCooldown;
        }
    }

    public void LaunchProjectile() // Animation Event
    {
        if (_projectilePrefab && _firePoint)
        {
            Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);
        }
    }
}