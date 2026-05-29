using UnityEngine;

namespace YFramework.HFSM
{
    public abstract class StateBase : IState
    {
        public virtual string Name => GetType().Name;

        // 持有所属状态机的引用
        public StateMachine OwnerMachine { get; private set; }

        public void SetOwner(StateMachine ownerMachine)
        {
            OwnerMachine = ownerMachine;
        }
        
        public virtual void OnEnter()
        {
        }

        public virtual void OnUpdate(float deltaTime)
        {
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
        }

        public void OnExit()
        {
        }
    }
}