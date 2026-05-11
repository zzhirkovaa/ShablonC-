using System.Collections.Generic;
using UnityEngine;

public sealed class EnemySaveStateReader : IEnemySaveStateReader
{
    public IReadOnlyList<EnemyDataModel> Read()
    {
        List<EnemyDataModel> result = new();
        HashSet<string> usedIds = new();

        foreach (EnemySaveId enemy in EnemySaveId.RegisteredInstances)
        {
            if (enemy == null || string.IsNullOrWhiteSpace(enemy.Id) || !usedIds.Add(enemy.Id))
                continue;

            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

            result.Add(new EnemyDataModel
            {
                EnemyId = enemy.Id,
                PositionX = enemy.transform.position.x,
                PositionY = enemy.transform.position.y,
                PositionZ = enemy.transform.position.z,
                Health = enemyHealth != null ? enemyHealth.CurrentHealth : 0f,
                IsDead = enemyHealth != null ? enemyHealth.IsDead : !enemy.gameObject.activeSelf
            });
        }

        return result;
    }
}
