using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Helpers.Services
{
public sealed class StatesMachine
{
    public string StateName => _currentState.GetType().Name;
    
    readonly Dictionary<Type, IState> _states;
    IState _currentState { get; set; }

    public StatesMachine(params IState[] states)
    {
        _states = new Dictionary<Type, IState>();

        AddStates(states);
    }

    public void AddStates(IState[] states)
    {
        foreach (var state in states)
        {
            if (_states.ContainsKey(state.GetType()))
                continue;

            state.StatesMachine = this;
            _states.Add(state.GetType(), state);
        }
    }

    public async UniTask Enter<TState>() where TState : class, IStateEnter
    {
        var state = await ChangeState<TState>();
        await state.Enter();
    }

    public async UniTask Enter<TState, TPayload>(TPayload payload) where TState : class, IStateEnterPayload<TPayload>
    {
        var state = await ChangeState<TState>();
        await state.Enter(payload);
    }

    async UniTask<TState> ChangeState<TState>() where TState : class, IState
    {
        if (_currentState != null)
            await _currentState.Exit();

        var state = _states[typeof(TState)] as TState;
        _currentState = state;
        return state;
    }
}
}