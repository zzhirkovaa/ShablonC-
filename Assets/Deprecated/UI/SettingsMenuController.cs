using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private IAudioService _audioService;

    public void Construct(IAudioService audioService)
    {
        _audioService = audioService;
    }

    private void Start()
    {
        if (_audioService == null)
        {
            Debug.LogError("SettingsMenuController: AudioService не передан.");
            return;
        }

        float currentVolume = _audioService.LoadVolume();
        volumeSlider.value = currentVolume;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnDestroy()
    {
        volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }

    private void OnVolumeChanged(float value)
    {
        _audioService.SetVolume(value);
    }
}