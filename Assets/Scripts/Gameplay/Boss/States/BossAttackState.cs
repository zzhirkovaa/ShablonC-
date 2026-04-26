using UnityEngine;

public sealed class BossAttackState : AbstractBossState
{
    private float _elapsed;
    private bool _hitboxesEnabledByFallback;
    private bool _damageWindowOpened;

    public BossAttackState(BossContext context, BossStateMachine stateMachine) : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.StopMovement();
        Boss.FaceTargetImmediately();

        if (!Boss.TryStartAttack())
        {
            StateMachine.ChangeState(Boss.AggroState, "Normal attack was on cooldown");
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
        TickDamageWindow(Boss.AttackDuration, Boss.AttackDamageWindowStart, Boss.AttackDamageWindowEnd);

        if (Boss.ConsumeAttackAnimationFinished() || _elapsed >= Boss.AttackDuration)
            StateMachine.ChangeState(Boss.SelectMovementOrIdleState(), "Boss finished normal attack");
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
