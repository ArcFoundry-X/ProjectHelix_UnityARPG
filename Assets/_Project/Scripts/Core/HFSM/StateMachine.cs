using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace YFramework.HFSM
{
    public class StateMachine : IState
    {
        public string Name { get; }

        // 黑板：所有子状态共享，根状态机的黑板向下透传
        public Blackboard Blackboard { get; private set; }

        private StateMachine _parent;

        private IState _currentState;

        private string _defaultStateName;
        
        // 注册的所有子状态 name -> IState
        private readonly Dictionary<string, IState> _states = new();
        //黑板数据
        private readonly Dictionary<string, Object> _blackboard = new();
        
        // 转换列表（包括 AnyState 转换）
        private readonly List<Transition> _transitions = new();
        private readonly List<Transition> _anyTransitions = new();
        
        public StateMachine(string name, Blackboard blackboard = null)
        {
            Name = name;
            Blackboard = blackboard ?? new Blackboard();
        }

        public void Run<TState>() where TState : StateBase
        {
            var stateType = typeof(TState);
            var stateName = stateType.FullName;
            
            Run(stateName);
        }

        public void Run(string entryState)
        {
            if (_states.TryGetValue(entryState, out _currentState))
            {
                _currentState.OnEnter();
            }
            else
            {
                throw new Exception($"Not found entry state: {entryState}");
            }
        }
        
        /// <summary>
        /// 添加叶子状态
        /// </summary>
        public StateMachine AddState<TNode>() where TNode : StateBase
        {
            var nodeType = typeof(TNode);
            var stateNode = Activator.CreateInstance(nodeType) as StateBase;

            return AddState(stateNode);
        }
        
        public StateMachine AddState(StateBase state, bool isDefault = false)
        {
            state.SetOwner(this);
            _states[state.Name] = state;
            if (isDefault || _defaultStateName == null)
                _defaultStateName = state.Name;
            return this;
        }

        /// <summary>
        /// 添加子状态机（HFSM 层级嵌套的核心）
        /// </summary>
        public StateMachine AddSubMachine(StateMachine subMachine, bool isDefault = false)
        {
            subMachine._parent = this;
            subMachine.Blackboard = Blackboard;
            _states[subMachine.Name] = subMachine;
            if (isDefault || _defaultStateName == null)
                _defaultStateName = subMachine.Name;
            return this;
        }

        /// <summary>
        /// 添加普通转换
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public Transition AddTransition(string from, string to)
        {
            var transition = new Transition(from, to);
            _transitions.Add(transition);
            return transition;
        }

        /// <summary>
        /// 添加AnyState转换，任意状态都能触发
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public Transition AddAnyTransition(string to)
        {
            var transition = new Transition(null, to);
            _anyTransitions.Add(transition);
            return transition;
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        public void ChangeState<T>() where T : StateBase
        {
            var nodeType = typeof(T);
            var stateName = nodeType.FullName;
            ChangeState(stateName);
        }
        
        
        public void ChangeState(string stateName)
        {
            if (!_states.TryGetValue(stateName, out var targetState))
            {
                Debug.LogError($"{stateName} not exit");
                return;
            }
            
            _currentState.OnExit();
            _currentState = targetState;
            _currentState.OnEnter();
        }

        /// <summary>
        /// 设置黑板数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetBlackboardValue(string key, Object value)
        {
            _blackboard[key] = value;
        }

        /// <summary>
        /// 获取黑板数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Object TryGetBlackboardValue(string key)
        {
            _blackboard.TryGetValue(key, out var value);
            return value;
        }
        
        public void OnEnter()
        {
            if (_defaultStateName == null)
            {
                Debug.LogError($"{Name} not set default status");
                return;
            }
            
            ChangeState(_defaultStateName);
        }

        public void OnUpdate(float deltaTime)
        {
            //先检查转换条件
            CheckTransitions();
            //再Tick当前子状态
            _currentState?.OnUpdate(deltaTime);
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            _currentState?.OnFixedUpdate(fixedDeltaTime);
        }

        public void OnExit()
        {
            _currentState?.OnExit();
            _currentState = null;
        }

        private void CheckTransitions()
        {
            // 1. 优先检查 AnyState 转换
            foreach (var t in _anyTransitions)
            {
                if (t.ToState == _currentState.Name) continue; // 已经在目标状态
                if (t.CanTransition())
                {
                    PerformTransition(t.ToState);
                    return;
                }
            }
 
            // 2. 检查当前状态的普通转换
            foreach (var t in _transitions)
            {
                if (t.FromState != _currentState.Name) continue;
                if (t.CanTransition())
                {
                    PerformTransition(t.ToState);
                    return;
                }
            }
        }

        private void PerformTransition(string toStateName)
        {
            if (!_states.ContainsKey(toStateName))
            {
                Debug.LogError($"target state {toStateName} not exit");
                return;
            }
            
            _currentState?.OnExit();
            _currentState = _states[toStateName];
            _currentState.OnEnter();
        }

        public string CurrentStateName => _currentState.Name;

        public bool IsInState(string stateName) => _currentState.Name == stateName;

        /// <summary>
        /// 获取完整的层级路径，如 "Root/Combat/Attack"
        /// </summary>
        public string GetFullPath()
        {
            var sub = _currentState is StateMachine sm ? $"/{sm.GetFullPath()}" : $"/{_currentState.Name}";
            return Name + sub;
        }
    }
}