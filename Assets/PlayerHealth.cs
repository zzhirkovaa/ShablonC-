using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float health = 100f;

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log("Нанесен урон. ост хп " + health);

        if (health <= 0)
        {
            Debug.Log("Смерть");
        }
    }
}