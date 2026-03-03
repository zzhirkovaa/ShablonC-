using UnityEngine;
using Player.Interfaces;
using Player.Helpers;

namespace Player.Core
{
    /// <summary>
    /// Ћогика управлени€ внешностью персонажа
    /// </summary>
    public class PlayerAppearance : IPlayerAppearance
    {
        private readonly Animator _animator;
        private readonly Renderer[] _characterMaterials;
        private readonly float _animationSmoothTime;

        private const string BLEND_PARAM = "Blend";
        private const string INPUT_X_PARAM = "InputX";
        private const string INPUT_Z_PARAM = "InputZ";

        public PlayerAppearance(Animator animator, Renderer[] materials, float smoothTime)
        {
            _animator = animator;
            _characterMaterials = materials;
            _animationSmoothTime = smoothTime;
        }

        public void SetEyeState(string state)
        {
            Vector2 offset = GetEyeOffsetForState(state);

            foreach (var renderer in _characterMaterials)
            {
                if (renderer.transform.CompareTag("PlayerEyes"))
                {
                    renderer.material.SetTextureOffset("_MainTex", offset);
                }
            }
        }

        public void UpdateAnimations(float speed)
        {
            _animator.SetFloat(BLEND_PARAM, speed, _animationSmoothTime, Time.deltaTime);

            // ћожно добавить обновление InputX/InputZ если нужно
        }

        private Vector2 GetEyeOffsetForState(string state)
        {
            return state switch
            {
                "normal" => new Vector2(0, 0),
                "angry" => new Vector2(0.66f, 0),
                "happy" => new Vector2(0.33f, 0),
                "dead" => new Vector2(0.33f, 0.66f),
                _ => new Vector2(0, 0)
            };
        }
    }
}