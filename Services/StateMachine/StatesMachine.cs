using System;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Helpers.Services
{
/// <summary>
/// Runs transitions one by one: Enter requested during a transition waits in a queue (last request wins).
/// Failed transition is logged and falls back to the fallback state.
/// </summary>
public sealed class StatesMachine
{
#region Fields

    public string StateName => _currentState?.GetType().Name;

    readonly Dictionary<Type, IState> _states;
    readonly IStateEnter _fallbackState;
    
    bool _isTransitioning;
    IState _currentState;
    IState _pendingState;
    CancellationTokenSource _stateCts;
    Func<CancellationToken, UniTask> _pendingEnter;

#endregion

#region Public methods

    public StatesMachine(IStateEnter fallbackState, params IState[] states)
    {
        _states = new Dictionary<Type, IState>();
        _fallbackState = fallbackState;

        AddStates(states);
        AddStates(new IState[] {fallbackState});
    }

    public void Enter<TState>() where TState : class, IStateEnter
    {
        var state = (TState)_states[typeof(TState)];
        Schedule(state, state.Enter);
    }

    public void Enter<TState, TPayload>(TPayload payload) where TState : class, IStateEnterPayload<TPayload>
    {
        var state = (TState)_states[typeof(TState)];
        Schedule(state, ct => state.Enter(payload, ct));
    }

#endregion

#region Private methods

    void AddStates(IState[] states)
    {
        foreach (var state in states)
        {
            if (_states.ContainsKey(state.GetType()))
                continue;

            state.StatesMachine = this;
            _states.Add(state.GetType(), state);
        }
    }

    void Schedule(IState state, Func<CancellationToken, UniTask> enter)
    {
        _pendingState = state;
        _pendingEnter = enter;

        if (!_isTransitioning)
            RunTransitions().Forget();
    }

    async UniTaskVoid RunTransitions()
    {
        _isTransitioning = true;

        try
        {
            while (_pendingState != null)
            {
                var state = _pendingState;
                var enter = _pendingEnter;
                _pendingState = null;
                _pendingEnter = null;

                try
                {
                    await ChangeState(state, enter);
                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    Debug.LogException(e);

                    if (state != _fallbackState)
                        Schedule(_fallbackState, _fallbackState.Enter);
                }
            }
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    async UniTask ChangeState(IState state, Func<CancellationToken, UniTask> enter)
    {
        _stateCts?.Cancel();
        _stateCts?.Dispose();
        _stateCts = null;

        if (_currentState != null)
            await _currentState.Exit();

        _stateCts = new CancellationTokenSource();
        _currentState = state;
        await enter(_stateCts.Token);
    }

#endregion
}
}