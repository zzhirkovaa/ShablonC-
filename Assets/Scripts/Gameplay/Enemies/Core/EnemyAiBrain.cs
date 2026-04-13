using UnityEngine;

public sealed class EnemyAiBrain
{
    private readonly float _detectionRadius;
    private readonly float _attackRange;
    private readonly float _moveSpeed;

    public EnemyAiBrain(float detectionRadius, float attackRange, float moveSpeed)
    {
        _detectionRadius = detectionRadius;
        _attackRange = attackRange;
        _moveSpeed = moveSpeed;
    }

    public EnemyAiDecision Evaluate(
        Vector3 enemyPosition,
        Vector3 playerPosition,
        IEnemyMovementBounds movementBounds)
    {
        float distance = Vector3.Distance(enemyPosition, playerPosition);
        if (distance <= _attackRange)
        {
            Vector3 lookDirection = playerPosition - enemyPosition;
            lookDirection.y = 0f;
            return EnemyAiDecision.Attack(lookDirection);
        }

        if (distance <= _detectionRadius)
        {
            Vector3 direction = (playerPosition - enemyPosition).normalized;
            direction.y = 0f;

            Vector3 nextPosition = enemyPosition + direction * _moveSpeed * Time.deltaTime;
            if (movementBounds != null)
                nextPosition = movementBounds.ClampPosition(nextPosition);

            return EnemyAiDecision.Follow(direction, nextPosition);
        }

        return EnemyAiDecision.Idle();
    }
}
