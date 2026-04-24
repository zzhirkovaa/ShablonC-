using UnityEngine;

public sealed class BossAggressionState : AbstractBossState
{
    private float _decisionDelayRemaining;

    public BossAggressionState(BossController boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        _decisionDelayRemaining = Boss.AggressionDecisionDelay;
        Boss.StopMotion();
        Boss.SetMovementAnimation(false);
        Boss.SetAnimatorSpeed(1f);
        Boss.FacePlayerImmediately();
    }

    public override void LogicUpdate()
    {
        if (TryEnterDeath() || TryEnterRage())
            return;

        if (!Boss.HasDetectedPlayer)
        {
            StateMachine.ChangeState(Boss.IdleState, "Boss lost sight of the player");
            return;
        }

        if (Boss.IsPlayerInStrongAttackRange && Boss.CanUseStrongAttack)
        {
            StateMachine.ChangeState(Boss.StrongAttackState, "Player entered strong attack range");
            return;
        }

        if (Boss.IsPlayerInAttackRange && Boss.CanUseNormalAttack)
        {
            StateMachine.ChangeState(Boss.AttackState, "Player entered normal attack range");
            return;
        }

        _decisionDelayRemaining -= Time.deltaTime;
        if (_decisionDelayRemaining <= 0f)
            StateMachine.ChangeState(Boss.ChaseState, "Boss needs to close distance to the player");
    }
}
