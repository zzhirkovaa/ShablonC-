using System;

public interface IScoreReadOnlyModel
{
    int CurrentScore { get; }

    event Action<int> ScoreChanged;
}
