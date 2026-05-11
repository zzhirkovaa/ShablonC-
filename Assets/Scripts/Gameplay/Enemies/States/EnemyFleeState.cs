using UnityEngine;

public sealed class EnemyFleeState : EnemyStateBase
{
    private float _fleeTimer;

    public EnemyFleeState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        _fleeTimer = Context.FleeDuration;
        Context.EnterFlee();
        Context.SetAnimatorBool("IsRunning", true);
        Context.SetAnimatorBool("IsFleeing", true);
        Context.ResetAnimatorTrigger("Attack");
    }

    public override void Exit()
    {
        Context.SetAnimatorBool("IsFleeing", false);
        Context.ClearFleeRequest();
    }

    public override void Tick()
    {
        if (!Context.HasPlayer)
        {
            StateMachine.ChangeState(Context.IdleState, "Lost player reference during flee");
            return;
        }

        _fleeTimer -= Time.deltaTime;

        bool fleeTimeExpired = _fleeTimer <= 0f;
        bool reachedSafety = Context.HasReachedFleeSafety;

        if (Context.IsPeacefulMode && (Context.ShouldFleeInPeacefulMode || !reachedSafety))
            return;

        if (reachedSafety || fleeTimeExpired)
            StateMachine.ChangeState(GetNextStateAfterFlee(), GetExitReason(fleeTimeExpired, reachedSafety));
    }

    public override void FixedTick()
    {
        Vector3 direction = Context.GetDirectionAwayFromPlayer();
        Context.Move(direction, Time.fixedDeltaTime);
        Context.FaceDirection(direction, Time.fixedDeltaTime);
    }

    private IEnemyState GetNextStateAfterFlee()
    {
        if (Context.IsPeacefulMode)
            return Context.IdleState;

        if (!Context.HasDetectedPlayer)
            return Context.IdleState;

        if (Context.IsPlayerInAttackRange)
            return Context.AttackState;

        return Context.AggressionState;
    }

    private static string GetExitReason(bool fleeTimeExpired, bool reachedSafety)
    {
        if (reachedSafety && fleeTimeExpired)
            return "Flee finished: timer expired and safe distance reached";

        if (reachedSafety)
            return "Flee finished: safe distance reached";

        return "Flee finished: timer expired";
    }
}
