using System;

public sealed class ScoreSaveStateReader : IScoreSaveStateReader
{
    private readonly IScoreReadOnlyModel _scoreModel;

    public ScoreSaveStateReader(IScoreReadOnlyModel scoreModel)
    {
        _scoreModel = scoreModel;
    }

    public int Read()
    {
        return _scoreModel != null ? Math.Max(0, _scoreModel.CurrentScore) : 0;
    }
}
