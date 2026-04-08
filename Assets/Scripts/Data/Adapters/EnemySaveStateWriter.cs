using System.Collections.Generic;
using UnityEngine;

public sealed class EnemySaveStateWriter : IEnemySaveStateWriter
{
    public void Apply(IReadOnlyList<EnemyDataModel> enemies)
    {
        if (enemies == null || enemies.Count == 0)
            return;

        EnemySaveId[] sceneEnemies = Object.FindObjectsOfType<EnemySaveId>();
        Dictionary<string, EnemySaveId> enemyMap = new();

        foreach (EnemySaveId enemy in sceneEnemies)
        {
            if (!string.IsNullOrWhiteSpace(enemy.Id))
                enemyMap[enemy.Id] = enemy;
        }

        foreach (EnemyDataModel enemyData in enemies)
        {
            if (!enemyMap.TryGetValue(enemyData.EnemyId, out EnemySaveId enemy))
                continue;

            enemy.transform.position = new Vector3(
                enemyData.PositionX,
                enemyData.PositionY,
                enemyData.PositionZ);
        }
    }
}
