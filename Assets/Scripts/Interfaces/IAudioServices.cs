public interface IAudioService
{
    float Volume { get; }
    void SetVolume(float value);
    float LoadVolume();
    void ConfigureVictoryMusic(
        UnityEngine.AudioSource audioSource,
        UnityEngine.AudioClip victoryClip,
        UnityEngine.AudioSource backgroundMusicSource = null);
    void PlayVictoryMusic();
}
