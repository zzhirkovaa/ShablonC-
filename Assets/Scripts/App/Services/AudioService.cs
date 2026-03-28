using UnityEngine;

public class AudioService : IAudioService
{
    private const string VolumeKey = "MasterVolume";

    public float Volume => AudioListener.volume;

    public void SetVolume(float value)
    {
        float clampedValue = Mathf.Clamp01(value);
        AudioListener.volume = clampedValue;

        PlayerPrefs.SetFloat(VolumeKey, clampedValue);
        PlayerPrefs.Save();
    }

    public float LoadVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        AudioListener.volume = savedVolume;
        return savedVolume;
    }
}