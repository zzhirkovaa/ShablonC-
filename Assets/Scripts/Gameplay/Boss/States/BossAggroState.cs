using UnityEngine;

public class BossAggroState : AbstractBossState
{
    public BossAggroState(BossContext context, BossStateMachine stateMachine) : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.DisableDamageHitboxes();
    }

    public override void Tick()
    {
        if (TryEnterDeath())
            return;

        if (Boss.IsPeacefulMode && !Context.WasProvokedByPlayer)
        {
            StateMachine.ChangeState(BossStateType.Idle, "Peaceful mode suppresses unprovoked boss aggression");
            return;
        }

        if (Context.HasLostTarget)
        {
            if (TryEnterHeal())
                return;

            StateMachine.ChangeState(BossStateType.Idle, "Boss lost the player");
            return;
        }

        if (TryEnterEnrage())
            return;

        if (Context.IsTargetInAttackRange)
        {
            Boss.StopMovement();
            FaceTarget(Time.deltaTime);

            if (Boss.TrySelectReadyAttackState(out BossStateType attackStateType))
            {
                StateMachine.ChangeState(attackStateType, "Boss selected an attack from aggro");
            }

            return;
        }

        if (Boss.TrySelectReadyRangedFireAttackState(true, out BossStateType rangedFireStateType))
        {
            Boss.StopMovement();
            FaceTarget(Time.deltaTime);
            StateMachine.ChangeState(rangedFireStateType, "Boss casts fire before chasing distant player");
            return;
        }

        StateMachine.ChangeState(BossStateType.Chase, "Player is outside attack range");
    }

    public override void FixedTick()
    {
        FaceTarget(Time.fixedDeltaTime);
    }
}
