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
            StateMachine.ChangeState(Boss.IdleState, "Peaceful mode suppresses unprovoked boss aggression");
            return;
        }

        if (Context.HasLostTarget)
        {
            if (TryEnterHeal())
                return;

            StateMachine.ChangeState(Boss.IdleState, "Boss lost the player");
            return;
        }

        if (TryEnterEnrage())
            return;

        if (Context.IsTargetInAttackRange)
        {
            Boss.StopMovement();
            FaceTarget(Time.deltaTime);

            IBossState attackState = Boss.SelectReadyAttackState();
            if (attackState != null)
            {
                StateMachine.ChangeState(attackState, "Boss selected an attack from aggro");
            }

            return;
        }

        IBossState rangedFireState = Boss.SelectReadyRangedFireAttackState(true);
        if (rangedFireState != null)
        {
            Boss.StopMovement();
            FaceTarget(Time.deltaTime);
            StateMachine.ChangeState(rangedFireState, "Boss casts fire before chasing distant player");
            return;
        }

        StateMachine.ChangeState(Boss.ChaseState, "Player is outside attack range");
    }

    public override void FixedTick()
    {
        FaceTarget(Time.fixedDeltaTime);
    }
}
