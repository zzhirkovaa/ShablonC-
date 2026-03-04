using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Player.Interfaces;

namespace Player.UI
{
    /// <summary>
    /// Слой представления - отвечает ТОЛЬКО за отображение (п. 3.1.2)
    /// Никакой логики здоровья здесь нет!
    /// </summary>
    public class HealthBarView : MonoBehaviour, IHealthBar
    {
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private TextMeshProUGUI _healthText;

        private void Start()
        {
            // Подписываемся на события игрока через HealthSystem
            var player = FindObjectOfType<PlayerHealth>();
            if (player != null)
            {
                player.OnHealthChanged += UpdateHealth;
            }
        }

        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            // Только визуальное отображение
            float percentage = currentHealth / maxHealth;
            _healthSlider.value = percentage;
            _healthText.text = $"{Mathf.Ceil(currentHealth)}/{maxHealth}";
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            var player = FindObjectOfType<PlayerHealth>();
            if (player != null)
            {
                player.OnHealthChanged -= UpdateHealth;
            }
        }
    }
}