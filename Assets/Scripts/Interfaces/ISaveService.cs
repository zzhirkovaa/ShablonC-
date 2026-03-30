public interface ISaveService
{
    void SaveGame(SaveData data);
    SaveData LoadGame();
    bool HasSave();
}