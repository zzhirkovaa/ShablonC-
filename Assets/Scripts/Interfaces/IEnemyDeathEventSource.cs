using System;

public interface IEnemyDeathEventSource
{
    event Action<EnemyHealth> EnemyDied;
}
