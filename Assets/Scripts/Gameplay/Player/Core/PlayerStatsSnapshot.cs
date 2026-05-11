public sealed class PlayerStatsSnapshot
{
    public PlayerStatsSnapshot(PlayerHealth playerHealth, PlayerRangedCombat playerRangedCombat)
    {
        PlayerHealth = playerHealth;
        PlayerRangedCombat = playerRangedCombat;
    }

    public PlayerHealth PlayerHealth { get; }
    public PlayerRangedCombat PlayerRangedCombat { get; }

    public float CurrentHp => PlayerHealth.CurrentHealth;
    public float MaxHp => PlayerHealth.MaxHealth;
    public float CurrentAbilityCooldown => PlayerRangedCombat.CurrentCooldown;

    public void RestoreHp(float value)
    {
        PlayerHealth.RestoreHealth(value);
    }

    public void RestoreAbilityCooldown(float value)
    {
        PlayerRangedCombat.RestoreCooldown(value);
    }
}
