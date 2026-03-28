using UnityEngine;

public sealed class MainMenuSceneEntryPoint : SceneEntryPointBase
{
    [SerializeField] private MainMenuView _mainMenuView;
    [SerializeField] private SettingsMenuView _settingsMenuView;
    [SerializeField] private string _gameSceneName = "GameScene";

    private MainMenuUiController _mainMenuController;
    private SettingsMenuUiController _settingsController;

    public override void Initialize(AppServices appServices)
    {
        _settingsController = new SettingsMenuUiController(
            _settingsMenuView,
            appServices.AudioService);

        _mainMenuController = new MainMenuUiController(
            _mainMenuView,
            _settingsMenuView,
            appServices.SceneLoader,
            _gameSceneName);
    }

    private void OnDestroy()
    {
        _mainMenuController?.Dispose();
        _settingsController?.Dispose();
    }
}