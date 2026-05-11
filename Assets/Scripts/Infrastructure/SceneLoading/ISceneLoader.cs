public interface ISceneLoader
{
    string CurrentSceneName { get; }
    void Load(string sceneName);
    void ReloadCurrent();
}