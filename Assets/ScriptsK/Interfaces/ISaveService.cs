using UnityEditor.Overlays;

public interface ISaveService
{
    void SaveGame(string sceneName, UnityEngine.Vector3 playerPosition);
    SaveData LoadGame();
    bool HasSave();
}