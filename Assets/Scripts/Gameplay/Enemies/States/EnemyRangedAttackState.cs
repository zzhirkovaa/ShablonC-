public sealed class EnemyRangedAttackState : EnemyAttackStateBase
{
    private readonly EnemyRangedCombat _combat;

    public EnemyRangedAttackState(EnemyContext context, EnemyStateMachine stateMachine, EnemyRangedCombat combat)
        : base(context, stateMachine)
    {
        _combat = combat;
    }

    protected override bool CanAttack()
    {
        return _combat != null && _combat.CanAttack;
    }

    protected override void CommitAttack()
    {
        _combat?.ResetCooldown();
    }
}
