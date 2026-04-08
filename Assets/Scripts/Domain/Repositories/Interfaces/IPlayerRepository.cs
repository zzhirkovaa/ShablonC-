public interface IPlayerRepository
{
    PlayerDataModel Load();
    void Save(PlayerDataModel playerData);
}
