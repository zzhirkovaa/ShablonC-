using UnityEngine;

namespace Player.Interfaces
{
    public interface IPlayerView
    {
        Quaternion Rotation { get; }
        void SetRotation(Quaternion rotation);
        void TriggerPunch();
    }
}
