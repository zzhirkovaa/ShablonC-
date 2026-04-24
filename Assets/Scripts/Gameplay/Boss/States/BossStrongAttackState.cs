using UnityEngine;

public sealed class BossStrongAttackState : AbstractBossState
{
    private float _attackTimer;

    public BossStrongAttackState(BossController boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.StopMotion();
        Boss.SetMovementAnimation(false);
        Boss.FacePlayerImmediately();

        if (!Boss.TryStartStrongAttack())
        {
            StateMachine.ChangeState(Boss.AggressionState, "Strong attack was still on cooldown");
            return;
        }

        _attackTimer = Boss.StrongAttackStateDuration;
        Boss.BeginAttackAnimation(true);
    }

    public override void Exit()
    {
        Boss.SetAnimatorSpeed(1f);
        Boss.CancelPendingAttack();
    }

    public override void LogicUpdate()
    {
        if (TryEnterDeath() || TryEnterRage())
            return;

        _attackTimer -= Time.deltaTime;
        if (_attackTimer > 0f)
            return;

        StateMachine.ChangeState(Boss.SelectPostAttackState(), "Boss finished strong attack");
    }

    public override void PhysicsUpdate()
    {
        Boss.StopMotion();
        Boss.FaceDirection(Boss.GetDirectionToPlayer(), Time.fixedDeltaTime);
    }
}
