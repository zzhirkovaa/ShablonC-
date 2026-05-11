using System;
using UnityEngine;

public sealed class BossStateMachine
{
    private readonly BossStateFactory _factory;
    private readonly string _ownerName;
    private IBossState _currentState;

    public BossStateMachine(BossContext context, string ownerName)
    {
        Context = context;
        _ownerName = ownerName;
        _factory = new BossStateFactory(context, this);
    }

    public BossContext Context { get; }

    public void ChangeState(BossStateType nextStateType, string reason = "No reason provided")
    {
        IBossState previousState = _currentState;
        string previousStateName = previousState?.GetType().Name ?? "None";

        previousState?.Exit();
        if (previousState is IDisposable disposableState)
            disposableState.Dispose();

        _currentState = _factory.Create(nextStateType);

        Debug.Log($"[{_ownerName}] Boss state change: {previousStateName} -> {_currentState.GetType().Name}. Reason: {reason}");
        _currentState.Enter();
    }

    public void Tick()
    {
        _currentState?.Tick();
    }

    public void FixedTick()
    {
        _currentState?.FixedTick();
    }

    public bool IsCurrentState<TState>() where TState : class, IBossState
    {
        return _currentState is TState;
    }
}
