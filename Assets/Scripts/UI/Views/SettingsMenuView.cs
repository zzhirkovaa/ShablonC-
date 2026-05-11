using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuView : MonoBehaviour
{
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Button _backButton;

    public event Action<float> VolumeChanged;
    public event Action BackClicked;

    private void OnEnable()
    {
        _volumeSlider.onValueChanged.AddListener(RaiseVolumeChanged);
        _backButton.onClick.AddListener(RaiseBackClicked);
    }

    private void OnDisable()
    {
        _volumeSlider.onValueChanged.RemoveListener(RaiseVolumeChanged);
        _backButton.onClick.RemoveListener(RaiseBackClicked);
    }

    public void SetVolumeWithoutNotify(float value)
    {
        _volumeSlider.SetValueWithoutNotify(value);
    }

    public void Show()
    {
        _settingsPanel.SetActive(true);
    }

    public void Hide()
    {
        _settingsPanel.SetActive(false);
    }

    private void RaiseVolumeChanged(float value) => VolumeChanged?.Invoke(value);
    private void RaiseBackClicked() => BackClicked?.Invoke();
}