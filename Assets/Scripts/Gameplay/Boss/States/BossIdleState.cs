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

        if (TryEnterHeal())
            return;

        if (Boss.IsPeacefulMode && !Context.WasProvokedByPlayer)
            return;

        if (Context.HasDetectedTarget)
            StateMachine.ChangeState(BossStateType.Aggro, "Player entered boss detection radius");
    }
}
