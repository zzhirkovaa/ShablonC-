public sealed class AppServices
{
    public AppServices(
        IAudioService audioService,
        ISaveService saveService,
        ISceneLoader sceneLoader,
        IPendingLoadDataService pendingLoadDataService,
        IGameModeService gameModeService)
    {
        AudioService = audioService;
        SaveService = saveService;
        SceneLoader = sceneLoader;
        PendingLoadDataService = pendingLoadDataService;
        GameModeService = gameModeService;
    }

    public IAudioService AudioService { get; }
    public ISaveService SaveService { get; }
    public ISceneLoader SceneLoader { get; }
    public IPendingLoadDataService PendingLoadDataService { get; }
    public IGameModeService GameModeService { get; }
}
