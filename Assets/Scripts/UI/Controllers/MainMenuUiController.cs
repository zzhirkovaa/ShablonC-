using System;

public sealed class MainMenuUiController : IDisposable
{
    private readonly MainMenuView _mainMenuView;
    private readonly SettingsMenuView _settingsMenuView;
    private readonly ISceneLoader _sceneLoader;
    private readonly string _gameSceneName;

    public MainMenuUiController(
        MainMenuView mainMenuView,
        SettingsMenuView settingsMenuView,
        ISceneLoader sceneLoader,
        string gameSceneName)
    {
        _mainMenuView = mainMenuView;
        _settingsMenuView = settingsMenuView;
        _sceneLoader = sceneLoader;
        _gameSceneName = gameSceneName;

        _mainMenuView.PlayClicked += OnPlayClicked;
        _mainMenuView.SettingsClicked += OnSettingsClicked;
        _mainMenuView.ExitClicked += OnExitClicked;
        _settingsMenuView.BackClicked += OnBackClicked;

        _mainMenuView.Show();
        _settingsMenuView.Hide();
    }

    public void Dispose()
    {
        _mainMenuView.PlayClicked -= OnPlayClicked;
        _mainMenuView.SettingsClicked -= OnSettingsClicked;
        _mainMenuView.ExitClicked -= OnExitClicked;
        _settingsMenuView.BackClicked -= OnBackClicked;
    }

    private void OnPlayClicked()
    {
        _sceneLoader.Load(_gameSceneName);
    }

    private void OnSettingsClicked()
    {
        _mainMenuView.Hide();
        _settingsMenuView.Show();
    }

    private void OnBackClicked()
    {
        _settingsMenuView.Hide();
        _mainMenuView.Show();
    }

    private void OnExitClicked()
    {
        UnityEngine.Application.Quit();
    }
}