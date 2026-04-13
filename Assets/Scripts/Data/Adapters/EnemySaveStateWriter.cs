using System.Collections.Generic;
using UnityEngine;

public sealed class EnemySaveStateWriter : IEnemySaveStateWriter
{
    public void Apply(IReadOnlyList<EnemyDataModel> enemies)
    {
        Dictionary<string, EnemySaveId> enemyMap = new();

        foreach (EnemySaveId enemy in EnemySaveId.RegisteredInstances)
        {
            if (enemy != null && !string.IsNullOrWhiteSpace(enemy.Id) && !enemyMap.ContainsKey(enemy.Id))
                enemyMap[enemy.Id] = enemy;
        }

        if (enemies == null)
            return;

        foreach (EnemyDataModel enemyData in enemies)
        {
            if (!enemyMap.TryGetValue(enemyData.EnemyId, out EnemySaveId enemy))
                continue;

            enemy.gameObject.SetActive(true);
            enemy.transform.position = new Vector3(
                enemyData.PositionX,
                enemyData.PositionY,
                enemyData.PositionZ);

            if (enemy.TryGetComponent(out EnemyHealth enemyHealth))
                enemyHealth.RestoreState(enemyData.Health, enemyData.IsDead);
            else if (enemyData.IsDead)
                enemy.gameObject.SetActive(false);
        }
    }
}
