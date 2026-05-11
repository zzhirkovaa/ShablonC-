using System;

public sealed class BossStateFactory
{
    private readonly BossContext _context;
    private readonly BossStateMachine _stateMachine;

    public BossStateFactory(BossContext context, BossStateMachine stateMachine)
    {
        _context = context;
        _stateMachine = stateMachine;
    }

    public IBossState Create(BossStateType stateType)
    {
        return stateType switch
        {
            BossStateType.Idle => new BossIdleState(_context, _stateMachine),
            BossStateType.Aggro => new BossAggroState(_context, _stateMachine),
            BossStateType.Chase => new BossChaseState(_context, _stateMachine),
            BossStateType.Attack => new BossAttackState(_context, _stateMachine),
            BossStateType.HeavyAttack => new BossHeavyAttackState(_context, _stateMachine),
            BossStateType.Enrage => new BossEnrageState(_context, _stateMachine),
            BossStateType.Heal => new BossHealState(_context, _stateMachine),
            BossStateType.Death => new BossDeathState(_context, _stateMachine),
            _ => throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null)
        };
    }
}
