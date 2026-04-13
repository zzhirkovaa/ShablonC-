using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerRangedCombat))]
public class PlayerStatsProvider : MonoBehaviour, IPlayerStatsProvider
{
    private PlayerStatsSnapshot _snapshot;

    public float CurrentHp => _snapshot.CurrentHp;
    public float MaxHp => _snapshot.MaxHp;

    public float CurrentAbilityCooldown => _snapshot.CurrentAbilityCooldown;

    private void Awake()
    {
        _snapshot = new PlayerStatsSnapshot(
            GetComponent<PlayerHealth>(),
            GetComponent<PlayerRangedCombat>());
    }

    public void RestoreHp(float value)
    {
        _snapshot.RestoreHp(value);
    }

    public void RestoreAbilityCooldown(float value)
    {
        _snapshot.RestoreAbilityCooldown(value);
    }
}
