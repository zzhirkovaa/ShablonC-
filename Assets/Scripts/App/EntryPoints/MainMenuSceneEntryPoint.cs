using UnityEngine;

public sealed class MainMenuSceneEntryPoint : SceneEntryPointBase
{
    [SerializeField] private MainMenuView _mainMenuView;
    [SerializeField] private SettingsMenuView _settingsMenuView;
    [SerializeField] private string _gameSceneName = "GameScene";

    private Ui.MainMenu.MainMenuController _mainMenuController;
    private Ui.Settings.SettingsController _settingsController;

    public override void Initialize(AppServices appServices)
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ISettingsRepository settingsRepository = new PlayerPrefsSettingsRepository();
        LoadSettingsInteractor loadSettingsInteractor = new LoadSettingsInteractor(settingsRepository);
        SaveSettingsInteractor saveSettingsInteractor = new SaveSettingsInteractor(settingsRepository);

        _settingsController = new Ui.Settings.SettingsController(
            new SettingsModel(),
            _settingsMenuView,
            loadSettingsInteractor,
            saveSettingsInteractor,
            appServices.AudioService);

        _mainMenuController = new Ui.MainMenu.MainMenuController(
            new MainMenuModel(),
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
