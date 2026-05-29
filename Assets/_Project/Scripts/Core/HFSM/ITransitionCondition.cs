using UnityEngine;


namespace YFramework.HFSM
{
    /// <summary>
    /// 状态转换条件接口
    /// </summary>
    public interface ITransitionCondition
    {
        bool Evaluate();
    }
}
