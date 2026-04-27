using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour, IDamageable, IPlayerHealthModel
{
    [Header("Настройки здоровья")]
    [SerializeField] private float maxHealth = 100f;

    private Animator _animator;
    private PlayerHealthState _state;

    public float CurrentHealth => _state.CurrentHealth;
    public float MaxHealth => maxHealth;

    public event Action<float, float> OnHealthChanged;
    public event Action OnPlayerDied;

    event Action<float, float> IPlayerHealthModel.HealthChanged
    {
        add => OnHealthChanged += value;
        remove => OnHealthChanged -= value;
    }

    event Action IPlayerHealthModel.Died
    {
        add => OnPlayerDied += value;
        remove => OnPlayerDied -= value;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _state = new PlayerHealthState(maxHealth);
        _state.HealthChanged += RaiseHealthChanged;
        _state.Died += OnDiedInternal;
    }

    private void Start()
    {
        _state.PublishCurrentState();
    }

    private void OnDestroy()
    {
        if (_state == null)
            return;

        _state.HealthChanged -= RaiseHealthChanged;
        _state.Died -= OnDiedInternal;
    }

    public void TakeDamage(DamageInfo damage)
    {
        if (_state.IsDead)
            return;

        if (_animator != null)
            _animator.SetTrigger("Hurt");

        _state.ApplyDamage(damage.Amount);
    }

    public void RestoreHealth(float value)
    {
        _state.RestoreHealth(value);
    }

    private void RaiseHealthChanged(float currentHealth, float maxHealthValue)
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealthValue);
    }

    private void OnDiedInternal()
    {
        if (_animator != null)
            _animator.SetTrigger("Die");

        if (TryGetComponent<PlayerController>(out var playerController))
            playerController.enabled = false;

        OnPlayerDied?.Invoke();
    }
}
