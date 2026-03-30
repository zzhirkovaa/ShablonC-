using System;

public sealed class SettingsMenuUiController : IDisposable
{
    private readonly SettingsMenuView _view;
    private readonly IAudioService _audioService;

    public SettingsMenuUiController(SettingsMenuView view, IAudioService audioService)
    {
        _view = view;
        _audioService = audioService;

        _view.VolumeChanged += OnVolumeChanged;
        _view.SetVolumeWithoutNotify(_audioService.Volume);
    }

    public void Dispose()
    {
        _view.VolumeChanged -= OnVolumeChanged;
    }

    private void OnVolumeChanged(float value)
    {
        _audioService.SetVolume(value);
    }
}