using System.Collections.Generic;

public interface IEnemySaveStateWriter
{
    void Apply(IReadOnlyList<EnemyDataModel> enemies);
}
