using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class EnemySpawner : MonoBehaviour
{
    [Header("Factories")]
    [SerializeField] private List<EnemyFactorySO> _factories = new();

    [Header("Spawn")]
    [SerializeField] private int _count = 3;
    [SerializeField] private float _spawnDelay = 0.5f;
    [SerializeField] private bool _spawnOnStart = true;
    [SerializeField] private SpawnPositionMode _positionMode = SpawnPositionMode.RandomRadius;
    [SerializeField] private float _spawnRadius = 4f;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private RoomBounds _roomBounds;
    [SerializeField] private bool _useNavMesh = true;
    [SerializeField] private float _navMeshSampleDistance = 4f;
    [SerializeField] private float _spawnClearRadius = 1.25f;
    [SerializeField] private int _spawnPositionAttempts = 12;

    private Transform _playerTransform;
    private bool _isPeacefulMode;
    private bool _isInitialized;
    private Coroutine _spawnCoroutine;
    private EnemyDeathEventHub _enemyDeathEventHub;

    public void Construct(Transform playerTransform, bool isPeacefulMode, EnemyDeathEventHub enemyDeathEventHub)
    {
        _playerTransform = playerTransform;
        _isPeacefulMode = isPeacefulMode;
        _enemyDeathEventHub = enemyDeathEventHub;
        _isInitialized = true;

        if (_spawnOnStart && isActiveAndEnabled)
            StartSpawnRoutine();
    }

    private void Awake()
    {
        if (_roomBounds == null)
            _roomBounds = GetComponent<RoomBounds>();

        if (_roomBounds == null)
            _roomBounds = GetComponentInParent<RoomBounds>();
    }

    private void OnDisable()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    public void SpawnNow()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning($"[{name}] EnemySpawner is not initialized yet.");
            return;
        }

        StartSpawnRoutine();
    }

    private void StartSpawnRoutine()
    {
        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);

        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        if (_factories == null || _factories.Count == 0)
        {
            Debug.LogWarning($"[{name}] EnemySpawner has no factories assigned.");
            _spawnCoroutine = null;
            yield break;
        }

        int spawnCount = Mathf.Max(0, _count);
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnSingleEnemy();

            if (i < spawnCount - 1 && _spawnDelay > 0f)
                yield return new WaitForSeconds(_spawnDelay);
        }

        _spawnCoroutine = null;
    }

    private void SpawnSingleEnemy()
    {
        EnemyFactorySO factory = SelectFactory();
        if (factory == null)
            return;

        Vector3 spawnPosition = ResolveSpawnPosition();
        Quaternion spawnRotation = transform.rotation;
        factory.Spawn(spawnPosition, spawnRotation, _playerTransform, _roomBounds, _isPeacefulMode, _enemyDeathEventHub);
    }

    private EnemyFactorySO SelectFactory()
    {
        List<EnemyFactorySO> validFactories = new();
        foreach (EnemyFactorySO factory in _factories)
        {
            if (factory != null)
                validFactories.Add(factory);
        }

        if (validFactories.Count == 0)
        {
            Debug.LogWarning($"[{name}] EnemySpawner factory list contains only null entries.");
            return null;
        }

        return validFactories[Random.Range(0, validFactories.Count)];
    }

    private Vector3 ResolveSpawnPosition()
    {
        int attempts = Mathf.Max(1, _spawnPositionAttempts);
        Vector3 fallbackPosition = transform.position;

        for (int i = 0; i < attempts; i++)
        {
            Vector3 candidate = _positionMode == SpawnPositionMode.SpawnPoints && _spawnPoints != null && _spawnPoints.Length > 0
                ? SelectSpawnPointPosition()
                : SelectRandomRadiusPosition();

            candidate = ApplyBounds(candidate);
            fallbackPosition = candidate;

            if (IsSpawnPositionFree(candidate))
                return candidate;
        }

        return fallbackPosition;
    }

    private Vector3 SelectSpawnPointPosition()
    {
        List<Transform> validPoints = new();
        foreach (Transform point in _spawnPoints)
        {
            if (point != null)
                validPoints.Add(point);
        }

        if (validPoints.Count == 0)
            return SelectRandomRadiusPosition();

        return validPoints[Random.Range(0, validPoints.Count)].position;
    }

    private Vector3 SelectRandomRadiusPosition()
    {
        Vector2 offset = Random.insideUnitCircle * Mathf.Max(0f, _spawnRadius);
        return transform.position + new Vector3(offset.x, 0f, offset.y);
    }

    private Vector3 ApplyBounds(Vector3 position)
    {
        if (_roomBounds != null)
            position = _roomBounds.ClampPosition(position);

        if (_useNavMesh && NavMesh.SamplePosition(position, out NavMeshHit hit, _navMeshSampleDistance, NavMesh.AllAreas))
            position = hit.position;

        return position;
    }

    private bool IsSpawnPositionFree(Vector3 position)
    {
        float radius = Mathf.Max(0.1f, _spawnClearRadius);
        Vector3 checkCenter = position + Vector3.up * radius;
        Collider[] overlaps = Physics.OverlapSphere(checkCenter, radius, ~0, QueryTriggerInteraction.Ignore);

        foreach (Collider overlap in overlaps)
        {
            if (overlap == null)
                continue;

            if (overlap.transform.IsChildOf(transform))
                continue;

            if (overlap.GetComponentInParent<EnemyHealth>() != null)
                return false;
        }

        return true;
    }
}
