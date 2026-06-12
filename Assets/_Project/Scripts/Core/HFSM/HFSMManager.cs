using System.Collections.Generic;
using UnityEngine;

namespace YFramework.HFSM
{
    /// <summary>
    /// 全局类，统一驱动所有 StateMachineRunner
    /// </summary>
    public class HFSMManager : MonoSingleton<HFSMManager>
    {
        private class AgentEntry
        {
            public StateMachine StateMachine;
            public bool IsPaused;
        }

        private readonly List<AgentEntry> _agents = new();
        private readonly List<AgentEntry> _pendingAdd = new();
        private readonly List<StateMachine> _pendingRemove = new();

        public StateMachine Create(string machineName)
        {
            return new StateMachine(machineName);
        }

        public void Register(StateMachine stateMachine)
        {
            stateMachine.OnEnter();
            _pendingAdd.Add(new AgentEntry { StateMachine = stateMachine });
        }

        public void UnRegister(StateMachine stateMachine)
        {
            stateMachine.OnExit();
            _pendingRemove.Add(stateMachine);
        }

        // ── 单个 machine 控制 ──────────────────────────────────────

        public void Pause(StateMachine stateMachine)
        {
            var entry = _agents.Find(e => e.StateMachine == stateMachine);
            if (entry != null) entry.IsPaused = true;
        }

        public void Resume(StateMachine stateMachine)
        {
            var entry = _agents.Find(e => e.StateMachine == stateMachine);
            if (entry != null) entry.IsPaused = false;
        }

        // ── Tick ─────────────────────────────────────────────────

        private void FlushPending()
        {
            if (_pendingAdd.Count > 0)
            {
                _agents.AddRange(_pendingAdd);
                _pendingAdd.Clear();
            }
            if (_pendingRemove.Count > 0)
            {
                foreach (var sm in _pendingRemove)
                    _agents.RemoveAll(e => e.StateMachine == sm);
                _pendingRemove.Clear();
            }
        }

        private void Update()
        {
            FlushPending();
            foreach (var entry in _agents)
            {
                if (entry.IsPaused) continue;
                entry.StateMachine.OnUpdate(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            foreach (var entry in _agents)
            {
                if (entry.IsPaused) continue;
                entry.StateMachine.OnFixedUpdate(Time.fixedDeltaTime);
            }
        }
        
        // ── 全局强制切换（过场、剧情用）──────────────────────────
 
        /// <summary>强制某个 Runner 的状态机跳到指定状态</summary>
        public void ForceTransition(StateMachine runner, string stateName)
        {
            runner.ChangeState(stateName);
        }
    }
}