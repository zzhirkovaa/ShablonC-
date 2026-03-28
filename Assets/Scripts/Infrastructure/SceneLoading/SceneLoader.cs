using UnityEngine.SceneManagement;

public sealed class SceneLoader : ISceneLoader
{
    public string CurrentSceneName => SceneManager.GetActiveScene().name;

    public void Load(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ReloadCurrent()
    {
        SceneManager.LoadScene(CurrentSceneName);
    }
}