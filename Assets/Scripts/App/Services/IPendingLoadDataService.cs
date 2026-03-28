public interface IPendingLoadDataService
{
    bool HasPendingData { get; }
    void Set(SaveData data);
    SaveData Consume();
}