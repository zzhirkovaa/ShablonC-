public sealed class LoadGameInteractor
{
    private readonly ISceneStateRepository _sceneStateRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IEnemyRepository _enemyRepository;

    public LoadGameInteractor(
        ISceneStateRepository sceneStateRepository,
        IPlayerRepository playerRepository,
        IEnemyRepository enemyRepository)
    {
        _sceneStateRepository = sceneStateRepository;
        _playerRepository = playerRepository;
        _enemyRepository = enemyRepository;
    }

    public GameProgressModel Execute()
    {
        string sceneName = _sceneStateRepository.Load();
        PlayerDataModel playerData = _playerRepository.Load();

        if (string.IsNullOrWhiteSpace(sceneName) || playerData == null)
            return null;

        return new GameProgressModel
        {
            SceneName = sceneName,
            PlayerData = playerData,
            Enemies = new System.Collections.Generic.List<EnemyDataModel>(_enemyRepository.Load())
        };
    }
}
