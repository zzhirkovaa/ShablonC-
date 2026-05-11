public sealed class EnemyScoreRewardResolver
{
    private readonly int _defaultMeleeReward;
    private readonly int _defaultRangedReward;
    private readonly int _defaultBossReward;

    public EnemyScoreRewardResolver(
        int defaultMeleeReward = 10,
        int defaultRangedReward = 15,
        int defaultBossReward = 100)
    {
        _defaultMeleeReward = defaultMeleeReward;
        _defaultRangedReward = defaultRangedReward;
        _defaultBossReward = defaultBossReward;
    }

    public int Resolve(EnemyHealth enemyHealth)
    {
        if (enemyHealth == null)
            return 0;

        if (enemyHealth.TryGetComponent(out EnemyScoreReward scoreReward))
            return scoreReward.ScoreReward;

        if (enemyHealth.GetComponent<BossController>() != null)
            return _defaultBossReward;

        if (enemyHealth.GetComponent<EnemyRangedAI>() != null)
            return _defaultRangedReward;

        if (enemyHealth.GetComponent<EnemyAI>() != null)
            return _defaultMeleeReward;

        return 0;
    }
}
