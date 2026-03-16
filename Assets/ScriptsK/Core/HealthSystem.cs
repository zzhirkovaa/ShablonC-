using System;
using UnityEngine;

namespace Player.Core
{
    public class HealthSystem
    {
        private float _currentHealth;
        private readonly float _maxHealth;

        public event Action<float, float> OnHealthChanged; 
        public event Action OnDied;

        public HealthSystem(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (_currentHealth <= 0) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (_currentHealth <= 0)
            {
                OnDied?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (_currentHealth <= 0) return;

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public float GetHealth() => _currentHealth;
        public float GetMaxHealth() => _maxHealth;
        public bool IsAlive => _currentHealth > 0;
    }
}