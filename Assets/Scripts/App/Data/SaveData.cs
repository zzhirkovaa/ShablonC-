using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string SceneName;

    public float PlayerPosX;
    public float PlayerPosY;
    public float PlayerPosZ;

    public float PlayerHp;
    public float PlayerAbilityCooldownRemaining;
    public int Score;

    public List<EnemySaveData> Enemies = new();
}

[Serializable]
public class EnemySaveData
{
    public string EnemyId;
    public float PosX;
    public float PosY;
    public float PosZ;
    public float Health;
    public bool IsDead;
}
