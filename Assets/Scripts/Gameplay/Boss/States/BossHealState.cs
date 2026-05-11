using UnityEngine;

public sealed class BossHealState : AbstractBossState
{
    private float _timer;
    private float _healPerSecond;

    public BossHealState(BossContext context, BossStateMachine stateMachine) : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.DisableDamageHitboxes();
        Boss.BeginHealAnimation();
        Context.ClearEnrageRequest();

        _timer = Boss.HealDuration;

        if (Context.Health == null || Boss.HealDuration <= 0f)
        {
            FinishHeal();
            return;
        }

        float targetHeal = Boss.HealToFull
            ? Context.Health.MaxHealth - Context.Health.CurrentHealth
            : Boss.HealAmount;

        _healPerSecond = Mathf.Max(0f, targetHeal) / Boss.HealDuration;
    }

    public override void Tick()
    {
        if (TryEnterDeath())
            return;

        if (_timer <= 0f)
        {
            FinishHeal();
            return;
        }

        float deltaTime = Time.deltaTime;
        _timer -= deltaTime;

        if (!Boss.HealToFull)
            Boss.Heal(_healPerSecond * deltaTime);

        if (_timer <= 0f)
            FinishHeal();
    }

    public override void FixedTick()
    {
        Boss.StopMovement();
    }

    private void FinishHeal()
    {
        if (Boss.HealToFull)
            Boss.RestoreHealthToFull();
        else if (Boss.HealDuration <= 0f)
            Boss.Heal(Boss.HealAmount);

        Context.MarkHealed();
        Context.ClearEnrageRequest();
        StateMachine.ChangeState(BossStateType.Idle, "Boss finished healing");
    }
}
