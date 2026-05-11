using System.Threading.Tasks;
using UnityEngine;

public class AudioService : IAudioService
{
    private const string VolumeKey = "MasterVolume";
    private AudioSource _victoryMusicSource;
    private AudioClip _victoryMusicClip;
    private AudioSource _backgroundMusicSource;
    private int _victoryPlaybackVersion;

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

    public void ConfigureVictoryMusic(
        AudioSource audioSource,
        AudioClip victoryClip,
        AudioSource backgroundMusicSource = null)
    {
        _victoryMusicSource = audioSource;
        _victoryMusicClip = victoryClip;
        _backgroundMusicSource = backgroundMusicSource;
    }

    public async void PlayVictoryMusic()
    {
        if (_victoryMusicSource == null || _victoryMusicClip == null)
        {
            Debug.LogWarning("Victory music source or clip is not assigned.");
            return;
        }

        float previousBackgroundVolume = 0f;
        bool hasBackgroundMusic = _backgroundMusicSource != null;
        if (hasBackgroundMusic)
        {
            previousBackgroundVolume = _backgroundMusicSource.volume;
            _backgroundMusicSource.volume = 0f;
        }

        int playbackVersion = ++_victoryPlaybackVersion;
        _victoryMusicSource.clip = _victoryMusicClip;
        _victoryMusicSource.Play();

        int delayMs = Mathf.CeilToInt(Mathf.Max(0f, _victoryMusicClip.length) * 1000f);
        if (delayMs > 0)
            await Task.Delay(delayMs);

        if (!hasBackgroundMusic || playbackVersion != _victoryPlaybackVersion || _backgroundMusicSource == null)
            return;

        _backgroundMusicSource.volume = previousBackgroundVolume;
    }
}
