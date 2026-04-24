using UnityEngine;

public sealed class BossIdleState : AbstractBossState
{
    private float _idleTimer;

    public BossIdleState(BossController boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        _idleTimer = Boss.IdleDuration;
        Boss.StopMotion();
        Boss.SetMovementAnimation(false);
        Boss.SetAnimatorSpeed(1f);
    }

    public override void LogicUpdate()
    {
        if (TryEnterDeath() || TryEnterRage())
            return;

        if (Boss.HasDetectedPlayer)
        {
            StateMachine.ChangeState(Boss.AggressionState, "Player entered boss detection radius");
            return;
        }

        _idleTimer -= Time.deltaTime;
        if (_idleTimer <= 0f)
            StateMachine.ChangeState(Boss.PatrolState, "Boss idle timer elapsed");
    }
}
