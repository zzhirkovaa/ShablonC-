public sealed class JsonPlayerRepository : IPlayerRepository
{
    private readonly ISaveService _saveService;

    public JsonPlayerRepository(ISaveService saveService)
    {
        _saveService = saveService;
    }

    public PlayerDataModel Load()
    {
        if (!_saveService.HasSave())
            return null;

        SaveData data = _saveService.LoadGame();
        if (data == null)
            return null;

        return new PlayerDataModel
        {
            PositionX = data.PlayerPosX,
            PositionY = data.PlayerPosY,
            PositionZ = data.PlayerPosZ,
            CurrentHp = data.PlayerHp,
            AbilityCooldownRemaining = data.PlayerAbilityCooldownRemaining
        };
    }

    public void Save(PlayerDataModel playerData)
    {
        SaveData data = LoadOrCreate();
        data.PlayerPosX = playerData.PositionX;
        data.PlayerPosY = playerData.PositionY;
        data.PlayerPosZ = playerData.PositionZ;
        data.PlayerHp = playerData.CurrentHp;
        data.PlayerAbilityCooldownRemaining = playerData.AbilityCooldownRemaining;

        _saveService.SaveGame(data);
    }

    private SaveData LoadOrCreate()
    {
        return _saveService.HasSave() ? _saveService.LoadGame() ?? new SaveData() : new SaveData();
    }
}
