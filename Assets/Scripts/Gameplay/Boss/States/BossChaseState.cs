using UnityEngine;

public sealed class BossChaseState : AbstractBossState
{
    public BossChaseState(BossController boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
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

        if (!Boss.HasDetectedPlayer)
        {
            StateMachine.ChangeState(Boss.IdleState, "Boss lost the player during chase");
            return;
        }

        if (Boss.IsPlayerInStrongAttackRange && Boss.CanUseStrongAttack)
        {
            StateMachine.ChangeState(Boss.StrongAttackState, "Player entered strong attack range during chase");
            return;
        }

        if (Boss.IsPlayerInAttackRange && Boss.CanUseNormalAttack)
        {
            StateMachine.ChangeState(Boss.AttackState, "Player entered normal attack range during chase");
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        Vector3 direction = Boss.GetDirectionToPlayer();
        Boss.FaceDirection(direction, Time.fixedDeltaTime);
        Boss.Move(direction, Time.fixedDeltaTime);
    }
}
