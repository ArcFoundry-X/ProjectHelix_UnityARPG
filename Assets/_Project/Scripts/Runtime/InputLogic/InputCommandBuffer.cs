using UnityEngine;

public enum CommandType
{
    Skill,
    Doge
}

public struct BufferedCommand
{
    public CommandType CurrentCommandType;
    public int Slot;
    public float InvokeTime;
}

/// <summary>
/// ARPG游戏中，玩家期望的永远是最新的操作
/// </summary>
public class InputCommandBuffer
{
    private BufferedCommand _currentCommand;
    private bool _isValid;
    private readonly float _interval;   //缓冲时长， ARPG一般0.15~0.3s

    public InputCommandBuffer(float interval) => _interval = interval;

    public void Push(CommandType type, int slot = 0)
    {
        _currentCommand = new BufferedCommand()
        {
            CurrentCommandType = type,
            Slot = slot,
            InvokeTime = Time.time
        };
        _isValid = true;
    }

    public bool TryConsume(out BufferedCommand cmd)
    {
        cmd = _currentCommand;
        if (!_isValid) return false;
        if (Time.time - _currentCommand.InvokeTime > _interval)     //数据过期
        {
            _isValid = false;
            return false;
        }

        _isValid = false;
        return true;
    }

    public void Clear()
    {
        _isValid = false;
    }
}
