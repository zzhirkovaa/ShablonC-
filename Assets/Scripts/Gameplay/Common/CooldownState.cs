using UnityEngine;

public sealed class CooldownState
{
    private readonly float _maxCooldown;
    private float _currentCooldown;

    public CooldownState(float maxCooldown)
    {
        _maxCooldown = maxCooldown;
    }

    public float CurrentCooldown => _currentCooldown;
    public float MaxCooldown => _maxCooldown;
    public bool IsReady => _currentCooldown <= 0f;

    public void Tick(float deltaTime)
    {
        if (_currentCooldown <= 0f)
            return;

        _currentCooldown -= deltaTime;
        if (_currentCooldown < 0f)
            _currentCooldown = 0f;
    }

    public void Trigger()
    {
        _currentCooldown = _maxCooldown;
    }

    public void Restore(float value)
    {
        _currentCooldown = Mathf.Clamp(value, 0f, _maxCooldown);
    }
}
