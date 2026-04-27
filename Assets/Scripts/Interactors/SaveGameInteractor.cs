using System.Collections.Generic;

public sealed class SaveGameInteractor
{
    private readonly ISceneStateRepository _sceneStateRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IEnemyRepository _enemyRepository;

    public SaveGameInteractor(
        ISceneStateRepository sceneStateRepository,
        IPlayerRepository playerRepository,
        IEnemyRepository enemyRepository)
    {
        _sceneStateRepository = sceneStateRepository;
        _playerRepository = playerRepository;
        _enemyRepository = enemyRepository;
    }

    public void Execute(string sceneName, PlayerDataModel playerData, IReadOnlyList<EnemyDataModel> enemies)
    {
        _sceneStateRepository.Save(sceneName);
        _playerRepository.Save(playerData);
        _enemyRepository.Save(enemies);
    }
}
