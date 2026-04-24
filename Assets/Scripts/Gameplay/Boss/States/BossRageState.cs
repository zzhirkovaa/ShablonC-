using UnityEngine;

public sealed class BossRageState : AbstractBossState
{
    private float _rageTimer;

    public BossRageState(BossController boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.CompleteRageTransition();
        Boss.StopMotion();
        Boss.SetMovementAnimation(false);
        Boss.BeginRageAnimation();
        _rageTimer = Boss.RageStateDuration;
    }

    public override void Exit()
    {
        Boss.EndRageAnimation();
    }

    public override void LogicUpdate()
    {
        if (TryEnterDeath())
            return;

        _rageTimer -= Time.deltaTime;
        if (_rageTimer > 0f)
            return;

        StateMachine.ChangeState(Boss.SelectPostRageState(), "Boss rage transition finished");
    }

    public override void PhysicsUpdate()
    {
        Boss.StopMotion();
    }
}
