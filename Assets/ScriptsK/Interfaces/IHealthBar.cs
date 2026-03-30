using UnityEngine;

namespace Player.Interfaces
{
    /// <summary>
    /// Интерфейс для отображения здоровья (п. 3.1.1 лекции)
    /// </summary>
    public interface IHealthBar
    {
        void UpdateHealth(float currentHealth, float maxHealth);
        void Show();
        void Hide();
    }
}