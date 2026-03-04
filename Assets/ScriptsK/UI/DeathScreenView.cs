using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace Player.UI
{
    /// <summary>
    /// Слой представления - экран смерти
    /// </summary>
    public class DeathScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject _deathPanel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private TextMeshProUGUI _messageText;

        private void Start()
        {
            _deathPanel.SetActive(false);
            _restartButton.onClick.AddListener(RestartGame);

            // Подписываемся на смерть игрока
            var player = FindObjectOfType<PlayerHealth>();
            if (player != null)
            {
                player.OnPlayerDied += ShowDeathScreen;
            }
        }

        public void ShowDeathScreen()
        {
            _deathPanel.SetActive(true);

            // Блокируем игру и разблокируем курсор
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnDestroy()
        {
            var player = FindObjectOfType<PlayerHealth>();
            if (player != null)
            {
                player.OnPlayerDied -= ShowDeathScreen;
            }
        }
    }
}