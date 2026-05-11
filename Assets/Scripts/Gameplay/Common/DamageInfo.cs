using UnityEngine;

public enum DamageType { Physical, Magical }

public struct DamageInfo
{
    public float Amount;
    public DamageType Type;
    public GameObject Source;

    public DamageInfo(float amount, DamageType type, GameObject source = null)
    {
        Amount = amount;
        Type = type;
        Source = source;
    }
}
