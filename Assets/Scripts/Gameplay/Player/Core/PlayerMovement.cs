using UnityEngine;
using Player.Interfaces;

namespace Player.Core
{
    public class PlayerMovement : IPlayerMovement
    {
        private const float GRAVITY_FORCE = -9.81f;
        private const float GROUNDED_GRAVITY = -0.5f;

        private readonly Transform _playerTransform;
        private readonly float _walkSpeed;
        private readonly float _runSpeed;  
        private readonly float _rotationSpeed;
        private readonly CharacterController _controller;

        private Vector3 _moveDirection;
        private float _verticalVelocity;
        private float _currentSpeed; 

        public PlayerMovement(Transform transform, CharacterController controller,
                             float walkSpeed, float runSpeed, float rotationSpeed)  
        {
            _playerTransform = transform;
            _controller = controller;
            _walkSpeed = walkSpeed;
            _runSpeed = runSpeed;
            _rotationSpeed = rotationSpeed;
            _verticalVelocity = 0;
            _currentSpeed = 0;
        }

        public void HandleMovement(Vector2 input, bool isMoving, bool isRunning)
        {
            if (!isMoving)
            {
                _moveDirection = Vector3.zero;
                _currentSpeed = 0;
                return;
            }

            float moveSpeed = isRunning ? _runSpeed : _walkSpeed;
            _currentSpeed = moveSpeed;  

            var camera = Camera.main;
            var forward = camera.transform.forward;
            var right = camera.transform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            _moveDirection = (forward * input.y + right * input.x).normalized;
        }

        public void RotateToDirection(Vector3 direction)
        {
            if (direction == Vector3.zero) return;

            var targetRotation = Quaternion.LookRotation(direction);
            _playerTransform.rotation = Quaternion.Slerp(
                _playerTransform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }

        public void ApplyGravity()
        {
            if (_controller.isGrounded)
            {
                _verticalVelocity = GROUNDED_GRAVITY;
            }
            else
            {
                _verticalVelocity += GRAVITY_FORCE * Time.deltaTime;
            }

            var moveWithGravity = _moveDirection * _currentSpeed * Time.deltaTime; 
            moveWithGravity.y = _verticalVelocity * Time.deltaTime;

            _controller.Move(moveWithGravity);
        }

        public Vector3 GetDesiredMoveDirection()
        {
            return _moveDirection;
        }

        public float GetCurrentSpeed() 
        {
            return _currentSpeed;
        }
    }
}