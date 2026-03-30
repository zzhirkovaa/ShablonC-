using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PauseMenuUiController : IDisposable
{
    private readonly PauseMenuView _view;
    private readonly ISaveService _saveService;
    private readonly ISceneLoader _sceneLoader;
    private readonly IPendingLoadDataService _pendingLoadDataService;
    private readonly Transform _playerTransform;
    private readonly CharacterController _playerCharacterController;
    private readonly IPlayerStatsProvider _playerStatsProvider;
    private readonly MonoBehaviour[] _scriptsToDisableOnPause;
    private readonly string _mainMenuSceneName;

    private bool _isPaused;

    public PauseMenuUiController(
        PauseMenuView view,
        ISaveService saveService,
        ISceneLoader sceneLoader,
        IPendingLoadDataService pendingLoadDataService,
        Transform playerTransform,
        CharacterController playerCharacterController,
        IPlayerStatsProvider playerStatsProvider,
        MonoBehaviour[] scriptsToDisableOnPause,
        string mainMenuSceneName)
    {
        _view = view;
        _saveService = saveService;
        _sceneLoader = sceneLoader;
        _pendingLoadDataService = pendingLoadDataService;
        _playerTransform = playerTransform;
        _playerCharacterController = playerCharacterController;
        _playerStatsProvider = playerStatsProvider;
        _scriptsToDisableOnPause = scriptsToDisableOnPause;
        _mainMenuSceneName = mainMenuSceneName;

        if (_view == null)
        {
            Debug.LogError("PauseMenuUiController: PauseMenuView не передан.");
            return;
        }

        if (_playerTransform == null)
        {
            Debug.LogError("PauseMenuUiController: Player Transform не передан.");
            return;
        }

        if (_playerStatsProvider == null)
        {
            Debug.LogError("PauseMenuUiController: IPlayerStatsProvider не передан.");
            return;
        }

        _view.ToggleRequested += OnToggleRequested;
        _view.ResumeClicked += OnResumeClicked;
        _view.SaveClicked += OnSaveClicked;
        _view.LoadClicked += OnLoadClicked;
        _view.MainMenuClicked += OnMainMenuClicked;

        ResumeGame();
    }

    public void Dispose()
    {
        if (_view == null)
            return;

        _view.ToggleRequested -= OnToggleRequested;
        _view.ResumeClicked -= OnResumeClicked;
        _view.SaveClicked -= OnSaveClicked;
        _view.LoadClicked -= OnLoadClicked;
        _view.MainMenuClicked -= OnMainMenuClicked;
    }

    public void ApplyPendingSaveIfNeeded()
    {
        if (!_pendingLoadDataService.HasPendingData)
            return;

        SaveData data = _pendingLoadDataService.Consume();
        ApplyLoadedData(data);
    }

    private void OnToggleRequested()
    {
        if (_isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void OnResumeClicked()
    {
        ResumeGame();
    }

    private void OnSaveClicked()
    {
        SaveData data = BuildSaveData();
        _saveService.SaveGame(data);
    }

    private void OnLoadClicked()
    {
        SaveData data = _saveService.LoadGame();
        if (data == null)
            return;

        Time.timeScale = 1f;
        _isPaused = false;

        if (data.SceneName == _sceneLoader.CurrentSceneName)
        {
            ApplyLoadedData(data);
            _view.Hide();
            SetGameplayScriptsEnabled(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }

        _pendingLoadDataService.Set(data);
        _sceneLoader.Load(data.SceneName);
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        _sceneLoader.Load(_mainMenuSceneName);
    }

    private SaveData BuildSaveData()
    {
        SaveData data = new SaveData
        {
            SceneName = _sceneLoader.CurrentSceneName,
            PlayerPosX = _playerTransform.position.x,
            PlayerPosY = _playerTransform.position.y,
            PlayerPosZ = _playerTransform.position.z,
            PlayerHp = _playerStatsProvider.CurrentHp,
            PlayerAbilityCooldownRemaining = _playerStatsProvider.CurrentAbilityCooldown,
            Enemies = CollectEnemyData()
        };

        return data;
    }

    private List<EnemySaveData> CollectEnemyData()
    {
        List<EnemySaveData> result = new();

        EnemySaveId[] enemies = UnityEngine.Object.FindObjectsOfType<EnemySaveId>();
        foreach (EnemySaveId enemy in enemies)
        {
            result.Add(new EnemySaveData
            {
                EnemyId = enemy.Id,
                PosX = enemy.transform.position.x,
                PosY = enemy.transform.position.y,
                PosZ = enemy.transform.position.z
            });
        }

        return result;
    }

    private void ApplyLoadedData(SaveData data)
    {
        ApplyPlayerPosition(data);
        _playerStatsProvider.RestoreHp(data.PlayerHp);
        _playerStatsProvider.RestoreAbilityCooldown(data.PlayerAbilityCooldownRemaining);
        ApplyEnemyPositions(data);
    }

    private void PauseGame()
    {
        _view.Show();
        Time.timeScale = 0f;
        _isPaused = true;

        SetGameplayScriptsEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        _view.Hide();
        Time.timeScale = 1f;
        _isPaused = false;

        SetGameplayScriptsEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ApplyPlayerPosition(SaveData data)
    {
        if (_playerCharacterController != null)
            _playerCharacterController.enabled = false;

        _playerTransform.position = new Vector3(
            data.PlayerPosX,
            data.PlayerPosY,
            data.PlayerPosZ);

        if (_playerCharacterController != null)
            _playerCharacterController.enabled = true;
    }

    private void ApplyEnemyPositions(SaveData data)
    {
        if (data.Enemies == null || data.Enemies.Count == 0)
            return;

        EnemySaveId[] enemies = UnityEngine.Object.FindObjectsOfType<EnemySaveId>();
        Dictionary<string, EnemySaveId> enemyMap = new();

        foreach (EnemySaveId enemy in enemies)
        {
            if (!string.IsNullOrWhiteSpace(enemy.Id))
                enemyMap[enemy.Id] = enemy;
        }

        foreach (EnemySaveData enemyData in data.Enemies)
        {
            if (!enemyMap.TryGetValue(enemyData.EnemyId, out EnemySaveId enemy))
                continue;

            enemy.transform.position = new Vector3(
                enemyData.PosX,
                enemyData.PosY,
                enemyData.PosZ);
        }
    }

    private void SetGameplayScriptsEnabled(bool enabledState)
    {
        if (_scriptsToDisableOnPause == null)
            return;

        foreach (MonoBehaviour script in _scriptsToDisableOnPause)
        {
            if (script != null)
                script.enabled = enabledState;
        }
    }
}