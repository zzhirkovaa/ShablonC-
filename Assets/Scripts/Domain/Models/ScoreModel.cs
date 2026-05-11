using System;

public sealed class ScoreModel : IScoreModel
{
    private int _currentScore;

    public int CurrentScore => _currentScore;

    public event Action<int> ScoreChanged;

    public void AddScore(int amount)
    {
        if (amount <= 0)
            return;

        SetScore(_currentScore + amount);
    }

    public void SetScore(int score)
    {
        int sanitizedScore = Math.Max(0, score);
        if (_currentScore == sanitizedScore)
            return;

        _currentScore = sanitizedScore;
        ScoreChanged?.Invoke(_currentScore);
    }

    public void Reset()
    {
        SetScore(0);
    }
}
