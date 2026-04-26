using System;
using UnityEngine;

public sealed class EnemyHealthState
{
    private readonly float _maxHealth;
    private float _currentHealth;
    private bool _isDead;

    public EnemyHealthState(float maxHealth)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
    }

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public bool IsDead => _isDead;

    public event Action<float> HealthChanged;
    public event Action Died;

    public void PublishCurrentState()
    {
        HealthChanged?.Invoke(_currentHealth / _maxHealth);
    }

    public bool ApplyDamage(float amount)
    {
        if (_isDead)
            return false;

        _currentHealth -= amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth);
        HealthChanged?.Invoke(_currentHealth / _maxHealth);

        if (_currentHealth <= 0f)
        {
            _isDead = true;
            Died?.Invoke();
        }

        return true;
    }

    public void Heal(float amount)
    {
        if (_isDead)
            return;

        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0f, _maxHealth);
        HealthChanged?.Invoke(_currentHealth / _maxHealth);
    }

    public void RestoreToFull()
    {
        if (_isDead)
            return;

        _currentHealth = _maxHealth;
        HealthChanged?.Invoke(_currentHealth / _maxHealth);
    }

    public void Restore(float currentHealth, bool isDead)
    {
        _currentHealth = Mathf.Clamp(currentHealth, 0f, _maxHealth);
        _isDead = isDead || _currentHealth <= 0f;
        HealthChanged?.Invoke(_currentHealth / _maxHealth);
    }
}
