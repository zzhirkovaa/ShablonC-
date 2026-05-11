using System.Collections.Generic;

public sealed class SaveCurrentGameInteractor
{
    private readonly SaveGameInteractor _saveGameInteractor;
    private readonly ISceneLoader _sceneLoader;
    private readonly IPlayerSaveStateReader _playerSaveStateReader;
    private readonly IEnemySaveStateReader _enemySaveStateReader;
    private readonly IScoreSaveStateReader _scoreSaveStateReader;

    public SaveCurrentGameInteractor(
        SaveGameInteractor saveGameInteractor,
        ISceneLoader sceneLoader,
        IPlayerSaveStateReader playerSaveStateReader,
        IEnemySaveStateReader enemySaveStateReader,
        IScoreSaveStateReader scoreSaveStateReader)
    {
        _saveGameInteractor = saveGameInteractor;
        _sceneLoader = sceneLoader;
        _playerSaveStateReader = playerSaveStateReader;
        _enemySaveStateReader = enemySaveStateReader;
        _scoreSaveStateReader = scoreSaveStateReader;
    }

    public void Execute()
    {
        IReadOnlyList<EnemyDataModel> enemies = _enemySaveStateReader.Read();

        _saveGameInteractor.Execute(
            _sceneLoader.CurrentSceneName,
            _playerSaveStateReader.Read(),
            enemies,
            _scoreSaveStateReader.Read());
    }
}
