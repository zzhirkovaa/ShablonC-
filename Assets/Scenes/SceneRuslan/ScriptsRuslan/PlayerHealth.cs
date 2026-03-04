using UnityEngine;
using System;
using Player.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Настройки здоровья")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private bool isDead = false;  // Флаг, что игрок мертв

    // События для UI
    public event Action<float, float> OnHealthChanged;
    public event Action OnPlayerDied;

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log("Нанесен урон. ост хп " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;

            Debug.Log("Смерть");
            OnPlayerDied?.Invoke();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // ВРЕМЕННО: принудительно ищем DeathScreen
            var deathScreen = FindObjectOfType<DeathScreenView>();
            if (deathScreen != null)
            {
                Debug.Log("Нашел DeathScreen, вызываю вручную");
                deathScreen.ShowDeathScreen();
            }
            else
            {
                Debug.LogError("DeathScreenView НЕ НАЙДЕН на сцене!");
            }
        }
        else
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    // Методы для UI
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
}