using UnityEngine;
using System;
using UnityEngine.AI;

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

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    public void TakeDamage(DamageInfo damage)
    {
        if (isDead) return;

        currentHealth -= damage.Amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"{gameObject.name} получил {damage.Type} урон. Осталось ХП: {currentHealth}");

        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
            animator.SetFloat("Speed", 0f);
            animator.SetTrigger("Die");
        }

        if (TryGetComponent<EnemyRangedAI>(out var rangedAi))
            rangedAi.enabled = false;

        if (TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (TryGetComponent<Collider>(out var col))
            col.enabled = false;

        Destroy(gameObject, 3f);
    }
}