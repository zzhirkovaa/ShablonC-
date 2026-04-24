using UnityEngine;

public sealed class BossAttackState : AbstractBossState
{
    private float _attackTimer;

    public BossAttackState(BossController boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.StopMotion();
        Boss.SetMovementAnimation(false);
        Boss.FacePlayerImmediately();

        if (!Boss.TryStartNormalAttack())
        {
            StateMachine.ChangeState(Boss.AggressionState, "Normal attack was still on cooldown");
            return;
        }

        _attackTimer = Boss.AttackStateDuration;
        Boss.BeginAttackAnimation(false);
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

        StateMachine.ChangeState(Boss.SelectPostAttackState(), "Boss finished normal attack");
    }

    public override void PhysicsUpdate()
    {
        Boss.StopMotion();
        Boss.FaceDirection(Boss.GetDirectionToPlayer(), Time.fixedDeltaTime);
    }
}
