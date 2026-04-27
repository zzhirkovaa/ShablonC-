using UnityEngine;
using UnityEngine.AI;

public sealed class BossContext
{
    public BossContext(BossController controller, Transform transform, NavMeshAgent agent, Animator animator, EnemyHealth health)
    {
        Controller = controller;
        Transform = transform;
        Agent = agent;
        Animator = animator;
        Health = health;
    }

    public BossController Controller { get; }
    public Transform Transform { get; }
    public NavMeshAgent Agent { get; }
    public Animator Animator { get; }
    public EnemyHealth Health { get; }
    public Transform Target { get; private set; }
    public PlayerHealth TargetHealth { get; private set; }

    public bool HasTarget => Target != null;
    public bool IsDead => Health != null && Health.IsDead;
    public bool IsEnraged { get; private set; }
    public bool HasEnteredEnrage { get; private set; }
    public bool EnrageRequested { get; private set; }
    public bool IsPeacefulMode { get; private set; }
    public bool WasProvokedByPlayer { get; private set; }
    public bool HasHealed { get; private set; }
    public float AttackSpeedMultiplier { get; private set; } = 1f;

    public float AttackCooldownRemaining { get; private set; }
    public float HeavyAttackCooldownRemaining { get; private set; }

    public float DistanceToTarget
    {
        get
        {
            if (!HasTarget)
                return float.PositiveInfinity;

            return Vector3.Distance(Transform.position, Target.position);
        }
    }

    public Vector3 DirectionToTarget
    {
        get
        {
            if (!HasTarget)
                return Vector3.zero;

            Vector3 direction = Target.position - Transform.position;
            direction.y = 0f;
            return direction.normalized;
        }
    }

    public bool HasDetectedTarget => HasTarget && DistanceToTarget <= Controller.DetectionRadius;
    public bool HasLostTarget => !HasTarget || DistanceToTarget > Controller.LoseTargetRadius;
    public bool IsTargetInAttackRange => HasTarget && DistanceToTarget <= Controller.AttackRange;
    public bool CanUseAttack => AttackCooldownRemaining <= 0f;
    public bool CanUseHeavyAttack => HeavyAttackCooldownRemaining <= 0f;
    public bool IsTargetHealthLow => TargetHealth != null && TargetHealth.MaxHealth > 0f && TargetHealth.CurrentHealth / TargetHealth.MaxHealth < Controller.PlayerLowHealthThreshold;
    public bool ShouldEnterEnrage => !HasEnteredEnrage && (EnrageRequested || IsBossHealthAtOrBelow(Controller.EnrageHealthThreshold));
    public bool CanEnterHeal => Controller.CanHealOnce && !HasHealed && IsBossHealthAtOrBelow(Controller.HealThreshold);

    public void SetTarget(Transform target)
    {
        Target = target;
        TargetHealth = target != null ? target.GetComponentInParent<PlayerHealth>() : null;
    }

    public void SetPeacefulMode(bool isPeacefulMode)
    {
        IsPeacefulMode = isPeacefulMode;
    }

    public void MarkProvokedByPlayer()
    {
        WasProvokedByPlayer = true;
    }

    public void RequestEnrage()
    {
        if (!HasEnteredEnrage)
            EnrageRequested = true;
    }

    public void ClearEnrageRequest()
    {
        EnrageRequested = false;
    }

    public void MarkHealed()
    {
        HasHealed = true;
    }

    public void Tick(float deltaTime)
    {
        AttackCooldownRemaining = Mathf.Max(0f, AttackCooldownRemaining - deltaTime);
        HeavyAttackCooldownRemaining = Mathf.Max(0f, HeavyAttackCooldownRemaining - deltaTime);
    }

    public void TriggerAttackCooldown()
    {
        AttackCooldownRemaining = Controller.AttackCooldown / AttackSpeedMultiplier;
    }

    public void TriggerHeavyAttackCooldown()
    {
        HeavyAttackCooldownRemaining = Controller.HeavyAttackCooldown / AttackSpeedMultiplier;
    }

    public void EnterEnrage(float attackSpeedMultiplier)
    {
        if (HasEnteredEnrage)
            return;

        HasEnteredEnrage = true;
        EnrageRequested = false;
        IsEnraged = true;
        AttackSpeedMultiplier = Mathf.Max(0.1f, attackSpeedMultiplier);
    }

    private bool IsBossHealthAtOrBelow(float threshold)
    {
        if (Health == null || Health.MaxHealth <= 0f || Health.IsDead)
            return false;

        return Health.CurrentHealth / Health.MaxHealth <= threshold;
    }
}
