public sealed class BossIdleState : AbstractBossState
{
    public BossIdleState(BossContext context, BossStateMachine stateMachine) : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.StopMovement();
        Boss.DisableDamageHitboxes();
    }

    public override void Tick()
    {
        if (TryEnterDeath())
            return;

        if (Boss.IsPeacefulMode)
            return;

        if (Context.HasDetectedTarget)
            StateMachine.ChangeState(Boss.AggroState, "Player entered boss detection radius");
    }
}
