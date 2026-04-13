using UnityEngine;

public enum DamageType { Physical, Magical }

public struct DamageInfo
{
    public float Amount;
    public DamageType Type;

    public DamageInfo(float amount, DamageType type)
    {
        Amount = amount;
        Type = type;
    }
}