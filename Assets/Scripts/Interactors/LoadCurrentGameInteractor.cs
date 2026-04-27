public enum LoadCurrentGameResult
{
    NoData,
    AppliedInCurrentScene,
    SceneSwitchStarted
}

public sealed class LoadCurrentGameInteractor
{
    private readonly LoadGameInteractor _loadGameInteractor;
    private readonly ISceneLoader _sceneLoader;
    private readonly IPendingLoadDataService _pendingLoadDataService;
    private readonly IPlayerSaveStateWriter _playerSaveStateWriter;
    private readonly IEnemySaveStateWriter _enemySaveStateWriter;

    public LoadCurrentGameInteractor(
        LoadGameInteractor loadGameInteractor,
        ISceneLoader sceneLoader,
        IPendingLoadDataService pendingLoadDataService,
        IPlayerSaveStateWriter playerSaveStateWriter,
        IEnemySaveStateWriter enemySaveStateWriter)
    {
        _loadGameInteractor = loadGameInteractor;
        _sceneLoader = sceneLoader;
        _pendingLoadDataService = pendingLoadDataService;
        _playerSaveStateWriter = playerSaveStateWriter;
        _enemySaveStateWriter = enemySaveStateWriter;
    }

    public void ApplyPendingLoadIfNeeded()
    {
        if (!_pendingLoadDataService.HasPendingData)
            return;

        Apply(_pendingLoadDataService.Consume());
    }

    public LoadCurrentGameResult Execute()
    {
        GameProgressModel progress = _loadGameInteractor.Execute();
        if (progress == null)
            return LoadCurrentGameResult.NoData;

        if (progress.SceneName == _sceneLoader.CurrentSceneName)
        {
            Apply(progress);
            return LoadCurrentGameResult.AppliedInCurrentScene;
        }

        _pendingLoadDataService.Set(progress);
        _sceneLoader.Load(progress.SceneName);
        return LoadCurrentGameResult.SceneSwitchStarted;
    }

    private void Apply(GameProgressModel progress)
    {
        _playerSaveStateWriter.Apply(progress.PlayerData);
        _enemySaveStateWriter.Apply(progress.Enemies);
    }
}
