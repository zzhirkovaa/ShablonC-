using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour, IDamageable, IPlayerHealthModel
{
    [Header("Настройки здоровья")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private bool isDead;

    private Animator _animator;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public event Action<float, float> OnHealthChanged;
    public event Action OnPlayerDied;

    event Action<float, float> IPlayerHealthModel.HealthChanged
    {
        add => OnHealthChanged += value;
        remove => OnHealthChanged -= value;
    }

    event Action IPlayerHealthModel.Died
    {
        add => OnPlayerDied += value;
        remove => OnPlayerDied -= value;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        isDead = false;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(DamageInfo damage)
    {
        if (isDead) return;

        currentHealth -= damage.Amount;

        if (_animator != null)
            _animator.SetTrigger("Hurt");

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (_animator != null)
            _animator.SetTrigger("Die");

        if (TryGetComponent<PlayerController>(out var controller))
            controller.enabled = false;

        OnPlayerDied?.Invoke();
    }
}