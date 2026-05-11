using System;

public sealed class ScoreboardUiController : IDisposable
{
    private readonly IScoreReadOnlyModel _scoreModel;
    private readonly IScoreboardView _view;

    public ScoreboardUiController(IScoreReadOnlyModel scoreModel, IScoreboardView view)
    {
        _scoreModel = scoreModel;
        _view = view;

        if (_scoreModel != null)
            _scoreModel.ScoreChanged += OnScoreChanged;

        _view?.Show();
        _view?.SetScore(_scoreModel != null ? _scoreModel.CurrentScore : 0);
    }

    public void Dispose()
    {
        if (_scoreModel != null)
            _scoreModel.ScoreChanged -= OnScoreChanged;
    }

    private void OnScoreChanged(int score)
    {
        _view?.SetScore(score);
    }
}
