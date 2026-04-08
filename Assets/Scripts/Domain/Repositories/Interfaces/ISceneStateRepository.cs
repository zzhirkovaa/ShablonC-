public interface ISceneStateRepository
{
    string Load();
    void Save(string sceneName);
}
