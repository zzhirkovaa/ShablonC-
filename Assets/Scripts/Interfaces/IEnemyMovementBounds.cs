using UnityEngine;

public interface IEnemyMovementBounds
{
    Vector3 ClampPosition(Vector3 position);
}