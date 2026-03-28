using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private SettingsMenuController settingsMenuController;

    [Header("Game Scene")]
    [SerializeField] private PauseMenuController pauseMenuController;

    private IAudioService _audioService;
    private ISaveService _saveService;

    private void Awake()
    {
        _audioService = new AudioService();
        _saveService = new SaveService();

        _audioService.LoadVolume();

        if (mainMenuController != null)
            mainMenuController.Construct();

        if (settingsMenuController != null)
            settingsMenuController.Construct(_audioService);

        if (pauseMenuController != null)
            pauseMenuController.Construct(_saveService);
    }
}