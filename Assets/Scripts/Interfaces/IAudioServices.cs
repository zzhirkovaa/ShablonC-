public interface IAudioService
{
    float Volume { get; }
    void SetVolume(float value);
    float LoadVolume();
}