using UnityEngine;

public sealed class EnemyMeleeAttackState : EnemyAttackStateBase
{
    private readonly EnemyCombat _combat;

    public EnemyMeleeAttackState(EnemyContext context, EnemyStateMachine stateMachine, EnemyCombat combat)
        : base(context, stateMachine)
    {
        _combat = combat;
    }

    protected override bool CanAttack()
    {
        return _combat != null && _combat.CanAttack(Time.time);
    }

    protected override void CommitAttack()
    {
        _combat?.ResetCooldownAfterTrigger(Time.time);
    }
}
