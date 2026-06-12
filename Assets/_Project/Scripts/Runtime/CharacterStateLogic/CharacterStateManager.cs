using UnityEngine;
using YFramework.HFSM;

public class CharacterStateManager : MonoBehaviour
{
    private StateMachine _characterState;
    
    private void Start()
    {
        _characterState = HFSMManager.Instance.Create("CharacterState");

        _characterState.AddState<IdleState>();

        HFSMManager.Instance.Register(_characterState);
    }

    
    private void Update()
    {
        
    }
}
