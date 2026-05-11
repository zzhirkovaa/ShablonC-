public sealed class JsonSceneStateRepository : ISceneStateRepository
{
    private readonly ISaveService _saveService;

    public JsonSceneStateRepository(ISaveService saveService)
    {
        _saveService = saveService;
    }

    public string Load()
    {
        if (!_saveService.HasSave())
            return null;

        return _saveService.LoadGame()?.SceneName;
    }

    public void Save(string sceneName)
    {
        SaveData data = LoadOrCreate();
        data.SceneName = sceneName;
        _saveService.SaveGame(data);
    }

    private SaveData LoadOrCreate()
    {
        return _saveService.HasSave() ? _saveService.LoadGame() ?? new SaveData() : new SaveData();
    }
}
