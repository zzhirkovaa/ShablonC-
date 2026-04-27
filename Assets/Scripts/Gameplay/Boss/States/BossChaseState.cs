using UnityEngine;

public sealed class BossChaseState : AbstractBossState
{
    public BossChaseState(BossContext context, BossStateMachine stateMachine) : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.DisableDamageHitboxes();
    }

    public override void Exit()
    {
        Boss.StopMovement();
    }

    public override void Tick()
    {
        if (TryEnterDeath())
            return;

        if (Boss.IsPeacefulMode && !Context.WasProvokedByPlayer)
        {
            StateMachine.ChangeState(Boss.IdleState, "Peaceful mode suppresses unprovoked chase");
            return;
        }

        if (Context.HasLostTarget)
        {
            if (TryEnterHeal())
                return;

            StateMachine.ChangeState(Boss.IdleState, "Boss lost the player during chase");
            return;
        }

        if (TryEnterEnrage())
            return;

        if (Context.IsTargetInAttackRange)
        {
            Boss.StopMovement();

            if (Context.CanUseHeavyAttack)
            {
                StateMachine.ChangeState(Boss.HeavyAttackState, "Heavy attack cooldown is ready after chase");
                return;
            }

            if (Context.CanUseAttack)
                StateMachine.ChangeState(Boss.AttackState, "Player reached attack range during chase");

            return;
        }

        float speed = Context.IsTargetHealthLow ? Boss.FinisherChaseSpeed : Boss.ChaseSpeed;
        Boss.MoveToTarget(speed);
    }

    public override void FixedTick()
    {
        FaceTarget(Time.fixedDeltaTime);
    }
}
