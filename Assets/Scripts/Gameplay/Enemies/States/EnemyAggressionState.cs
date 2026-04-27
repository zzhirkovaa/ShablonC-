using UnityEngine;

public sealed class EnemyAggressionState : EnemyStateBase
{
    public EnemyAggressionState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Context.SetAnimatorBool("IsRunning", true);
        Context.SetAnimatorBool("IsFleeing", false);
        Context.ResetAnimatorTrigger("Attack");
    }

    public override void Tick()
    {
        if (!Context.HasPlayer)
        {
            StateMachine.ChangeState(Context.IdleState, "Lost player reference during aggression");
            return;
        }

        if (Context.IsPeacefulMode)
        {
            if (Context.ShouldEnterFlee)
            {
                StateMachine.ChangeState(Context.FleeState, Context.GetFleeReasonLabel());
                return;
            }

            StateMachine.ChangeState(Context.IdleState, "Peaceful mode suppresses aggression");
            return;
        }

        if (Context.ShouldEnterFlee)
        {
            StateMachine.ChangeState(Context.FleeState, Context.GetFleeReasonLabel());
            return;
        }

        if (Context.IsPlayerInAttackRange)
        {
            StateMachine.ChangeState(Context.AttackState, "Player reached attack range during aggression");
            return;
        }

        if (!Context.HasDetectedPlayer)
            StateMachine.ChangeState(Context.IdleState, "Player no longer detected during aggression");
    }

    public override void FixedTick()
    {
        Vector3 direction = Context.GetDirectionToPlayer();
        Context.Move(direction, Time.fixedDeltaTime);
        Context.FaceDirection(direction, Time.fixedDeltaTime);
    }
}
