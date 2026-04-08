using System;

namespace Ui.MainMenu
{
    public sealed class MainMenuController : IDisposable
    {
        private readonly MainMenuModel _model;
        private readonly MainMenuView _mainMenuView;
        private readonly SettingsMenuView _settingsView;
        private readonly ISceneLoader _sceneLoader;
        private readonly string _gameSceneName;

        public MainMenuController(
            MainMenuModel model,
            MainMenuView mainMenuView,
            SettingsMenuView settingsView,
            ISceneLoader sceneLoader,
            string gameSceneName)
        {
            _model = model;
            _mainMenuView = mainMenuView;
            _settingsView = settingsView;
            _sceneLoader = sceneLoader;
            _gameSceneName = gameSceneName;

            _mainMenuView.PlayClicked += OnPlayClicked;
            _mainMenuView.SettingsClicked += OnSettingsClicked;
            _mainMenuView.ExitClicked += OnExitClicked;
            _settingsView.BackClicked += OnBackClicked;

            ApplyState(false);
        }

        public void Dispose()
        {
            _mainMenuView.PlayClicked -= OnPlayClicked;
            _mainMenuView.SettingsClicked -= OnSettingsClicked;
            _mainMenuView.ExitClicked -= OnExitClicked;
            _settingsView.BackClicked -= OnBackClicked;
        }

        private void OnPlayClicked()
        {
            _sceneLoader.Load(_gameSceneName);
        }

        private void OnSettingsClicked()
        {
            ApplyState(true);
        }

        private void OnBackClicked()
        {
            ApplyState(false);
        }

        private void OnExitClicked()
        {
            UnityEngine.Application.Quit();
        }

        private void ApplyState(bool isSettingsOpen)
        {
            _model.IsSettingsOpen = isSettingsOpen;

            if (_model.IsSettingsOpen)
            {
                _mainMenuView.Hide();
                _settingsView.Show();
                return;
            }

            _settingsView.Hide();
            _mainMenuView.Show();
        }
    }
}
