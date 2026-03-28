using UnityEngine;
using Player.Interfaces;

namespace Player.Core
{
    public class CameraLogic : ICameraController
    {
        private readonly Transform _cameraTransform;
        private Transform _targetTransform;

        private readonly float _mouseSensitivity;
        private float _distanceFromTarget;  
        private readonly float _minVerticalAngle;
        private readonly float _maxVerticalAngle;
        private readonly float _smoothTime;

        private readonly float _collisionRadius = 0.3f;
        private readonly float _minDistance = 1.5f;
        private readonly LayerMask _collisionMask;

        private float _currentX = 0f;
        private float _currentY = 0f;
        private Vector3 _velocity = Vector3.zero;
        private float _currentDistance; 

        public CameraLogic(Transform cameraTransform, float sensitivity, float distance,
                          float minAngle, float maxAngle, float smoothTime,
                          LayerMask collisionMask)  
        {
            _cameraTransform = cameraTransform;
            _mouseSensitivity = sensitivity;
            _distanceFromTarget = distance;
            _minVerticalAngle = minAngle;
            _maxVerticalAngle = maxAngle;
            _smoothTime = smoothTime;
            _collisionMask = collisionMask;  

            _currentDistance = distance;
        }

        public void SetTarget(Transform target)
        {
            _targetTransform = target;
        }

        public void SetDistance(float newDistance)
        {
            _distanceFromTarget = newDistance;
        }

        public void HandleMouseInput(Vector2 mouseDelta)
        {
            if (_targetTransform == null) return;

            _currentX += mouseDelta.x * _mouseSensitivity;
            _currentY -= mouseDelta.y * _mouseSensitivity;
            _currentY = Mathf.Clamp(_currentY, _minVerticalAngle, _maxVerticalAngle);
        }

        public void UpdatePosition()
        {
            if (_targetTransform == null) return;

            Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0);
            Vector3 desiredPosition = _targetTransform.position - (rotation * Vector3.forward * _distanceFromTarget);

            float safeDistance = CheckCameraCollision(rotation, _targetTransform.position);

            Vector3 finalPosition = _targetTransform.position - (rotation * Vector3.forward * safeDistance);

            float minCameraHeight = 0.8f;
            if (finalPosition.y < minCameraHeight)
            {
                finalPosition.y = minCameraHeight;
            }

            _cameraTransform.position = Vector3.SmoothDamp(
                _cameraTransform.position,
                finalPosition,
                ref _velocity,
                _smoothTime
            );

            Vector3 lookAtPosition = _targetTransform.position + Vector3.up * 1.5f;
            _cameraTransform.LookAt(lookAtPosition);
        }

        private float CheckCameraCollision(Quaternion rotation, Vector3 targetPosition)
        {
            float desiredDistance = _distanceFromTarget;
            Vector3 direction = rotation * Vector3.forward;

            RaycastHit hit;
            if (Physics.SphereCast(
                targetPosition,           
                _collisionRadius,          
                -direction,                
                out hit,                   
                desiredDistance,            
                _collisionMask             
            ))
            {
                return Mathf.Max(hit.distance - _collisionRadius, _minDistance);
            }

            return desiredDistance;
        }
    }
}