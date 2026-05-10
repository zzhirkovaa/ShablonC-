using UnityEngine;

public class EnemyRoomReference : MonoBehaviour
{
    [SerializeField] private RoomBounds _roomBounds;

    public RoomBounds RoomBounds => _roomBounds;

    public void SetRoomBounds(RoomBounds roomBounds)
    {
        _roomBounds = roomBounds;
    }
}
