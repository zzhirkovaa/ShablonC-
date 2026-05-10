using UnityEngine;

public sealed class BossAttackState : AbstractBossState
{
    private float _elapsed;
    private float _duration;
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
        _duration = Boss.GetAttackDuration(BossAttackType.Kick);
        _hitboxesEnabledByFallback = false;
        _damageWindowOpened = false;
    }

    public override void Exit()
    {
        Boss.DisableDamageHitboxes();
        Boss.ClearActiveElementEffects();
    }

    public override void Tick()
    {
        if (TryEnterDeath())
            return;

        if (TryEnterEnrage())
            return;

        _elapsed += Time.deltaTime;
        TickDamageWindow(_duration, Boss.AttackDamageWindowStart, Boss.AttackDamageWindowEnd);

        if (Boss.ConsumeAttackAnimationFinished() || _elapsed >= _duration)
        {
            IBossState nextState = Boss.SelectMovementOrIdleState();
            Boss.PrepareAnimatorForPostAttack(nextState);
            StateMachine.ChangeState(nextState, "Boss finished normal attack");
        }
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
            Boss.OpenAttackDamageWindow();
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
