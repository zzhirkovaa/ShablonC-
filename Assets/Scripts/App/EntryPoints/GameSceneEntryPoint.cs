using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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

    [Header("Boss")]
    [SerializeField] private string _bossObjectName = "Boss";

    private Ui.PauseMenu.PauseMenuController _pauseMenuController;
    private HealthBarUiController _healthBarController;
    private DeathScreenUiController _deathScreenController;
    private IGameModeService _gameModeService;

    public override void Initialize(AppServices appServices)
    {
        _gameModeService = appServices.GameModeService;
        ComposePlayer();
        EnsureEventSystem();

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
        IPauseStateService pauseStateService = new PauseStateService(_scriptsToDisableOnPause);

        SaveCurrentGameInteractor saveCurrentGameInteractor = new SaveCurrentGameInteractor(
            saveGameInteractor,
            appServices.SceneLoader,
            playerSaveStateReader,
            enemySaveStateReader);

        LoadCurrentGameInteractor loadCurrentGameInteractor = new LoadCurrentGameInteractor(
            loadGameInteractor,
            appServices.SceneLoader,
            appServices.PendingLoadDataService,
            playerSaveStateWriter,
            enemySaveStateWriter);

        _pauseMenuController = new Ui.PauseMenu.PauseMenuController(
            new PauseMenuModel(),
            _pauseMenuView,
            saveCurrentGameInteractor,
            loadCurrentGameInteractor,
            appServices.SceneLoader,
            pauseStateService,
            _mainMenuSceneName);

        _pauseMenuController.ApplyPendingLoadIfNeeded();

        _healthBarController = new HealthBarUiController(
            _playerHealth,
            _healthBarView);

        _deathScreenController = new DeathScreenUiController(
            new DeathScreenModel(),
            _playerHealth,
            _deathScreenView,
            appServices.SceneLoader);

        InjectBoss();
        InjectPlayerIntoEnemies();
    }

    private void InjectBoss()
    {
        GameObject bossObject = GameObject.Find(_bossObjectName);
        BossController bossController = null;

        if (bossObject != null)
            bossController = bossObject.GetComponent<BossController>();
        else
            bossController = Object.FindFirstObjectByType<BossController>();

        if (bossController == null && bossObject == null)
        {
            Debug.LogWarning($"Boss object '{_bossObjectName}' or any BossController was not found on the scene.");
            return;
        }

        if (bossController == null)
            bossController = bossObject.AddComponent<BossController>();

        EnemyRoomReference roomReference = bossController.GetComponent<EnemyRoomReference>();
        bossController.SetPeacefulMode(_gameModeService.IsPeacefulMode);
        bossController.Construct(_playerController.transform, roomReference != null ? roomReference.RoomBounds : null);
    }

    private void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private void ComposePlayer()
    {
        IPlayerInputService inputService = new UnityPlayerInputService();
        PlayerModel playerModel = new PlayerModel();

        IPlayerMovement movement = new PlayerMovement(
            _playerController.transform,
            _playerController.CharacterController,
            _playerController.WalkSpeed,
            _playerController.RunSpeed,
            _playerController.RotationSpeed,
            _cameraController);

        IPlayerAppearance appearance = new PlayerAppearance(
            _playerController.Animator,
            _playerController.Renderers,
            _playerController.AnimationSmoothTime);

        PlayerGameplayController gameplayController = new PlayerGameplayController(
            playerModel,
            _playerController,
            inputService,
            _cameraController,
            movement,
            appearance,
            _playerController.RotationSpeed,
            _playerController.StartAnimThreshold);

        _playerController.Initialize(gameplayController);
    }

    private void InjectPlayerIntoEnemies()
    {
        EnemyAI[] meleeEnemies = Object.FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemyAI in meleeEnemies)
        {
            if (enemyAI.TryGetComponent<BossController>(out _))
                continue;

            EnemyRoomReference roomReference = enemyAI.GetComponent<EnemyRoomReference>();

            if (roomReference == null || roomReference.RoomBounds == null)
            {
                Debug.LogWarning($"Enemy {enemyAI.name} has no EnemyRoomReference or RoomBounds assigned.");
                enemyAI.SetPeacefulMode(_gameModeService.IsPeacefulMode);
                enemyAI.Construct(_playerController.transform, null);
                continue;
            }

            enemyAI.SetPeacefulMode(_gameModeService.IsPeacefulMode);
            enemyAI.Construct(_playerController.transform, roomReference.RoomBounds);
        }

        EnemyRangedAI[] rangedEnemies = Object.FindObjectsOfType<EnemyRangedAI>();
        foreach (EnemyRangedAI enemyAI in rangedEnemies)
        {
            if (enemyAI.TryGetComponent<BossController>(out _))
                continue;

            EnemyRoomReference roomReference = enemyAI.GetComponent<EnemyRoomReference>();

            if (roomReference == null || roomReference.RoomBounds == null)
            {
                Debug.LogWarning($"Ranged enemy {enemyAI.name} has no EnemyRoomReference or RoomBounds assigned.");
                enemyAI.SetPeacefulMode(_gameModeService.IsPeacefulMode);
                enemyAI.Construct(_playerController.transform, null);
                continue;
            }

            enemyAI.SetPeacefulMode(_gameModeService.IsPeacefulMode);
            enemyAI.Construct(_playerController.transform, roomReference.RoomBounds);
        }

        EnemyCombat[] enemiesCombat = Object.FindObjectsOfType<EnemyCombat>();
        foreach (EnemyCombat enemyCombat in enemiesCombat)
        {
            if (enemyCombat.TryGetComponent<BossController>(out _))
                continue;

            enemyCombat.Construct(_playerController.transform);
        }

        EnemyRangedCombat[] rangedCombat = Object.FindObjectsOfType<EnemyRangedCombat>();
        foreach (EnemyRangedCombat enemyCombat in rangedCombat)
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
