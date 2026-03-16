using UnityEngine;
using System;
using Player.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Настройки здоровья")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private bool isDead = false;

    private Animator _animator;

    public event Action<float, float> OnHealthChanged;
    public event Action OnPlayerDied;

    void Awake() 
    {
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(DamageInfo damage)
    {
        if (isDead) return;

        currentHealth -= damage.Amount;

        if (_animator != null)
        {
            _animator.SetTrigger("Hurt"); 
        }

        Debug.Log($"Получен {damage.Type} урон: {damage.Amount}. Осталось ХП: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Игрок погиб");

        if (_animator != null)
        {
            _animator.SetTrigger("Die");
        }

        if (TryGetComponent<PlayerController>(out var controller))
        {
            controller.enabled = false;
        }

        OnPlayerDied?.Invoke();
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
}