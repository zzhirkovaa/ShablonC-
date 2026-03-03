using UnityEngine;

namespace Player.Interfaces
{
    /// <summary>
    /// Интерфейс для управления внешностью (глаза, материалы)
    /// </summary>
    public interface IPlayerAppearance
    {
        void SetEyeState(string state);
        void UpdateAnimations(float speed);
    }
}