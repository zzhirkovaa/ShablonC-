using System.IO;
using UnityEngine;

public class SaveService : ISaveService
{
    private readonly string _savePath;

    public SaveService()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    public void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_savePath, json);

        Debug.Log($"Игра сохранена: {_savePath}");
    }

    public SaveData LoadGame()
    {
        if (!File.Exists(_savePath))
        {
            Debug.LogWarning("Сохранение не найдено.");
            return null;
        }

        string json = File.ReadAllText(_savePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public bool HasSave()
    {
        return File.Exists(_savePath);
    }
}