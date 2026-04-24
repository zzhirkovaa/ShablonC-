public sealed class BossDeathState : AbstractBossState
{
    public BossDeathState(BossController boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.CancelPendingAttack();
        Boss.StopMotion();
        Boss.BeginDeathAnimation();
    }
}
