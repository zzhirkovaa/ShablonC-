public sealed class GameModeService : IGameModeService
{
    public GameMode CurrentMode { get; private set; } = GameMode.Normal;
    public bool IsPeacefulMode => CurrentMode == GameMode.Peaceful;

    public void SetMode(GameMode mode)
    {
        CurrentMode = mode;
    }
}
