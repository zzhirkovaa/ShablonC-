using UnityEngine;

public class BossHeavyAttackState : AbstractBossState
{
    private float _elapsed;
    private float _duration;
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
        _duration = Boss.GetAttackDuration(BossAttackType.HeavyHands);
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
        TickDamageWindow(_duration, Boss.HeavyAttackDamageWindowStart, Boss.HeavyAttackDamageWindowEnd);

        if (Boss.ConsumeAttackAnimationFinished() || _elapsed >= _duration)
        {
            IBossState nextState = Boss.SelectMovementOrIdleState();
            Boss.PrepareAnimatorForPostAttack(nextState);
            StateMachine.ChangeState(nextState, "Boss finished heavy attack");
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
