using UnityEngine;
using System;
using Player.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Настройки здоровья")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private bool isDead = false;

    public event Action<float, float> OnHealthChanged;
    public event Action OnPlayerDied;

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
        isDead = true;
        Debug.Log("Игрок погиб");
        OnPlayerDied?.Invoke();

        var deathScreen = FindObjectOfType<DeathScreenView>();
        deathScreen?.ShowDeathScreen();
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
}