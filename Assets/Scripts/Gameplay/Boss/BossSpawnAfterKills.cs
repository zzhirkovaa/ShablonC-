using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class BossSpawnAfterKills : MonoBehaviour
{
    [SerializeField] private GameObject _bossRoot;
    [SerializeField] private BossController _bossController;
    [SerializeField] private int _killsRequired = 3;
    [SerializeField] private bool _hideBossOnStart = true;
    [SerializeField] private bool _scanSpawnedEnemies = true;
    [SerializeField] private float _scanInterval = 1f;

    private readonly Dictionary<EnemyHealth, Action> _subscriptions = new Dictionary<EnemyHealth, Action>();
    private Coroutine _scanCoroutine;
    private int _killCount;
    private bool _bossActivated;

    private void Awake()
    {
        ResolveBossReferences();

        if (_hideBossOnStart && _bossRoot != null)
            _bossRoot.SetActive(false);
    }

    private void OnEnable()
    {
        ScanEnemies();

        if (_scanSpawnedEnemies)
            _scanCoroutine = StartCoroutine(ScanRoutine());
    }

    private void OnDisable()
    {
        if (_scanCoroutine != null)
        {
            StopCoroutine(_scanCoroutine);
            _scanCoroutine = null;
        }

        UnsubscribeAll();
    }

    private IEnumerator ScanRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(Mathf.Max(0.1f, _scanInterval));
        while (!_bossActivated)
        {
            yield return wait;
            ScanEnemies();
        }
    }

    private void ScanEnemies()
    {
        EnemyHealth[] enemyHealths = FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth enemyHealth in enemyHealths)
            TrySubscribe(enemyHealth);
    }

    private void TrySubscribe(EnemyHealth enemyHealth)
    {
        if (enemyHealth == null || _subscriptions.ContainsKey(enemyHealth))
            return;

        if (enemyHealth.GetComponent<BossController>() != null)
            return;

        Action handler = () => HandleEnemyDied(enemyHealth);
        _subscriptions.Add(enemyHealth, handler);
        enemyHealth.OnDied += handler;
    }

    private void HandleEnemyDied(EnemyHealth enemyHealth)
    {
        if (enemyHealth == null || !_subscriptions.ContainsKey(enemyHealth))
            return;

        enemyHealth.OnDied -= _subscriptions[enemyHealth];
        _subscriptions.Remove(enemyHealth);

        if (_bossActivated)
            return;

        _killCount++;
        if (_killCount >= Mathf.Max(1, _killsRequired))
            ActivateBoss();
    }

    private void ActivateBoss()
    {
        _bossActivated = true;
        ResolveBossReferences();

        if (_bossRoot != null)
            _bossRoot.SetActive(true);

        ResolveBossReferences();
        _bossController?.ActivateBoss();
    }

    private void ResolveBossReferences()
    {
        if (_bossController == null && _bossRoot != null)
            _bossController = _bossRoot.GetComponentInChildren<BossController>(true);

        if (_bossRoot == null && _bossController != null)
            _bossRoot = _bossController.gameObject;
    }

    private void UnsubscribeAll()
    {
        foreach (KeyValuePair<EnemyHealth, Action> subscription in _subscriptions)
        {
            if (subscription.Key != null)
                subscription.Key.OnDied -= subscription.Value;
        }

        _subscriptions.Clear();
    }
}
