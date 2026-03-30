using UnityEngine;
using Player.Core;
using Player.Interfaces;
using Player.UI;

public sealed class GameSceneEntryPoint : SceneEntryPointBase
{
    [Header("Player")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private PlayerStatsProvider _playerStatsProvider;

    [Header("Camera")]
    [SerializeField] private CameraController _cameraController;

    [Header("UI")]
    [SerializeField] private PauseMenuView _pauseMenuView;
    [SerializeField] private HealthBarView _healthBarView;
    [SerializeField] private DeathScreenView _deathScreenView;

    [Header("Pause")]
    [SerializeField] private MonoBehaviour[] _scriptsToDisableOnPause;
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    private PauseMenuUiController _pauseMenuController;
    private HealthBarUiController _healthBarController;
    private DeathScreenUiController _deathScreenController;

    public override void Initialize(AppServices appServices)
    {
        ComposePlayer();

        _pauseMenuController = new PauseMenuUiController(
            _pauseMenuView,
            appServices.SaveService,
            appServices.SceneLoader,
            appServices.PendingLoadDataService,
            _playerController.transform,
            _playerController.CharacterController,
            _playerStatsProvider,
            _scriptsToDisableOnPause,
            _mainMenuSceneName);

        _pauseMenuController.ApplyPendingSaveIfNeeded();

        _healthBarController = new HealthBarUiController(
            _playerHealth,
            _healthBarView);

        _deathScreenController = new DeathScreenUiController(
            _playerHealth,
            _deathScreenView,
            appServices.SceneLoader);

        InjectPlayerIntoEnemies();
    }

    private void ComposePlayer()
    {
        IPlayerInputService inputService = new UnityPlayerInputService();

        IPlayerMovement movement = new PlayerMovement(
            _playerController.transform,
            _playerController.CharacterController,
            _playerController.WalkSpeed,
            _playerController.RunSpeed,
            _playerController.RotationSpeed);

        IPlayerAppearance appearance = new PlayerAppearance(
            _playerController.Animator,
            _playerController.Renderers,
            _playerController.AnimationSmoothTime);

        _playerController.Construct(
            inputService,
            _cameraController,
            movement,
            appearance);
    }

    private void InjectPlayerIntoEnemies()
    {
        EnemyAI[] enemiesAi = Object.FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemyAI in enemiesAi)
            enemyAI.Construct(_playerController.transform);

        EnemyCombat[] enemiesCombat = Object.FindObjectsOfType<EnemyCombat>();
        foreach (EnemyCombat enemyCombat in enemiesCombat)
            enemyCombat.Construct(_playerController.transform);
    }

    private void OnDestroy()
    {
        _pauseMenuController?.Dispose();
        _healthBarController?.Dispose();
        _deathScreenController?.Dispose();

        Time.timeScale = 1f;
    }
}