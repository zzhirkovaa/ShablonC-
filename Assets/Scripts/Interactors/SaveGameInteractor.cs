using System.Collections.Generic;

public sealed class SaveGameInteractor
{
    private readonly ISceneStateRepository _sceneStateRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IEnemyRepository _enemyRepository;
    private readonly IScoreRepository _scoreRepository;

    public SaveGameInteractor(
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

    public void Execute(string sceneName, PlayerDataModel playerData, IReadOnlyList<EnemyDataModel> enemies, int score)
    {
        _sceneStateRepository.Save(sceneName);
        _playerRepository.Save(playerData);
        _enemyRepository.Save(enemies);
        _scoreRepository.Save(score);
    }
}
