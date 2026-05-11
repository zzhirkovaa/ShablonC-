using System;

public interface IPlayerHealthModel
{
    float CurrentHealth { get; }
    float MaxHealth { get; }

    event Action<float, float> HealthChanged;
    event Action Died;
}