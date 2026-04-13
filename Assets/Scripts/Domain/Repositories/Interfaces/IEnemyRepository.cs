using System.Collections.Generic;

public interface IEnemyRepository
{
    IReadOnlyList<EnemyDataModel> Load();
    void Save(IReadOnlyList<EnemyDataModel> enemies);
}
