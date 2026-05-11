using System;

namespace Ui.Settings
{
    public sealed class SettingsController : IDisposable
    {
        private readonly SettingsModel _model;
        private readonly SettingsMenuView _view;
        private readonly LoadSettingsInteractor _loadSettingsInteractor;
        private readonly SaveSettingsInteractor _saveSettingsInteractor;
        private readonly IAudioService _audioService;

        public SettingsController(
            SettingsModel model,
            SettingsMenuView view,
            LoadSettingsInteractor loadSettingsInteractor,
            SaveSettingsInteractor saveSettingsInteractor,
            IAudioService audioService)
        {
            _model = model;
            _view = view;
            _loadSettingsInteractor = loadSettingsInteractor;
            _saveSettingsInteractor = saveSettingsInteractor;
            _audioService = audioService;

            _view.VolumeChanged += OnVolumeChanged;

            SettingsModel persistedSettings = _loadSettingsInteractor.Execute();
            _model.Volume = persistedSettings.Volume;

            _audioService.SetVolume(_model.Volume);
            _view.SetVolumeWithoutNotify(_model.Volume);
        }

        public void Dispose()
        {
            _view.VolumeChanged -= OnVolumeChanged;
        }

        private void OnVolumeChanged(float value)
        {
            _model.Volume = value;
            _audioService.SetVolume(_model.Volume);
            _saveSettingsInteractor.Execute(_model);
        }
    }
}
