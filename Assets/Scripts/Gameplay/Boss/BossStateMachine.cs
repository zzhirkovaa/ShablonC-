using UnityEngine;

public sealed class BossStateMachine
{
    private readonly string _ownerName;

    public BossStateMachine(BossContext context, string ownerName)
    {
        Context = context;
        _ownerName = ownerName;
    }

    public BossContext Context { get; }
    public IBossState CurrentState { get; private set; }

    public void ChangeState(IBossState nextState, string reason = "No reason provided")
    {
        if (nextState == null || ReferenceEquals(CurrentState, nextState))
            return;

        string previousStateName = CurrentState?.GetType().Name ?? "None";
        string nextStateName = nextState.GetType().Name;
        Debug.Log($"[{_ownerName}] Boss state change: {previousStateName} -> {nextStateName}. Reason: {reason}");

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
