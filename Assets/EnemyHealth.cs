using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Settings")]
    public float maxHealth = 50f;
    private float currentHealth;
    private bool isDead = false;

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log(gameObject.name + " получил урон. ХП: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
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

        if (TryGetComponent<EnemyAI>(out var ai)) ai.enabled = false;
        if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
        {
            agent.isStopped = true;
            agent.enabled = false; 
        }

        if (TryGetComponent<Collider>(out var col)) col.enabled = false;

        Destroy(gameObject, 3f);
    }
}