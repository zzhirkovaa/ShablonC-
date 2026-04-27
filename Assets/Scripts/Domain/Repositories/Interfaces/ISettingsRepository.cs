public interface ISettingsRepository
{
    SettingsModel Load();
    void Save(SettingsModel settings);
}
