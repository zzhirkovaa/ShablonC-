using UnityEngine;

namespace Player.Interfaces
{
    /// <summary>
    /// Интерфейс для управления камерой (п. 3.1.1)
    /// </summary>
    public interface ICameraController
    {
        void HandleMouseInput(Vector2 mouseDelta);
        void SetTarget(Transform target);
        void UpdatePosition();
    }
}