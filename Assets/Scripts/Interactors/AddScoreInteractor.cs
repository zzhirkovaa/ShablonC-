public sealed class AddScoreInteractor
{
    private readonly IScoreModel _scoreModel;

    public AddScoreInteractor(IScoreModel scoreModel)
    {
        _scoreModel = scoreModel;
    }

    public void Execute(int amount)
    {
        _scoreModel?.AddScore(amount);
    }
}
