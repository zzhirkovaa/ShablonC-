using UnityEngine;

public sealed class EnemyStateMachine
{
    private readonly string _ownerName;

    public EnemyStateMachine(EnemyContext context, string ownerName)
    {
        Context = context;
        _ownerName = ownerName;
    }

    public EnemyContext Context { get; }
    public IEnemyState CurrentState { get; private set; }

    public void ChangeState(IEnemyState nextState, string reason = "No reason provided")
    {
        if (nextState == null || ReferenceEquals(CurrentState, nextState))
            return;

        string previousStateName = CurrentState?.GetType().Name ?? "None";
        string nextStateName = nextState.GetType().Name;

        Debug.Log($"[{_ownerName}] State change: {previousStateName} -> {nextStateName}. Reason: {reason}");

        CurrentState?.Exit();
        CurrentState = nextState;
        CurrentState.Enter();
    }

    public void Tick()
    {
        CurrentState?.Tick();
    }

    public void FixedTick()
    {
        CurrentState?.FixedTick();
    }
}
