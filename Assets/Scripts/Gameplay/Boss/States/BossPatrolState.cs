using UnityEngine;

public sealed class BossPatrolState : AbstractBossState
{
    private Vector3 _patrolPoint;

    public BossPatrolState(BossController boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        _patrolPoint = Boss.CreatePatrolPoint();
        Boss.SetMovementAnimation(true);
        Boss.SetAnimatorSpeed(1f);
    }

    public override void Exit()
    {
        Boss.StopMotion();
    }

    public override void LogicUpdate()
    {
        if (TryEnterDeath() || TryEnterRage())
            return;

        if (Boss.HasDetectedPlayer)
        {
            StateMachine.ChangeState(Boss.AggressionState, "Boss detected player while patrolling");
            return;
        }

        if (Boss.HasReachedPoint(_patrolPoint))
            StateMachine.ChangeState(Boss.IdleState, "Boss reached patrol point");
    }

    public override void PhysicsUpdate()
    {
        Vector3 direction = _patrolPoint - Boss.transform.position;
        direction.y = 0f;
        Boss.FaceDirection(direction.normalized, Time.fixedDeltaTime);
        Boss.MoveTowards(_patrolPoint, Time.fixedDeltaTime);
    }
}
