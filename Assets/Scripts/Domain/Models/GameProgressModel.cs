using System.Collections.Generic;

public sealed class GameProgressModel
{
    public string SceneName { get; set; }
    public PlayerDataModel PlayerData { get; set; }
    public List<EnemyDataModel> Enemies { get; set; } = new();
}
