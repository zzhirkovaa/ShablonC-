public interface IPendingLoadDataService
{
    bool HasPendingData { get; }
    void Set(GameProgressModel data);
    GameProgressModel Consume();
}
