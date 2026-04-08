using System.Collections.Generic;

public sealed class JsonEnemyRepository : IEnemyRepository
{
    private readonly ISaveService _saveService;

    public JsonEnemyRepository(ISaveService saveService)
    {
        _saveService = saveService;
    }

    public IReadOnlyList<EnemyDataModel> Load()
    {
        if (!_saveService.HasSave())
            return new List<EnemyDataModel>();

        SaveData data = _saveService.LoadGame();
        List<EnemyDataModel> result = new();

        if (data?.Enemies == null)
            return result;

        foreach (EnemySaveData enemy in data.Enemies)
        {
            result.Add(new EnemyDataModel
            {
                EnemyId = enemy.EnemyId,
                PositionX = enemy.PosX,
                PositionY = enemy.PosY,
                PositionZ = enemy.PosZ
            });
        }

        return result;
    }

    public void Save(IReadOnlyList<EnemyDataModel> enemies)
    {
        SaveData data = LoadOrCreate();
        data.Enemies = new List<EnemySaveData>();

        if (enemies != null)
        {
            foreach (EnemyDataModel enemy in enemies)
            {
                data.Enemies.Add(new EnemySaveData
                {
                    EnemyId = enemy.EnemyId,
                    PosX = enemy.PositionX,
                    PosY = enemy.PositionY,
                    PosZ = enemy.PositionZ
                });
            }
        }

        _saveService.SaveGame(data);
    }

    private SaveData LoadOrCreate()
    {
        return _saveService.HasSave() ? _saveService.LoadGame() ?? new SaveData() : new SaveData();
    }
}
