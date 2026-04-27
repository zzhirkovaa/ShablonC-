public interface IGameModeService
{
    GameMode CurrentMode { get; }
    bool IsPeacefulMode { get; }
    void SetMode(GameMode mode);
}
