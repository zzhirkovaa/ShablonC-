using UnityEngine;

public sealed class PlayerSaveStateReader : IPlayerSaveStateReader
{
    private readonly Transform _playerTransform;
    private readonly IPlayerStatsProvider _playerStatsProvider;

    public PlayerSaveStateReader(Transform playerTransform, IPlayerStatsProvider playerStatsProvider)
    {
        _playerTransform = playerTransform;
        _playerStatsProvider = playerStatsProvider;
    }

    public PlayerDataModel Read()
    {
        return new PlayerDataModel
        {
            PositionX = _playerTransform.position.x,
            PositionY = _playerTransform.position.y,
            PositionZ = _playerTransform.position.z,
            CurrentHp = _playerStatsProvider.CurrentHp,
            AbilityCooldownRemaining = _playerStatsProvider.CurrentAbilityCooldown
        };
    }
}
