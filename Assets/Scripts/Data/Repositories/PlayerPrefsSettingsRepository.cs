using UnityEngine;

public sealed class PlayerPrefsSettingsRepository : ISettingsRepository
{
    private const string VolumeKey = "MasterVolume";

    public SettingsModel Load()
    {
        return new SettingsModel
        {
            Volume = PlayerPrefs.GetFloat(VolumeKey, 1f)
        };
    }

    public void Save(SettingsModel settings)
    {
        PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(settings.Volume));
        PlayerPrefs.Save();
    }
}
