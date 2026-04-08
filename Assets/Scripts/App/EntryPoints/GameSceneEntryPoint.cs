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

    private Ui.PauseMenu.PauseMenuController _pauseMenuController;
    private HealthBarUiController _healthBarController;
    private DeathScreenUiController _deathScreenController;

    public override void Initialize(AppServices appServices)
    {
        ComposePlayer();

        IPlayerRepository playerRepository = new JsonPlayerRepository(appServices.SaveService);
        IEnemyRepository enemyRepository = new JsonEnemyRepository(appServices.SaveService);
        ISceneStateRepository sceneStateRepository = new JsonSceneStateRepository(appServices.SaveService);

        SaveGameInteractor saveGameInteractor = new SaveGameInteractor(
            sceneStateRepository,
            playerRepository,
            enemyRepository);

        LoadGameInteractor loadGameInteractor = new LoadGameInteractor(
            sceneStateRepository,
            playerRepository,
            enemyRepository);

        IPlayerSaveStateReader playerSaveStateReader = new PlayerSaveStateReader(
            _playerController.transform,
            _playerStatsProvider);

        IPlayerSaveStateWriter playerSaveStateWriter = new PlayerSaveStateWriter(
            _playerController.transform,
            _playerController.CharacterController,
            _playerStatsProvider);

        IEnemySaveStateReader enemySaveStateReader = new EnemySaveStateReader();
        IEnemySaveStateWriter enemySaveStateWriter = new EnemySaveStateWriter();

        _pauseMenuController = new Ui.PauseMenu.PauseMenuController(
            new PauseMenuModel(),
            _pauseMenuView,
            saveGameInteractor,
            loadGameInteractor,
            appServices.SceneLoader,
            appServices.PendingLoadDataService,
            playerSaveStateReader,
            playerSaveStateWriter,
            enemySaveStateReader,
            enemySaveStateWriter,
            _scriptsToDisableOnPause,
            _mainMenuSceneName);

        _pauseMenuController.ApplyPendingLoadIfNeeded();

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
        EnemyAI[] meleeEnemies = Object.FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemyAI in meleeEnemies)
        {
            EnemyRoomReference roomReference = enemyAI.GetComponent<EnemyRoomReference>();

            if (roomReference == null || roomReference.RoomBounds == null)
            {
                Debug.LogWarning($"Enemy {enemyAI.name} has no EnemyRoomReference or RoomBounds assigned.");
                enemyAI.Construct(_playerController.transform, null);
                continue;
            }

            enemyAI.Construct(_playerController.transform, roomReference.RoomBounds);
        }

        EnemyRangedAI[] rangedEnemies = Object.FindObjectsOfType<EnemyRangedAI>();
        foreach (EnemyRangedAI enemyAI in rangedEnemies)
        {
            EnemyRoomReference roomReference = enemyAI.GetComponent<EnemyRoomReference>();

            if (roomReference == null || roomReference.RoomBounds == null)
            {
                Debug.LogWarning($"Ranged enemy {enemyAI.name} has no EnemyRoomReference or RoomBounds assigned.");
                enemyAI.Construct(_playerController.transform, null);
                continue;
            }

            enemyAI.Construct(_playerController.transform, roomReference.RoomBounds);
        }

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
