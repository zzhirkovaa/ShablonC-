public sealed class PendingLoadDataService : IPendingLoadDataService
{
    private SaveData _pendingData;

    public bool HasPendingData => _pendingData != null;

    public void Set(SaveData data)
    {
        _pendingData = data;
    }

    public SaveData Consume()
    {
        SaveData result = _pendingData;
        _pendingData = null;
        return result;
    }
}