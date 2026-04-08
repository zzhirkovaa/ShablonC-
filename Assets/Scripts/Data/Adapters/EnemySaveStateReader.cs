using System.Collections.Generic;
using UnityEngine;

public sealed class EnemySaveStateReader : IEnemySaveStateReader
{
    public IReadOnlyList<EnemyDataModel> Read()
    {
        List<EnemyDataModel> result = new();
        EnemySaveId[] enemies = Object.FindObjectsOfType<EnemySaveId>();

        foreach (EnemySaveId enemy in enemies)
        {
            result.Add(new EnemyDataModel
            {
                EnemyId = enemy.Id,
                PositionX = enemy.transform.position.x,
                PositionY = enemy.transform.position.y,
                PositionZ = enemy.transform.position.z
            });
        }

        return result;
    }
}
