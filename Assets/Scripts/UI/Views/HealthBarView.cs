using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Player.Interfaces;

namespace Player.UI
{
    public class HealthBarView : MonoBehaviour, IHealthBar
    {
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private TextMeshProUGUI _healthText;

        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            float percentage = maxHealth <= 0f ? 0f : currentHealth / maxHealth;
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
    }
}