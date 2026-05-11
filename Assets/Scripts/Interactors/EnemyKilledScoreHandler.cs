using System;

public sealed class EnemyKilledScoreHandler : IDisposable
{
    private readonly EnemyDeathEventHub _enemyDeathEventHub;
    private readonly AddScoreInteractor _addScoreInteractor;
    private readonly EnemyScoreRewardResolver _rewardResolver;

    public EnemyKilledScoreHandler(
        EnemyDeathEventHub enemyDeathEventHub,
        AddScoreInteractor addScoreInteractor,
        EnemyScoreRewardResolver rewardResolver)
    {
        _enemyDeathEventHub = enemyDeathEventHub;
        _addScoreInteractor = addScoreInteractor;
        _rewardResolver = rewardResolver;

        if (_enemyDeathEventHub != null)
            _enemyDeathEventHub.EnemyDied += OnEnemyDied;
    }

    public void Dispose()
    {
        if (_enemyDeathEventHub != null)
            _enemyDeathEventHub.EnemyDied -= OnEnemyDied;
    }

    private void OnEnemyDied(EnemyHealth enemyHealth)
    {
        int reward = _rewardResolver != null ? _rewardResolver.Resolve(enemyHealth) : 0;
        if (reward > 0)
            _addScoreInteractor?.Execute(reward);
    }
}
