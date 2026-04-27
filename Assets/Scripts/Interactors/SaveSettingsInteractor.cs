public sealed class SaveSettingsInteractor
{
    private readonly ISettingsRepository _settingsRepository;

    public SaveSettingsInteractor(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public void Execute(SettingsModel settings)
    {
        _settingsRepository.Save(settings);
    }
}
