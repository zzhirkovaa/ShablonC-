public sealed class PendingLoadDataService : IPendingLoadDataService
{
    private GameProgressModel _pendingData;

    public bool HasPendingData => _pendingData != null;

    public void Set(GameProgressModel data)
    {
        _pendingData = data;
    }

    public GameProgressModel Consume()
    {
        GameProgressModel result = _pendingData;
        _pendingData = null;
        return result;
    }
}
