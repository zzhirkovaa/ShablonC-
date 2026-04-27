using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _objectToHideOnSettings;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _playPeaceModeButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _exitButton;

    public event Action PlayClicked;
    public event Action PlayPeaceModeClicked;
    public event Action SettingsClicked;
    public event Action ExitClicked;

    private void OnEnable()
    {
        BindMissingButtons();

        if (_playButton != null)
            _playButton.onClick.AddListener(RaisePlayClicked);

        if (_playPeaceModeButton != null)
            _playPeaceModeButton.onClick.AddListener(RaisePlayPeaceModeClicked);

        if (_settingsButton != null)
            _settingsButton.onClick.AddListener(RaiseSettingsClicked);

        if (_exitButton != null)
            _exitButton.onClick.AddListener(RaiseExitClicked);
    }

    private void OnDisable()
    {
        if (_playButton != null)
            _playButton.onClick.RemoveListener(RaisePlayClicked);

        if (_playPeaceModeButton != null)
            _playPeaceModeButton.onClick.RemoveListener(RaisePlayPeaceModeClicked);

        if (_settingsButton != null)
            _settingsButton.onClick.RemoveListener(RaiseSettingsClicked);

        if (_exitButton != null)
            _exitButton.onClick.RemoveListener(RaiseExitClicked);
    }

    public void Show()
    {
        if (_mainMenuPanel != null)
            _mainMenuPanel.SetActive(true);

        if (_objectToHideOnSettings != null)
            _objectToHideOnSettings.SetActive(true);
    }

    public void Hide()
    {
        if (_mainMenuPanel != null)
            _mainMenuPanel.SetActive(false);

        if (_objectToHideOnSettings != null)
            _objectToHideOnSettings.SetActive(false);
    }

    private void RaisePlayClicked() => PlayClicked?.Invoke();
    private void RaisePlayPeaceModeClicked() => PlayPeaceModeClicked?.Invoke();
    private void RaiseSettingsClicked() => SettingsClicked?.Invoke();
    private void RaiseExitClicked() => ExitClicked?.Invoke();

    private void BindMissingButtons()
    {
        if (_playPeaceModeButton != null)
            return;

        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            if (button.name == "Play peace mode")
            {
                _playPeaceModeButton = button;
                return;
            }
        }

        foreach (Button button in FindObjectsOfType<Button>(true))
        {
            if (button.name == "Play peace mode")
            {
                _playPeaceModeButton = button;
                return;
            }
        }
    }
}
