using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _exitButton;

    public event Action PlayClicked;
    public event Action SettingsClicked;
    public event Action ExitClicked;

    private void OnEnable()
    {
        _playButton.onClick.AddListener(RaisePlayClicked);
        _settingsButton.onClick.AddListener(RaiseSettingsClicked);
        _exitButton.onClick.AddListener(RaiseExitClicked);
    }

    private void OnDisable()
    {
        _playButton.onClick.RemoveListener(RaisePlayClicked);
        _settingsButton.onClick.RemoveListener(RaiseSettingsClicked);
        _exitButton.onClick.RemoveListener(RaiseExitClicked);
    }

    public void Show()
    {
        _mainMenuPanel.SetActive(true);
    }

    public void Hide()
    {
        _mainMenuPanel.SetActive(false);
    }

    private void RaisePlayClicked() => PlayClicked?.Invoke();
    private void RaiseSettingsClicked() => SettingsClicked?.Invoke();
    private void RaiseExitClicked() => ExitClicked?.Invoke();
}