public sealed class AppServices
{
    public AppServices(
        IAudioService audioService,
        ISaveService saveService,
        ISceneLoader sceneLoader,
        IPendingLoadDataService pendingLoadDataService)
    {
        AudioService = audioService;
        SaveService = saveService;
        SceneLoader = sceneLoader;
        PendingLoadDataService = pendingLoadDataService;
    }

    public IAudioService AudioService { get; }
    public ISaveService SaveService { get; }
    public ISceneLoader SceneLoader { get; }
    public IPendingLoadDataService PendingLoadDataService { get; }
}