public interface IScoreModel : IScoreReadOnlyModel
{
    void AddScore(int amount);
    void SetScore(int score);
    void Reset();
}
