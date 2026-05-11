using UnityEngine;

public sealed class BossChaseState : AbstractBossState
{
    public BossChaseState(BossContext context, BossStateMachine stateMachine) : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.DisableDamageHitboxes();

        if (!Context.HasLostTarget)
        {
            float speed = Context.IsTargetHealthLow ? Boss.FinisherChaseSpeed : Boss.ChaseSpeed;
            Boss.MoveToTarget(speed);
        }
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
            StateMachine.ChangeState(BossStateType.Idle, "Peaceful mode suppresses unprovoked chase");
            return;
        }

        if (Context.HasLostTarget)
        {
            if (TryEnterHeal())
                return;

            StateMachine.ChangeState(BossStateType.Idle, "Boss lost the player during chase");
            return;
        }

        if (TryEnterEnrage())
            return;

        if (Context.IsTargetInAttackRange)
        {
            Boss.StopMovement();

            if (Boss.TrySelectReadyAttackState(out BossStateType attackStateType))
            {
                StateMachine.ChangeState(attackStateType, "Boss selected an attack after chase");
            }

            return;
        }

        if (Boss.TrySelectReadyRangedFireAttackState(false, out BossStateType rangedFireStateType))
        {
            Boss.StopMovement();
            FaceTarget(Time.deltaTime);
            StateMachine.ChangeState(rangedFireStateType, "Boss casts fire at distant player during chase");
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
