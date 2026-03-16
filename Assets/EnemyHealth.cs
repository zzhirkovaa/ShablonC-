using UnityEngine;
using System;
using UnityEngine.AI; // Добавил для чистоты кода с NavMeshAgent

public class EnemyHealth : MonoBehaviour, IDamageable, IHealth
{
    [Header("Settings")]
    public float maxHealth = 50f;
    private float currentHealth;
    private bool isDead = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public event Action<float> OnHealthChanged;

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        // Оповещаем UI о начальном здоровье
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    // ВАЖНО: Изменили float на DamageInfo
    public void TakeDamage(DamageInfo damage)
    {
        if (isDead) return;

        // Здесь ты можешь добавить логику: например, 
        // скелет получает меньше урона от магии, а маг от физики.
        currentHealth -= damage.Amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"{gameObject.name} получил {damage.Type} урон. Осталось ХП: {currentHealth}");

        // Обновляем полоску здоровья (Slider)
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
            animator.SetFloat("Speed", 0f);
            animator.SetTrigger("Die");
        }

        // Отключаем ИИ и навигацию
        // Попробуй заменить EnemyAI на EnemyRangedAI, если это маг
        if (TryGetComponent<EnemyRangedAI>(out var rangedAi)) rangedAi.enabled = false;

        if (TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (TryGetComponent<Collider>(out var col)) col.enabled = false;

        Destroy(gameObject, 3f);
    }
}