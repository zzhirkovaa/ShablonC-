using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuView : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenuPanel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _mainMenuButton;

    public event Action ResumeClicked;
    public event Action SaveClicked;
    public event Action LoadClicked;
    public event Action MainMenuClicked;

    private void OnEnable()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(RaiseResumeClicked);

        if (_saveButton != null)
            _saveButton.onClick.AddListener(RaiseSaveClicked);

        if (_loadButton != null)
            _loadButton.onClick.AddListener(RaiseLoadClicked);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(RaiseMainMenuClicked);
    }

    private void OnDisable()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.RemoveListener(RaiseResumeClicked);

        if (_saveButton != null)
            _saveButton.onClick.RemoveListener(RaiseSaveClicked);

        if (_loadButton != null)
            _loadButton.onClick.RemoveListener(RaiseLoadClicked);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.RemoveListener(RaiseMainMenuClicked);
    }

    public void Show()
    {
        if (_pauseMenuPanel != null)
            _pauseMenuPanel.SetActive(true);
    }

    public void Hide()
    {
        if (_pauseMenuPanel != null)
            _pauseMenuPanel.SetActive(false);
    }

    private void RaiseResumeClicked() => ResumeClicked?.Invoke();
    private void RaiseSaveClicked() => SaveClicked?.Invoke();
    private void RaiseLoadClicked() => LoadClicked?.Invoke();
    private void RaiseMainMenuClicked() => MainMenuClicked?.Invoke();
}
