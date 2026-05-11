using System;
using System.Collections.Generic;

public sealed class EnemyDeathEventHub : IDisposable, IEnemyDeathEventSource
{
    private readonly Dictionary<EnemyHealth, Action> _subscriptions = new();

    public event Action<EnemyHealth> EnemyDied;

    public void Register(EnemyHealth enemyHealth)
    {
        if (enemyHealth == null || enemyHealth.IsDead || _subscriptions.ContainsKey(enemyHealth))
            return;

        Action handler = () => HandleEnemyDied(enemyHealth);
        _subscriptions.Add(enemyHealth, handler);
        enemyHealth.OnDied += handler;
    }

    public void Unregister(EnemyHealth enemyHealth)
    {
        if (enemyHealth == null || !_subscriptions.TryGetValue(enemyHealth, out Action handler))
            return;

        enemyHealth.OnDied -= handler;
        _subscriptions.Remove(enemyHealth);
    }

    public void Dispose()
    {
        foreach (KeyValuePair<EnemyHealth, Action> subscription in _subscriptions)
        {
            if (subscription.Key != null)
                subscription.Key.OnDied -= subscription.Value;
        }

        _subscriptions.Clear();
    }

    private void HandleEnemyDied(EnemyHealth enemyHealth)
    {
        Unregister(enemyHealth);
        EnemyDied?.Invoke(enemyHealth);
    }
}
