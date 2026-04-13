using UnityEngine;

public sealed class PlayerSaveStateWriter : IPlayerSaveStateWriter
{
    private readonly Transform _playerTransform;
    private readonly CharacterController _characterController;
    private readonly IPlayerStatsProvider _playerStatsProvider;

    public PlayerSaveStateWriter(
        Transform playerTransform,
        CharacterController characterController,
        IPlayerStatsProvider playerStatsProvider)
    {
        _playerTransform = playerTransform;
        _characterController = characterController;
        _playerStatsProvider = playerStatsProvider;
    }

    public void Apply(PlayerDataModel playerData)
    {
        if (playerData == null)
            return;

        if (_characterController != null)
            _characterController.enabled = false;

        _playerTransform.position = new Vector3(
            playerData.PositionX,
            playerData.PositionY,
            playerData.PositionZ);

        if (_characterController != null)
            _characterController.enabled = true;

        _playerStatsProvider.RestoreHp(playerData.CurrentHp);
        _playerStatsProvider.RestoreAbilityCooldown(playerData.AbilityCooldownRemaining);
    }
}
