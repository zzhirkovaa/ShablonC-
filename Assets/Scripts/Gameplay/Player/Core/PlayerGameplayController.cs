using UnityEngine;
using Player.Interfaces;

namespace Player.Core
{
    // MVC Controller: coordinates player input, model state and gameplay services.
    public sealed class PlayerGameplayController
    {
        private readonly PlayerModel _model;
        private readonly IPlayerView _view;
        private readonly IPlayerInputService _inputService;
        private readonly ICameraYawProvider _cameraYawProvider;
        private readonly IPlayerMovement _movement;
        private readonly IPlayerAppearance _appearance;
        private readonly float _rotationSpeed;
        private readonly float _startAnimThreshold;

        public PlayerGameplayController(
            PlayerModel model,
            IPlayerView view,
            IPlayerInputService inputService,
            ICameraYawProvider cameraYawProvider,
            IPlayerMovement movement,
            IPlayerAppearance appearance,
            float rotationSpeed,
            float startAnimThreshold)
        {
            _model = model;
            _view = view;
            _inputService = inputService;
            _cameraYawProvider = cameraYawProvider;
            _movement = movement;
            _appearance = appearance;
            _rotationSpeed = rotationSpeed;
            _startAnimThreshold = startAnimThreshold;
        }

        public void Tick()
        {
            Vector2 inputVector = _inputService.GetMoveInput();
            bool isRunning = _inputService.IsRunPressed();

            _model.UpdateInput(inputVector, isRunning, _startAnimThreshold);

            if (_model.IsMoving && _cameraYawProvider != null)
            {
                float targetYaw = _cameraYawProvider.GetYaw();
                Quaternion targetRotation = Quaternion.Euler(0f, targetYaw, 0f);
                Quaternion smoothedRotation = Quaternion.Slerp(
                    _view.Rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime);

                _view.SetRotation(smoothedRotation);
            }

            _movement.HandleMovement(_model.MoveInput, _model.IsMoving, _model.IsRunning);
            _movement.ApplyGravity();
            _appearance.UpdateAnimations(_model.InputMagnitude);

            if (_inputService.IsPunchPressedThisFrame())
                _view.TriggerPunch();
        }
    }
}
