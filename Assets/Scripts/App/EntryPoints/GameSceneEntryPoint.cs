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
    [SerializeField] private ScoreboardView _scoreboardView;

    [Header("Pause")]
    [SerializeField] private MonoBehaviour[] _scriptsToDisableOnPause;
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    [Header("Boss")]
    [SerializeField] private string _bossObjectName = "Boss";

    [Header("Victory Audio")]
    [SerializeField] private AudioSource _victoryMusicSource;
    [SerializeField] private AudioClip _victoryMusicClip;
    [SerializeField] private AudioSource _backgroundMusicSource;

    private Ui.PauseMenu.PauseMenuController _pauseMenuController;
    private HealthBarUiController _healthBarController;
    private DeathScreenUiController _deathScreenController;
    private ScoreboardUiController _scoreboardController;
    private IGameModeService _gameModeService;
    private IScoreModel _scoreModel;
    private EnemyDeathEventHub _enemyDeathEventHub;
    private EnemyKilledScoreHandler _enemyKilledScoreHandler;
    private PlayVictoryMusicOnMobKillsInteractor _playVictoryMusicOnMobKillsInteractor;

    public override void Initialize(AppServices appServices)
    {
        _gameModeService = appServices.GameModeService;
        ComposePlayer();
        EnsureEventSystem();
        appServices.AudioService.ConfigureVictoryMusic(
            _victoryMusicSource,
            _victoryMusicClip,
            _backgroundMusicSource);

        IPlayerRepository playerRepository = new JsonPlayerRepository(appServices.SaveService);
        IEnemyRepository enemyRepository = new JsonEnemyRepository(appServices.SaveService);
        ISceneStateRepository sceneStateRepository = new JsonSceneStateRepository(appServices.SaveService);
        IScoreRepository scoreRepository = new JsonScoreRepository(appServices.SaveService);
        _scoreModel = new ScoreModel();
        _enemyDeathEventHub = new EnemyDeathEventHub();
        _enemyKilledScoreHandler = new EnemyKilledScoreHandler(
            _enemyDeathEventHub,
            new AddScoreInteractor(_scoreModel),
            new EnemyScoreRewardResolver());
        _playVictoryMusicOnMobKillsInteractor = new PlayVictoryMusicOnMobKillsInteractor(
            _enemyDeathEventHub,
            appServices.AudioService);

        SaveGameInteractor saveGameInteractor = new SaveGameInteractor(
            sceneStateRepository,
            playerRepository,
            enemyRepository,
            scoreRepository);

        LoadGameInteractor loadGameInteractor = new LoadGameInteractor(
            sceneStateRepository,
            playerRepository,
            enemyRepository,
            scoreRepository);

        IPlayerSaveStateReader playerSaveStateReader = new PlayerSaveStateReader(
            _playerController.transform,
            _playerStatsProvider);

        IPlayerSaveStateWriter playerSaveStateWriter = new PlayerSaveStateWriter(
            _playerController.transform,
            _playerController.CharacterController,
            _playerStatsProvider);

        IEnemySaveStateReader enemySaveStateReader = new EnemySaveStateReader();
        IEnemySaveStateWriter enemySaveStateWriter = new EnemySaveStateWriter();
        IScoreSaveStateReader scoreSaveStateReader = new ScoreSaveStateReader(_scoreModel);
        IScoreSaveStateWriter scoreSaveStateWriter = new ScoreSaveStateWriter(_scoreModel);
        IPauseStateService pauseStateService = new PauseStateService(_scriptsToDisableOnPause);

        SaveCurrentGameInteractor saveCurrentGameInteractor = new SaveCurrentGameInteractor(
            saveGameInteractor,
            appServices.SceneLoader,
            playerSaveStateReader,
            enemySaveStateReader,
            scoreSaveStateReader);

        LoadCurrentGameInteractor loadCurrentGameInteractor = new LoadCurrentGameInteractor(
            loadGameInteractor,
            appServices.SceneLoader,
            appServices.PendingLoadDataService,
            playerSaveStateWriter,
            enemySaveStateWriter,
            scoreSaveStateWriter);

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

        if (_scoreboardView != null)
            _scoreboardController = new ScoreboardUiController(_scoreModel, _scoreboardView);
        else
            Debug.LogWarning("GameSceneEntryPoint has no ScoreboardView assigned. Score will be tracked but not shown.");

        InjectBoss();
        InjectPlayerIntoEnemies();
        InjectPlayerIntoSpawners();
    }

    private void InjectBoss()
    {
        GameObject bossObject = GameObject.Find(_bossObjectName);
        BossController bossController = null;

        if (bossObject != null)
            bossController = bossObject.GetComponent<BossController>();
        else
            bossController = FindBossControllerIncludingInactive();

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
        RegisterEnemyForScore(bossController.gameObject);
    }

    private BossController FindBossControllerIncludingInactive()
    {
        BossController activeBoss = Object.FindFirstObjectByType<BossController>();
        if (activeBoss != null)
            return activeBoss;

        BossController[] bosses = Resources.FindObjectsOfTypeAll<BossController>();
        foreach (BossController boss in bosses)
        {
            if (boss != null && boss.gameObject.scene.IsValid())
                return boss;
        }

        return null;
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
                RegisterEnemyForScore(enemyAI.gameObject);
                continue;
            }

            enemyAI.SetPeacefulMode(_gameModeService.IsPeacefulMode);
            enemyAI.Construct(_playerController.transform, roomReference.RoomBounds);
            RegisterEnemyForScore(enemyAI.gameObject);
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
                RegisterEnemyForScore(enemyAI.gameObject);
                continue;
            }

            enemyAI.SetPeacefulMode(_gameModeService.IsPeacefulMode);
            enemyAI.Construct(_playerController.transform, roomReference.RoomBounds);
            RegisterEnemyForScore(enemyAI.gameObject);
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

    private void InjectPlayerIntoSpawners()
    {
        EnemySpawner[] spawners = Object.FindObjectsOfType<EnemySpawner>();
        foreach (EnemySpawner spawner in spawners)
            spawner.Construct(_playerController.transform, _gameModeService.IsPeacefulMode, _enemyDeathEventHub);
    }

    private void RegisterEnemyForScore(GameObject enemyObject)
    {
        if (_enemyDeathEventHub == null || enemyObject == null)
            return;

        if (enemyObject.TryGetComponent(out EnemyHealth enemyHealth))
            _enemyDeathEventHub.Register(enemyHealth);
    }

    private void OnDestroy()
    {
        _pauseMenuController?.Dispose();
        _healthBarController?.Dispose();
        _deathScreenController?.Dispose();
        _scoreboardController?.Dispose();
        _playVictoryMusicOnMobKillsInteractor?.Dispose();
        _enemyKilledScoreHandler?.Dispose();
        _enemyDeathEventHub?.Dispose();

        Time.timeScale = 1f;
    }
}
