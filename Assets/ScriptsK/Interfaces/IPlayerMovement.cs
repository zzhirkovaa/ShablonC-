using UnityEngine;

namespace Player.Interfaces
{
    public interface IPlayerMovement
    {
        void HandleMovement(Vector2 input, bool isMoving, bool isRunning); 
        void RotateToDirection(Vector3 direction);
        void ApplyGravity();
        Vector3 GetDesiredMoveDirection();
        float GetCurrentSpeed();  
    }
}