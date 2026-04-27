public sealed class LoadSettingsInteractor
{
    private readonly ISettingsRepository _settingsRepository;

    public LoadSettingsInteractor(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public SettingsModel Execute()
    {
        return _settingsRepository.Load();
    }
}
