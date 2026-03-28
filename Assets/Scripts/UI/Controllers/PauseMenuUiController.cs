using System;
using UnityEngine;

public sealed class PauseMenuUiController : IDisposable
{
    private readonly PauseMenuView _view;
    private readonly ISaveService _saveService;
    private readonly ISceneLoader _sceneLoader;
    private readonly IPendingLoadDataService _pendingLoadDataService;
    private readonly Transform _playerTransform;
    private readonly CharacterController _playerCharacterController;
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
        MonoBehaviour[] scriptsToDisableOnPause,
        string mainMenuSceneName)
    {
        _view = view;
        _saveService = saveService;
        _sceneLoader = sceneLoader;
        _pendingLoadDataService = pendingLoadDataService;
        _playerTransform = playerTransform;
        _playerCharacterController = playerCharacterController;
        _scriptsToDisableOnPause = scriptsToDisableOnPause;
        _mainMenuSceneName = mainMenuSceneName;

        _view.ToggleRequested += OnToggleRequested;
        _view.ResumeClicked += OnResumeClicked;
        _view.SaveClicked += OnSaveClicked;
        _view.LoadClicked += OnLoadClicked;
        _view.MainMenuClicked += OnMainMenuClicked;

        ResumeGame();
    }

    public void Dispose()
    {
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
        ApplyPlayerPosition(data);
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
        _saveService.SaveGame(_sceneLoader.CurrentSceneName, _playerTransform.position);
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
            ApplyPlayerPosition(data);
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

        _playerTransform.position = new Vector3(data.PosX, data.PosY, data.PosZ);

        if (_playerCharacterController != null)
            _playerCharacterController.enabled = true;
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