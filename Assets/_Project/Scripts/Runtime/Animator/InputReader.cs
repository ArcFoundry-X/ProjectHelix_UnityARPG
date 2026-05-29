using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, WindowsInputSystem.IGameplayActions
{
    private WindowsInputSystem _inputSystem;
    
    //连续输入：每帧轮询读取最新值
    public Vector2 MoveDirection { get; private set; }
    public Vector2 AimScreenPos { get; private set; }
    
    //离散输入：以事件抛出
    public event Action<int> OnSkillPressed;
    public event Action<int> OnSkillReleased;
    public event Action OnDodge;

    private void OnEnable()
    {
        if (_inputSystem == null)
        {
            _inputSystem = new WindowsInputSystem();
            _inputSystem.Gameplay.SetCallbacks(this);
        }
        
        _inputSystem.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _inputSystem.Gameplay.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveDirection = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        
    }

    public void OnSkill1(InputAction.CallbackContext context)
    {
        HandleSkill(1, context);
    }

    public void OnSkill2(InputAction.CallbackContext context)
    {
        HandleSkill(2, context);
    }

    public void OnSkill3(InputAction.CallbackContext context)
    {
        HandleSkill(3, context);
    }

    void HandleSkill(int slot, InputAction.CallbackContext ctx)
    {
        if(ctx.performed) OnSkillPressed?.Invoke(slot);
        else if(ctx.canceled) OnSkillReleased?.Invoke(slot);
    }
    
    // public void OnDodge(InputAction.CallbackContext ctx)
    // {
    //     if (ctx.performed) OnDodge?.Invoke();
    // }
}
