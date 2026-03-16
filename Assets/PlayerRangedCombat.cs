using UnityEngine;

public class PlayerRangedCombat : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _attackCooldown = 3f;

    private float _nextAttackTime = 0f;
    private Animator _animator;

    private void Awake() => _animator = GetComponent<Animator>();

    private void Update()
    {
        // Правая кнопка мыши (1)
        if (Input.GetMouseButtonDown(1) && Time.time >= _nextAttackTime)
        {
            _animator.SetTrigger("Shoot");
            _nextAttackTime = Time.time + _attackCooldown;
        }
    }

    // Вызывается через Animation Event
    public void LaunchProjectile()
    {
        if (_projectilePrefab == null || _firePoint == null) return;

        GameObject projGO = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);

        if (projGO.TryGetComponent<Projectile>(out var proj))
        {
            proj.owner = this.gameObject; // Передаем игрока как владельца
        }
    }
}