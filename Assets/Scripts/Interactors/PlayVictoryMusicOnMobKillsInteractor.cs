using System;

public sealed class PlayVictoryMusicOnMobKillsInteractor : IDisposable
{
    private const int RequiredMobKills = 5;

    private readonly IEnemyDeathEventSource _enemyDeathEventSource;
    private readonly IAudioService _audioService;

    private int _killCount;
    private bool _played;

    public PlayVictoryMusicOnMobKillsInteractor(
        IEnemyDeathEventSource enemyDeathEventSource,
        IAudioService audioService)
    {
        _enemyDeathEventSource = enemyDeathEventSource;
        _audioService = audioService;

        if (_enemyDeathEventSource != null)
            _enemyDeathEventSource.EnemyDied += OnEnemyDied;
    }

    public void Reset()
    {
        _killCount = 0;
        _played = false;
    }

    public void Dispose()
    {
        if (_enemyDeathEventSource != null)
            _enemyDeathEventSource.EnemyDied -= OnEnemyDied;
    }

    private void OnEnemyDied(EnemyHealth enemyHealth)
    {
        if (_played || !IsRegularMob(enemyHealth))
            return;

        _killCount++;
        if (_killCount < RequiredMobKills)
            return;

        _played = true;
        _audioService?.PlayVictoryMusic();
    }

    private static bool IsRegularMob(EnemyHealth enemyHealth)
    {
        if (enemyHealth == null)
            return false;

        if (enemyHealth.GetComponent<BossController>() != null)
            return false;

        return enemyHealth.GetComponent<EnemyAI>() != null
            || enemyHealth.GetComponent<EnemyRangedAI>() != null;
    }
}
