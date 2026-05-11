public sealed class ScoreSaveStateWriter : IScoreSaveStateWriter
{
    private readonly IScoreModel _scoreModel;

    public ScoreSaveStateWriter(IScoreModel scoreModel)
    {
        _scoreModel = scoreModel;
    }

    public void Apply(int score)
    {
        _scoreModel?.SetScore(score);
    }
}
