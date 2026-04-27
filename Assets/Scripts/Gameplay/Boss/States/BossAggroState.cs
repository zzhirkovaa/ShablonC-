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

            if (Context.CanUseHeavyAttack)
            {
                StateMachine.ChangeState(Boss.HeavyAttackState, "Heavy attack cooldown is ready");
                return;
            }

            if (Context.CanUseAttack)
                StateMachine.ChangeState(Boss.AttackState, "Player is in attack range");

            return;
        }

        StateMachine.ChangeState(Boss.ChaseState, "Player is outside attack range");
    }

    public override void FixedTick()
    {
        FaceTarget(Time.fixedDeltaTime);
    }
}
