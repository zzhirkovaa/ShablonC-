using UnityEngine;

public sealed class BossStateMachine
{
    private readonly string _ownerName;

    public BossStateMachine(string ownerName)
    {
        _ownerName = ownerName;
    }

    public IBossState CurrentState { get; private set; }

    // State machine entry point for every boss transition.
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
}
