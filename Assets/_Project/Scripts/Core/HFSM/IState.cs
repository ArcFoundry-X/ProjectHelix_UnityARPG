using UnityEngine;

namespace YFramework.HFSM
{
    public interface IState
    {
        string Name { get; }

        void OnEnter();

        void OnUpdate(float deltaTime);

        void OnFixedUpdate(float fixedDeltaTime);

        void OnExit();
    }
}
