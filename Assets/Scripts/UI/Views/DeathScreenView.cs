using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Player.UI
{
    public class DeathScreenView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _deathPanel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private TextMeshProUGUI _messageText;

        public event Action RestartClicked;

        private void OnEnable()
        {
            _restartButton.onClick.AddListener(RaiseRestartClicked);
        }

        private void OnDisable()
        {
            _restartButton.onClick.RemoveListener(RaiseRestartClicked);
        }

        public void HideImmediately()
        {
            _deathPanel.SetActive(false);
        }

        public void ShowAfterDelay(float delay)
        {
            StartCoroutine(ShowRoutine(delay));
        }

        private IEnumerator ShowRoutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            _deathPanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void RaiseRestartClicked()
        {
            RestartClicked?.Invoke();
        }
    }
}