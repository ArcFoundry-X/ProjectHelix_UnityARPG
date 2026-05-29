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

        public StateMachine Create(string machineName)
        {
            StateMachine machine = new StateMachine(machineName);
            
            
            return machine;
        }
        
        public void Register(StateMachine stateMachine)
        {
            var entry = new AgentEntry() { StateMachine = stateMachine };
            _agents.Add(entry);

            stateMachine.OnEnter();
        }

        public void UnRegister(StateMachine stateMachine)
        {
            var entry = _agents.Find(e => e.StateMachine == stateMachine);
            if (entry == null) return;

            stateMachine.OnExit();

            _agents.Remove(entry);
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

        private void Update()
        {
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