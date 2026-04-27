using UnityEngine;

public class EnemyRoomReference : MonoBehaviour
{
    [SerializeField] private RoomBounds _roomBounds;

    public RoomBounds RoomBounds => _roomBounds;
}