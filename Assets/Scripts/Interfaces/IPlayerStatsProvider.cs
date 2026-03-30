public interface IPlayerStatsProvider
{
    float CurrentHp { get; }
    float MaxHp { get; }

    float CurrentAbilityCooldown { get; }

    void RestoreHp(float value);
    void RestoreAbilityCooldown(float value);
}