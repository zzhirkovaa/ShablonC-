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

        if (Context.HasLostTarget)
        {
            StateMachine.ChangeState(Boss.IdleState, "Boss lost the player");
            return;
        }

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

        Boss.MoveToTarget();
    }

    public override void FixedTick()
    {
        FaceTarget(Time.fixedDeltaTime);
    }
}
