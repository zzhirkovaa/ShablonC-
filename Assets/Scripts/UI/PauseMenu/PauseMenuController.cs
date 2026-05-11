using System;
using UnityEngine;

namespace Ui.PauseMenu
{
    public sealed class PauseMenuController : IDisposable
    {
        private readonly PauseMenuModel _model;
        private readonly PauseMenuView _view;
        private readonly SaveCurrentGameInteractor _saveCurrentGameInteractor;
        private readonly LoadCurrentGameInteractor _loadCurrentGameInteractor;
        private readonly ISceneLoader _sceneLoader;
        private readonly IPauseStateService _pauseStateService;
        private readonly string _mainMenuSceneName;

        public PauseMenuController(
            PauseMenuModel model,
            PauseMenuView view,
            SaveCurrentGameInteractor saveCurrentGameInteractor,
            LoadCurrentGameInteractor loadCurrentGameInteractor,
            ISceneLoader sceneLoader,
            IPauseStateService pauseStateService,
            string mainMenuSceneName)
        {
            _model = model;
            _view = view;
            _saveCurrentGameInteractor = saveCurrentGameInteractor;
            _loadCurrentGameInteractor = loadCurrentGameInteractor;
            _sceneLoader = sceneLoader;
            _pauseStateService = pauseStateService;
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
            _loadCurrentGameInteractor.ApplyPendingLoadIfNeeded();
        }

        private void OnToggleRequested()
        {
            if (_model.IsPaused)
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
            _saveCurrentGameInteractor.Execute();
        }

        private void OnLoadClicked()
        {
            LoadCurrentGameResult result = _loadCurrentGameInteractor.Execute();
            if (result == LoadCurrentGameResult.NoData)
                return;

            _model.IsPaused = false;
            _pauseStateService.ExitPause();

            if (result == LoadCurrentGameResult.AppliedInCurrentScene)
            {
                _view.Hide();
            }
        }

        private void OnMainMenuClicked()
        {
            _model.IsPaused = false;
            _pauseStateService.ExitPause();
            _sceneLoader.Load(_mainMenuSceneName);
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
