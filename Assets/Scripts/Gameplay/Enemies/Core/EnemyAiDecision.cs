using UnityEngine;

public readonly struct EnemyAiDecision
{
    public EnemyAiDecisionType Type { get; }
    public Vector3 Direction { get; }
    public Vector3 TargetPosition { get; }

    private EnemyAiDecision(EnemyAiDecisionType type, Vector3 direction, Vector3 targetPosition)
    {
        Type = type;
        Direction = direction;
        TargetPosition = targetPosition;
    }

    public static EnemyAiDecision Idle() => new(EnemyAiDecisionType.Idle, Vector3.zero, Vector3.zero);
    public static EnemyAiDecision Attack(Vector3 direction) => new(EnemyAiDecisionType.Attack, direction, Vector3.zero);
    public static EnemyAiDecision Follow(Vector3 direction, Vector3 targetPosition) => new(EnemyAiDecisionType.Follow, direction, targetPosition);
}
