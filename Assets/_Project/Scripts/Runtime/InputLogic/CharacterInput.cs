using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;

    public InputCommandBuffer Buffer { get; } = new InputCommandBuffer(0.2f);

    public Vector2 MoveDir => _inputReader.MoveDirection;

    private void OnEnable()
    {
        _inputReader.OnSkillPressed += OnSkillPressed;
    }
    private readonly List<string> _popupStack = new();
    private void OnSkillPressed(int slot)
    {
        Buffer.Push(CommandType.Skill, slot);
    }

    private void Update()
    {
        if (Buffer.TryConsume(out var cmd))
        {
            Debug.Log("consume buffer:" + cmd.Slot);
        }
    }

    private void OnDisable()
    {
        _inputReader.OnSkillPressed -= OnSkillPressed;
    }
}
