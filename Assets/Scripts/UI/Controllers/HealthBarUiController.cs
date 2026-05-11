using System;
using Player.Interfaces;

public sealed class HealthBarUiController : IDisposable
{
    private readonly IPlayerHealthModel _healthModel;
    private readonly IHealthBar _view;

    public HealthBarUiController(IPlayerHealthModel healthModel, IHealthBar view)
    {
        _healthModel = healthModel;
        _view = view;

        _healthModel.HealthChanged += OnHealthChanged;

        _view.Show();
        _view.UpdateHealth(_healthModel.CurrentHealth, _healthModel.MaxHealth);
    }

    public void Dispose()
    {
        _healthModel.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        _view.UpdateHealth(currentHealth, maxHealth);
    }
}