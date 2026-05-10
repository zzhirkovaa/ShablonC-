public sealed class BossDeathState : AbstractBossState
{
    public BossDeathState(BossContext context, BossStateMachine stateMachine) : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.BeginDeath();
    }
}
