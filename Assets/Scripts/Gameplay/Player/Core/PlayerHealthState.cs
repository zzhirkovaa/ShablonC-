using System;
using UnityEngine;

public sealed class PlayerHealthState
{
    private readonly float _maxHealth;
    private float _currentHealth;
    private bool _isDead;

    public PlayerHealthState(float maxHealth)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
    }

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public bool IsDead => _isDead;

    public event Action<float, float> HealthChanged;
    public event Action Died;

    public void PublishCurrentState()
    {
        HealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public bool ApplyDamage(float amount)
    {
        if (_isDead)
            return false;

        _currentHealth -= amount;
        if (_currentHealth <= 0f)
        {
            _currentHealth = 0f;
            _isDead = true;
        }

        HealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_isDead)
            Died?.Invoke();

        return true;
    }

    public void RestoreHealth(float value)
    {
        _currentHealth = Mathf.Clamp(value, 0f, _maxHealth);
        if (_currentHealth > 0f)
            _isDead = false;

        HealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
}
