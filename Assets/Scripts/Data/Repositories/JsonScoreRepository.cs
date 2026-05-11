using System;

public sealed class JsonScoreRepository : IScoreRepository
{
    private readonly ISaveService _saveService;

    public JsonScoreRepository(ISaveService saveService)
    {
        _saveService = saveService;
    }

    public int Load()
    {
        if (!_saveService.HasSave())
            return 0;

        SaveData data = _saveService.LoadGame();
        return data != null ? Math.Max(0, data.Score) : 0;
    }

    public void Save(int score)
    {
        SaveData data = LoadOrCreate();
        data.Score = Math.Max(0, score);
        _saveService.SaveGame(data);
    }

    private SaveData LoadOrCreate()
    {
        return _saveService.HasSave() ? _saveService.LoadGame() ?? new SaveData() : new SaveData();
    }
}
