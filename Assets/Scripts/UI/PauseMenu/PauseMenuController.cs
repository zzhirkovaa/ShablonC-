using System;
using UnityEngine;

namespace Ui.PauseMenu
{
    public sealed class PauseMenuController : IDisposable
    {
        private readonly PauseMenuModel _model;
        private readonly PauseMenuView _view;
        private readonly IPauseMenuInput _pauseMenuInput;
        private readonly IPauseStateService _pauseStateService;
        private readonly SaveGameInteractor _saveGameInteractor;
        private readonly LoadGameInteractor _loadGameInteractor;
        private readonly ISceneLoader _sceneLoader;
        private readonly IPendingLoadDataService _pendingLoadDataService;
        private readonly IPlayerSaveStateReader _playerSaveStateReader;
        private readonly IPlayerSaveStateWriter _playerSaveStateWriter;
        private readonly IEnemySaveStateReader _enemySaveStateReader;
        private readonly IEnemySaveStateWriter _enemySaveStateWriter;
        private readonly string _mainMenuSceneName;

        public PauseMenuController(
            PauseMenuModel model,
            PauseMenuView view,
            IPauseMenuInput pauseMenuInput,
            IPauseStateService pauseStateService,
            SaveGameInteractor saveGameInteractor,
            LoadGameInteractor loadGameInteractor,
            ISceneLoader sceneLoader,
            IPendingLoadDataService pendingLoadDataService,
            IPlayerSaveStateReader playerSaveStateReader,
            IPlayerSaveStateWriter playerSaveStateWriter,
            IEnemySaveStateReader enemySaveStateReader,
            IEnemySaveStateWriter enemySaveStateWriter,
            string mainMenuSceneName)
        {
            _model = model;
            _view = view;
            _pauseMenuInput = pauseMenuInput;
            _pauseStateService = pauseStateService;
            _saveGameInteractor = saveGameInteractor;
            _loadGameInteractor = loadGameInteractor;
            _sceneLoader = sceneLoader;
            _pendingLoadDataService = pendingLoadDataService;
            _playerSaveStateReader = playerSaveStateReader;
            _playerSaveStateWriter = playerSaveStateWriter;
            _enemySaveStateReader = enemySaveStateReader;
            _enemySaveStateWriter = enemySaveStateWriter;
            _mainMenuSceneName = mainMenuSceneName;

            _pauseMenuInput.ToggleRequested += OnToggleRequested;
            _view.ResumeClicked += OnResumeClicked;
            _view.SaveClicked += OnSaveClicked;
            _view.LoadClicked += OnLoadClicked;
            _view.MainMenuClicked += OnMainMenuClicked;

            ResumeGame();
        }

        public void Dispose()
        {
            _pauseMenuInput.ToggleRequested -= OnToggleRequested;
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
            if (_model.IsPaused)
            {
                ResumeGame();
                return;
            }

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

            _pauseStateService.ExitPause();
            _model.IsPaused = false;

            if (progress.SceneName == _sceneLoader.CurrentSceneName)
            {
                ApplyLoadedData(progress);
                _view.Hide();
                return;
            }

            _pendingLoadDataService.Set(progress);
            _sceneLoader.Load(progress.SceneName);
        }

        private void OnMainMenuClicked()
        {
            _pauseStateService.ExitPause();
            _sceneLoader.Load(_mainMenuSceneName);
        }

        private void ApplyLoadedData(GameProgressModel progress)
        {
            _playerSaveStateWriter.Apply(progress.PlayerData);
            _enemySaveStateWriter.Apply(progress.Enemies);
        }

        private void PauseGame()
        {
            _model.IsPaused = true;
            _view.Show();
            _pauseStateService.EnterPause();
        }

        private void ResumeGame()
        {
            _model.IsPaused = false;
            _view.Hide();
            _pauseStateService.ExitPause();
        }
    }
}
