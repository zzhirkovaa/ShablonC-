using UnityEngine;

public class BossEnrageState : AbstractBossState
{
    private float _timer;

    public BossEnrageState(BossContext context, BossStateMachine stateMachine) : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.DisableDamageHitboxes();
        Boss.StopMovement();
        Boss.EnterPhaseTwo();
        Boss.BeginEnrageAnimation();
        _timer = Boss.EnrageDuration;
    }

    public override void Tick()
    {
        if (TryEnterDeath())
            return;

        _timer -= Time.deltaTime;
        if (_timer > 0f)
            return;

        StateMachine.ChangeState(Boss.SelectMovementOrIdleState(), "Boss enrage transition finished");
    }

    public override void FixedTick()
    {
        Boss.StopMovement();
    }
}
