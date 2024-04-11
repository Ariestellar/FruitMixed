using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Services;
using UnityEngine;
using YG;

namespace CodeBase.Infrastructure.States
{
    public class GameStateMachine
    {
        private Dictionary<Type, IExitableState> _states;
        private IExitableState _activeState;
        private bool _isDebug;
        public bool IsDebug { get => _isDebug; }

        public GameStateMachine(SceneLoader sceneLoader, ICoroutineRunner coroutineRunner, YandexGame yandexGame, AllServices allServices, bool isDebug) {
            _isDebug = isDebug;
            _states = new Dictionary<Type, IExitableState> {
                [typeof(BootstrapState)] = new BootstrapState(this, coroutineRunner, allServices),
                [typeof(LoadGameSceneState)] = new LoadGameSceneState(this, allServices, sceneLoader),
                [typeof(GameRunnerState)] = new GameRunnerState(this, coroutineRunner, allServices),
                [typeof(FinishState)] = new FinishState(this, allServices)
            };
        }

        public void Enter<TState>() where TState : class, IState { 
            IState state = ChangeState<TState>();
            if(_isDebug) Debug.Log("Вход в стейт: " + state.ToString());
            state.Enter();            
        }

        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload> {
            TState state = ChangeState<TState>();
            if(_isDebug) Debug.Log("Вход в стейт: " + state.ToString());
            state.Enter(payload);
        }

        private TState ChangeState<TState>() where TState : class, IExitableState {
            _activeState?.Exit();      
            TState state = GetState<TState>();
            _activeState = state;      
            return state;
        }

        private TState GetState<TState>() where TState : class, IExitableState => 
            _states[typeof(TState)] as TState;
    }
}