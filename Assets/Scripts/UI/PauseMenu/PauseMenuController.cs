using System;
using UnityEngine;

namespace Ui.PauseMenu
{
    public sealed class PauseMenuController : IDisposable
    {
        private readonly PauseMenuView _view;
        private readonly SaveGameInteractor _saveGameInteractor;
        private readonly LoadGameInteractor _loadGameInteractor;
        private readonly ISceneLoader _sceneLoader;
        private readonly IPendingLoadDataService _pendingLoadDataService;
        private readonly IPlayerSaveStateReader _playerSaveStateReader;
        private readonly IPlayerSaveStateWriter _playerSaveStateWriter;
        private readonly IEnemySaveStateReader _enemySaveStateReader;
        private readonly IEnemySaveStateWriter _enemySaveStateWriter;
        private readonly MonoBehaviour[] _scriptsToDisableOnPause;
        private readonly string _mainMenuSceneName;

        private bool _isPaused;

        public PauseMenuController(
            PauseMenuModel model,
            PauseMenuView view,
            SaveGameInteractor saveGameInteractor,
            LoadGameInteractor loadGameInteractor,
            ISceneLoader sceneLoader,
            IPendingLoadDataService pendingLoadDataService,
            IPlayerSaveStateReader playerSaveStateReader,
            IPlayerSaveStateWriter playerSaveStateWriter,
            IEnemySaveStateReader enemySaveStateReader,
            IEnemySaveStateWriter enemySaveStateWriter,
            MonoBehaviour[] scriptsToDisableOnPause,
            string mainMenuSceneName)
        {
            _view = view;
            _saveGameInteractor = saveGameInteractor;
            _loadGameInteractor = loadGameInteractor;
            _sceneLoader = sceneLoader;
            _pendingLoadDataService = pendingLoadDataService;
            _playerSaveStateReader = playerSaveStateReader;
            _playerSaveStateWriter = playerSaveStateWriter;
            _enemySaveStateReader = enemySaveStateReader;
            _enemySaveStateWriter = enemySaveStateWriter;
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

        public void ApplyPendingLoadIfNeeded()
        {
            if (!_pendingLoadDataService.HasPendingData)
                return;

            ApplyLoadedData(_pendingLoadDataService.Consume());
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
            _saveGameInteractor.Execute(
                _sceneLoader.CurrentSceneName,
                _playerSaveStateReader.Read(),
                _enemySaveStateReader.Read());
        }

        private void OnLoadClicked()
        {
            GameProgressModel progress = _loadGameInteractor.Execute();
            if (progress == null)
                return;

            Time.timeScale = 1f;
            _isPaused = false;

            if (progress.SceneName == _sceneLoader.CurrentSceneName)
            {
                ApplyLoadedData(progress);
                _view.Hide();
                SetGameplayScriptsEnabled(true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                return;
            }

            _pendingLoadDataService.Set(progress);
            _sceneLoader.Load(progress.SceneName);
        }

        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            _sceneLoader.Load(_mainMenuSceneName);
        }

        private void ApplyLoadedData(GameProgressModel progress)
        {
            _playerSaveStateWriter.Apply(progress.PlayerData);
            _enemySaveStateWriter.Apply(progress.Enemies);
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
}
