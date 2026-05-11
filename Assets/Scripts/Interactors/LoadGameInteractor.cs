public sealed class LoadGameInteractor
{
    private readonly ISceneStateRepository _sceneStateRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IEnemyRepository _enemyRepository;
    private readonly IScoreRepository _scoreRepository;

    public LoadGameInteractor(
        ISceneStateRepository sceneStateRepository,
        IPlayerRepository playerRepository,
        IEnemyRepository enemyRepository,
        IScoreRepository scoreRepository)
    {
        _sceneStateRepository = sceneStateRepository;
        _playerRepository = playerRepository;
        _enemyRepository = enemyRepository;
        _scoreRepository = scoreRepository;
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
            Score = _scoreRepository.Load(),
            Enemies = new System.Collections.Generic.List<EnemyDataModel>(_enemyRepository.Load())
        };
    }
}
