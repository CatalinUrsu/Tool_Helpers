﻿using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Helpers.StateMachine
{
[Serializable]
public class StatesMachine
{
    readonly Dictionary<Type, IState> _states;
    protected IState currentState { get; set; }

    public StatesMachine(params IState[] states)
    {
        _states = new Dictionary<Type, IState>();
        
        AddStates(states);
    }
    
    public virtual async UniTask Enter<TState>() where TState : class, IStateEnter
    {
        var state = await ChangeState<TState>();
        await state.Enter();
    }

    public virtual async UniTask Enter<TState, TPayload>(TPayload payload) where TState : class, IStateEnterPayload<TPayload>
    {
        var state = await ChangeState<TState>();
        await state.Enter(payload);
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

    protected virtual async UniTask<TState> ChangeState<TState>() where TState : class, IState
    {
        if (currentState != null)
            await currentState.Exit();

        var state = GetState<TState>();
        currentState = state;
        return state;
    }

    protected virtual TState GetState<TState>() where TState : class, IState => _states[typeof(TState)] as TState;
}
}