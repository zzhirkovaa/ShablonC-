using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerRangedCombat))]
public class PlayerStatsProvider : MonoBehaviour, IPlayerStatsProvider
{
    private PlayerHealth _health;
    private PlayerRangedCombat _rangedCombat;

    public float CurrentHp => _health.CurrentHealth;
    public float MaxHp => _health.MaxHealth;

    public float CurrentAbilityCooldown => _rangedCombat.CurrentCooldown;

    private void Awake()
    {
        _health = GetComponent<PlayerHealth>();
        _rangedCombat = GetComponent<PlayerRangedCombat>();
    }

    public void RestoreHp(float value)
    {
        _health.RestoreHealth(value);
    }

    public void RestoreAbilityCooldown(float value)
    {
        _rangedCombat.RestoreCooldown(value);
    }
}