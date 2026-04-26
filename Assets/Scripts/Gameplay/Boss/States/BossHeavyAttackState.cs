using UnityEngine;

public class BossHeavyAttackState : AbstractBossState
{
    private float _elapsed;
    private bool _hitboxesEnabledByFallback;
    private bool _damageWindowOpened;

    public BossHeavyAttackState(BossContext context, BossStateMachine stateMachine) : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.StopMovement();
        Boss.FaceTargetImmediately();

        if (!Boss.TryStartHeavyAttack())
        {
            StateMachine.ChangeState(Boss.AggroState, "Heavy attack was on cooldown");
            return;
        }

        _elapsed = 0f;
        _hitboxesEnabledByFallback = false;
        _damageWindowOpened = false;
    }

    public override void Exit()
    {
        Boss.DisableDamageHitboxes();
    }

    public override void Tick()
    {
        if (TryEnterDeath())
            return;

        if (TryEnterEnrage())
            return;

        _elapsed += Time.deltaTime;
        TickDamageWindow(Boss.HeavyAttackDuration, Boss.HeavyAttackDamageWindowStart, Boss.HeavyAttackDamageWindowEnd);

        if (Boss.ConsumeAttackAnimationFinished() || _elapsed >= Boss.HeavyAttackDuration)
            StateMachine.ChangeState(Boss.SelectMovementOrIdleState(), "Boss finished heavy attack");
    }

    public override void FixedTick()
    {
        FaceTarget(Time.fixedDeltaTime);
    }

    private void TickDamageWindow(float duration, float startNormalized, float endNormalized)
    {
        float normalizedTime = duration > 0f ? _elapsed / duration : 1f;

        if (!_damageWindowOpened && !Boss.HasDamageWindowOpenedThisAttack && normalizedTime >= startNormalized)
        {
            Boss.EnableDamageHitboxes();
            _hitboxesEnabledByFallback = true;
            _damageWindowOpened = true;
        }

        if (_hitboxesEnabledByFallback && normalizedTime >= endNormalized)
        {
            Boss.DisableDamageHitboxes();
            _hitboxesEnabledByFallback = false;
        }
    }
}
