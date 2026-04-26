using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public sealed class ProjectEntryPoint : MonoBehaviour
{
    [SerializeField] private string _startupSceneName = "MainMenu";
    [SerializeField] private string _bootstrapSceneName = "Bootstrap";

    private AppServices _appServices;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        _appServices = new AppServices(
            new AudioService(),
            new SaveService(),
            new SceneLoader(),
            new PendingLoadDataService(),
            new GameModeService());

        _appServices.AudioService.LoadVolume();

        SceneManager.sceneLoaded += OnSceneLoaded;

        _appServices.SceneLoader.Load(_startupSceneName);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == _bootstrapSceneName)
            return;

        SceneEntryPointBase sceneEntryPoint = FindObjectOfType<SceneEntryPointBase>();

        if (sceneEntryPoint == null)
        {
            Debug.LogWarning($"SceneEntryPointBase was not found on scene {scene.name}");
            return;
        }

        sceneEntryPoint.Initialize(_appServices);
    }
}
