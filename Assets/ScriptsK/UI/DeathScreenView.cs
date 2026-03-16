using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Player.UI
{
    public class DeathScreenView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _deathPanel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private TextMeshProUGUI _messageText;

        [Header("Settings")]
        [SerializeField] private float _delayBeforeShow = 2.5f; 

        private void Start()
        {
            _deathPanel.SetActive(false);
            _restartButton.onClick.AddListener(RestartGame);

            var player = FindObjectOfType<PlayerHealth>();
            if (player != null)
            {
                player.OnPlayerDied += HandlePlayerDied;
            }
        }

        private void HandlePlayerDied()
        {
            StartCoroutine(ShowDeathScreenRoutine());
        }

        private IEnumerator ShowDeathScreenRoutine()
        {
            yield return new WaitForSecondsRealtime(_delayBeforeShow);

            _deathPanel.SetActive(true);

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
                player.OnPlayerDied -= HandlePlayerDied;
            }
        }
    }
}