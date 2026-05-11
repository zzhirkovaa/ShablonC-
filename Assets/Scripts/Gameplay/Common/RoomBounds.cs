using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomBounds : MonoBehaviour, IEnemyMovementBounds
{
    [SerializeField] private Collider _roomCollider;

    private void Reset()
    {
        _roomCollider = GetComponent<Collider>();
    }

    private void Awake()
    {
        if (_roomCollider == null)
            _roomCollider = GetComponent<Collider>();
    }

    public Vector3 ClampPosition(Vector3 position)
    {
        if (_roomCollider == null)
            return position;

        return _roomCollider.ClosestPoint(position);
    }
}