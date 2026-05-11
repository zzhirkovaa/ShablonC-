using System.Collections.Generic;

public interface IEnemySaveStateReader
{
    IReadOnlyList<EnemyDataModel> Read();
}
